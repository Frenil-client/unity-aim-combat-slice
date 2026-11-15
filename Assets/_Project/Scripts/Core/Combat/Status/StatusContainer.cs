using System;
using System.Collections.Generic;

namespace AimCombat.Core.Combat
{
    public enum StatusEventKind
    {
        /// <summary>새로 걸렸다.</summary>
        Applied,
        /// <summary>중첩이 하나 늘었거나, 가득 찬 상태에서 가장 먼저 끝날 중첩을 새로 채웠다.</summary>
        Stacked,
        /// <summary>남은 시간을 새로 채웠다.</summary>
        Refreshed,
        /// <summary>면역이거나 이미 걸려 있어 걸리지 않았다. 게이지형 대상은 Amount 만큼 저항 게이지가 깎였다.</summary>
        Resisted,
        /// <summary>지속 피해 한 틱. Amount 가 들어간 피해다.</summary>
        Ticked,
        /// <summary>시간이 다 돼 풀렸다.</summary>
        Expired,
        /// <summary>대상이 쓰러졌거나 스킬이 지워서 없어졌다.</summary>
        Removed,
    }

    /// <summary>상태효과 이벤트. UI 와 연출이 구독한다(SPEC 3.3).</summary>
    public readonly struct StatusEvent
    {
        public readonly StatusEventKind Kind;
        public readonly Combatant Target;
        public readonly StatusDef Def;
        public readonly Combatant Caster;
        public readonly Fixed Amount;

        public StatusEvent(StatusEventKind kind, Combatant target, StatusDef def, Combatant caster, Fixed amount)
        {
            Kind = kind;
            Target = target;
            Def = def;
            Caster = caster;
            Amount = amount;
        }
    }

    /// <summary>걸려 있는 상태효과 하나. Stack 규칙은 중첩마다 남은 시간을 따로 가진다.</summary>
    public sealed class StatusInstance
    {
        internal readonly List<Fixed> stackTimes = new List<Fixed>();
        internal Fixed tickTimer;

        internal StatusInstance(StatusDef def, Combatant caster)
        {
            Def = def;
            Caster = caster;
        }

        public StatusDef Def { get; }

        /// <summary>시전자. 지속 피해만 시전자별로 따로 두고, 나머지 규칙에서는 처음 건 쪽이 남는다.</summary>
        public Combatant Caster { get; }

        public int Stacks => stackTimes.Count;

        /// <summary>가장 늦게 끝나는 중첩의 남은 시간.</summary>
        public Fixed TimeLeft
        {
            get
            {
                Fixed longest = Fixed.Zero;
                foreach (Fixed t in stackTimes) if (t > longest) longest = t;
                return longest;
            }
        }

        /// <summary>능력치에 더하는 양. 중첩형은 중첩 수만큼 곱한다.</summary>
        public Fixed Value => Def.Rule == StatusRule.Stack ? Def.Value * Fixed.FromInt(Stacks) : Def.Value;
    }

    /// <summary>
    /// 한 전투 단위에 걸린 상태효과 목록(SPEC 3.3). 거는 규칙, 시간 진행, 능력치 합산, 이벤트를 맡는다.
    /// - 중첩형: 정의 하나에 인스턴스 하나. 최대 중첩까지 쌓고 중첩마다 시간을 센다.
    /// - 갱신형, 최댓값형: 정의 하나에 인스턴스 하나. 다시 걸리면 시간만 채운다. 합산할 때 최댓값형은 같은 능력치 중 가장 큰 값만 쓴다.
    /// - 지속 피해: 정의와 시전자 쌍마다 인스턴스 하나. 다시 걸리면 시간만 채우고 틱 주기는 이어 간다.
    /// - 행동 불가: 걸린 동안과 풀린 뒤 면역 시간 동안 같은 효과가 걸리지 않는다. 게이지형 대상은 걸리지 않고 저항 게이지가 깎인다.
    /// 시간은 Fixed 초로 받는다. 프레임 시간을 넘길 때는 TickAccumulator 로 잔여분을 보존한다.
    /// </summary>
    public sealed class StatusContainer
    {
        private readonly Combatant owner;
        private readonly List<StatusInstance> active = new List<StatusInstance>();
        private readonly Dictionary<StatusDef, Fixed> immunity = new Dictionary<StatusDef, Fixed>();
        private readonly List<StatusDef> immunityKeys = new List<StatusDef>();

        internal StatusContainer(Combatant owner)
        {
            this.owner = owner;
        }

        public IReadOnlyList<StatusInstance> Active => active;

        /// <summary>행동 불가 효과가 걸려 있으면 true.</summary>
        public bool IsControlled
        {
            get
            {
                foreach (StatusInstance s in active) if (s.Def.Rule == StatusRule.Control) return true;
                return false;
            }
        }

        public event Action<StatusEvent> Changed;

        /// <summary>효과를 건다. 걸리거나 시간, 중첩이 바뀌었으면 true, 면역이나 저항으로 걸리지 않았으면 false.</summary>
        public bool Apply(StatusDef def, Combatant caster)
        {
            if (def == null || !owner.IsAlive) return false;

            switch (def.Rule)
            {
                case StatusRule.Stack:
                {
                    StatusInstance instance = Find(def, null);
                    if (instance == null)
                    {
                        Add(def, caster);
                        return true;
                    }
                    if (instance.Stacks < def.StackLimit)
                    {
                        instance.stackTimes.Add(def.Duration);
                    }
                    else
                    {
                        // 가득 찼다. 가장 먼저 끝날 중첩을 새로 채운다.
                        int shortest = 0;
                        for (int i = 1; i < instance.stackTimes.Count; i++)
                        {
                            if (instance.stackTimes[i] < instance.stackTimes[shortest]) shortest = i;
                        }
                        instance.stackTimes[shortest] = def.Duration;
                    }
                    Raise(StatusEventKind.Stacked, def, caster, Fixed.FromInt(instance.Stacks));
                    return true;
                }

                case StatusRule.Refresh:
                case StatusRule.Highest:
                case StatusRule.Marker:
                {
                    StatusInstance instance = Find(def, null);
                    if (instance == null)
                    {
                        Add(def, caster);
                        return true;
                    }
                    instance.stackTimes[0] = def.Duration;
                    Raise(StatusEventKind.Refreshed, def, caster, Fixed.Zero);
                    return true;
                }

                case StatusRule.DamageOverTime:
                {
                    StatusInstance instance = Find(def, caster);
                    if (instance == null)
                    {
                        Add(def, caster).tickTimer = def.TickInterval;
                        return true;
                    }
                    instance.stackTimes[0] = def.Duration;
                    Raise(StatusEventKind.Refreshed, def, caster, Fixed.Zero);
                    return true;
                }

                case StatusRule.Control:
                {
                    if (owner.ControlResponse == ControlResponse.Gauge)
                    {
                        Fixed cut = owner.AbsorbControl(def);
                        Raise(StatusEventKind.Resisted, def, caster, cut);
                        return false;
                    }
                    if (Find(def, null) != null || immunity.ContainsKey(def))
                    {
                        Raise(StatusEventKind.Resisted, def, caster, Fixed.Zero);
                        return false;
                    }
                    Add(def, caster);
                    return true;
                }
            }
            return false;
        }

        /// <summary>면역과 게이지를 거치지 않고 행동 불가를 건다. 게이지형 대상의 게이지가 다 깎였을 때(그로기) 쓴다.</summary>
        internal void ForceControl(StatusDef def)
        {
            StatusInstance instance = Find(def, null);
            if (instance != null)
            {
                instance.stackTimes[0] = def.Duration;
                Raise(StatusEventKind.Refreshed, def, null, Fixed.Zero);
                return;
            }
            Add(def, null);
        }

        /// <summary>능력치 합계. 중첩형과 갱신형은 더하고, 최댓값형은 같은 능력치 중 가장 큰 값 하나만 더한다.</summary>
        public Fixed Sum(StatKind stat)
        {
            Fixed sum = Fixed.Zero;
            Fixed highest = Fixed.Zero;
            foreach (StatusInstance s in active)
            {
                if (s.Def.Stat != stat) continue;
                if (s.Def.Rule == StatusRule.Highest)
                {
                    if (s.Value > highest) highest = s.Value;
                }
                else if (s.Def.Rule == StatusRule.Stack || s.Def.Rule == StatusRule.Refresh)
                {
                    sum = sum + s.Value;
                }
            }
            return sum + highest;
        }

        public StatusInstance Find(StatusDef def, Combatant caster)
        {
            foreach (StatusInstance s in active)
            {
                if (s.Def != def) continue;
                if (def.Rule == StatusRule.DamageOverTime && s.Caster != caster) continue;
                return s;
            }
            return null;
        }

        /// <summary>시전자와 상관없이 이 효과가 걸려 있는지.</summary>
        public bool Has(StatusDef def)
        {
            if (def == null) return false;
            foreach (StatusInstance s in active) if (s.Def == def) return true;
            return false;
        }

        /// <summary>이 효과의 중첩 수. 걸려 있지 않으면 0. 지속 피해는 시전자별 인스턴스 수다.</summary>
        public int StacksOf(StatusDef def)
        {
            int stacks = 0;
            foreach (StatusInstance s in active) if (s.Def == def) stacks += s.Stacks;
            return stacks;
        }

        /// <summary>이 효과를 시전자와 상관없이 모두 지운다(스킬의 RemoveStatus). 지운 것이 있으면 true. 면역 시간은 남기지 않는다.</summary>
        public bool Remove(StatusDef def)
        {
            if (def == null) return false;
            bool removed = false;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                StatusInstance s = active[i];
                if (s.Def != def) continue;
                active.RemoveAt(i);
                removed = true;
                Raise(StatusEventKind.Removed, s.Def, s.Caster, Fixed.Zero);
            }
            return removed;
        }

        /// <summary>시간을 진행한다. 지속 피해 틱, 만료, 면역 시간 감소를 처리한다.</summary>
        public void Tick(Fixed deltaTime)
        {
            if (deltaTime <= Fixed.Zero) return;

            for (int i = active.Count - 1; i >= 0 && i < active.Count; i--)
            {
                StatusInstance s = active[i];

                if (s.Def.Rule == StatusRule.DamageOverTime && s.Def.TickInterval > Fixed.Zero)
                {
                    // 남은 시간 안에서만 틱이 난다. 지속 시간이 틱 간격의 배수면 끝나는 순간의 틱까지 들어간다.
                    Fixed span = s.stackTimes[0] < deltaTime ? s.stackTimes[0] : deltaTime;
                    s.tickTimer = s.tickTimer - span;
                    while (s.tickTimer <= Fixed.Zero)
                    {
                        s.tickTimer = s.tickTimer + s.Def.TickInterval;
                        Fixed dealt = owner.TakeDotTick(s);
                        Raise(StatusEventKind.Ticked, s.Def, s.Caster, dealt);
                        if (!owner.IsAlive) return; // 쓰러지면서 목록이 비워졌다.
                    }
                }

                if (s.Def.Permanent) continue; // 지속형은 시간이 줄지 않는다

                for (int k = s.stackTimes.Count - 1; k >= 0; k--)
                {
                    s.stackTimes[k] = s.stackTimes[k] - deltaTime;
                    if (s.stackTimes[k] <= Fixed.Zero) s.stackTimes.RemoveAt(k);
                }
                if (s.stackTimes.Count > 0) continue;

                active.RemoveAt(i);
                if (s.Def.Rule == StatusRule.Control && s.Def.ImmunityAfter > Fixed.Zero) immunity[s.Def] = s.Def.ImmunityAfter;
                Raise(StatusEventKind.Expired, s.Def, s.Caster, Fixed.Zero);
            }

            if (immunity.Count == 0) return;
            immunityKeys.Clear();
            immunityKeys.AddRange(immunity.Keys);
            foreach (StatusDef def in immunityKeys)
            {
                Fixed left = immunity[def] - deltaTime;
                if (left <= Fixed.Zero) immunity.Remove(def);
                else immunity[def] = left;
            }
        }

        /// <summary>모두 지운다. 대상이 쓰러질 때 부른다.</summary>
        internal void Clear()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                StatusInstance s = active[i];
                active.RemoveAt(i);
                Raise(StatusEventKind.Removed, s.Def, s.Caster, Fixed.Zero);
            }
            immunity.Clear();
        }

        private StatusInstance Add(StatusDef def, Combatant caster)
        {
            var instance = new StatusInstance(def, caster);
            instance.stackTimes.Add(def.Duration);
            active.Add(instance);
            Raise(StatusEventKind.Applied, def, caster, Fixed.FromInt(1));
            return instance;
        }

        private void Raise(StatusEventKind kind, StatusDef def, Combatant caster, Fixed amount)
        {
            Changed?.Invoke(new StatusEvent(kind, owner, def, caster, amount));
        }
    }
}
