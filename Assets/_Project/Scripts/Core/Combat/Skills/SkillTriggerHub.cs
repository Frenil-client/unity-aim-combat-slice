using System;
using System.Collections.Generic;
using AimCombat.Core.Random;

namespace AimCombat.Core.Combat
{
    /// <summary>
    /// 패시브 스킬의 발동 판정(SPEC 3.2 패시브 트리거). 전투 이벤트를 받아 조건이 맞는 패시브를 SkillRunner 로 실행한다.
    ///
    /// 이벤트는 두 길로 들어온다.
    /// - 바깥(분대)이 아는 것은 Raise 로 넣는다: 전투 시작, 버스트 사용, 풀 버스트 시작과 끝, 사격, 명중, 재장전, 처치.
    /// - 전투 단위가 스스로 아는 것은 여기서 구독한다: 피격, 체력 하한, 회복, 상태효과 획득과 만료.
    ///
    /// 들어온 이벤트는 바로 실행하지 않고 줄에 세웠다가 Flush 에서 들어온 순서대로 처리한다.
    /// 스킬 실행 도중(SkillRunner 의 버퍼 사용 중)이나 적의 공격 처리 도중에 다른 스킬이 끼어들지 않게 하기 위해서다.
    /// 패시브가 실행되며 생긴 이벤트(버프 획득, 처치 등)도 같은 줄 뒤에 붙어 같은 Flush 안에서 이어서 처리된다.
    /// 서로를 계속 발동시키는 조합이 끝없이 돌지 않도록 한 번의 Flush 는 MaxEventsPerFlush 개까지만 처리하고 나머지는 버린다.
    ///
    /// 조건(상태 보유, 풀 버스트 여부)이 맞지 않는 동안의 이벤트는 횟수로 세지 않는다.
    /// 확률은 발동 직전에 한 번 굴린다. 실패해도 횟수는 처음부터 다시 센다.
    /// </summary>
    public sealed class SkillTriggerHub
    {
        public const int MaxEventsPerFlush = 256;

        private sealed class Passive
        {
            public Combatant Owner;
            /// <summary>발동 블록. 아래 코드에서는 skill 이라는 이름으로 읽는다.</summary>
            public SkillBlockDef Skill;
            /// <summary>이 블록이 들어 있는 스킬(스킬 1, 스킬 2, 버스트).</summary>
            public SkillDef Parent;
            /// <summary>횟수형 트리거가 지금까지 센 횟수.</summary>
            public int Count;
            /// <summary>Periodic 이 지금까지 센 시간.</summary>
            public Fixed Timer;
            /// <summary>다시 발동할 수 있을 때까지 남은 시간.</summary>
            public Fixed CooldownLeft;
            /// <summary>HealthBelow: 체력이 기준 위에 있어 다음에 내려가면 발동한다.</summary>
            public bool Armed = true;
            /// <summary>StatusMaxStacks: 지난 확인 때 최대 중첩이었는지.</summary>
            public bool WasMaxed;
        }

        private readonly struct Pending
        {
            public readonly SkillTrigger Kind;
            /// <summary>이벤트의 주인. null 이면 팀 전체 이벤트라 모든 패시브가 받는다.</summary>
            public readonly Combatant Subject;
            /// <summary>트리거를 일으킨 쪽(맞은 적, 버스트를 쓴 아군).</summary>
            public readonly Combatant Source;
            public readonly StatusDef Status;
            /// <summary>줄을 거치지 않고 이 패시브 하나만 발동시킨다(주기형, 최대 중첩).</summary>
            public readonly Passive Direct;

            public Pending(SkillTrigger kind, Combatant subject, Combatant source, StatusDef status, Passive direct)
            {
                Kind = kind;
                Subject = subject;
                Source = source;
                Status = status;
                Direct = direct;
            }
        }

        private readonly ISkillWorld world;
        private readonly DeterministicRandom random;
        private readonly List<Passive> passives = new List<Passive>();
        private readonly List<Combatant> owners = new List<Combatant>();
        private readonly Queue<Pending> queue = new Queue<Pending>();
        private bool flushing;

        public SkillTriggerHub(ISkillWorld world, DeterministicRandom random)
        {
            this.world = world;
            this.random = random;
        }

        /// <summary>패시브가 발동했다. 인자는 시전자, 스킬, 발동한 블록. HUD 와 연출이 구독한다.</summary>
        public event Action<Combatant, SkillDef, SkillBlockDef> Triggered;

        /// <summary>한 번의 Flush 한도를 넘겨 버린 이벤트 수. 0이 아니면 서로 발동시키는 패시브 조합이 있다.</summary>
        public int Dropped { get; private set; }

        public int Count => passives.Count;

        /// <summary>
        /// owner 의 스킬에 든 패시브 발동 블록을 등록한다. 액티브와 버스트 블록은 쓰는 쪽(분대)이 직접 실행하므로 넘어간다.
        /// 블록은 등록 뒤에 스킬에 더하거나 빼도 반영되지 않는다(전투 시작 때 한 번 등록한다).
        /// </summary>
        public void Add(Combatant owner, SkillDef skill)
        {
            if (owner == null || skill == null) return;
            foreach (SkillBlockDef block in skill.Blocks) Add(owner, skill, block);
        }

        private void Add(Combatant owner, SkillDef parent, SkillBlockDef skill)
        {
            if (skill == null || !skill.IsPassive) return;
            passives.Add(new Passive { Owner = owner, Skill = skill, Parent = parent });
            if (owners.Contains(owner)) return;

            owners.Add(owner);
            owner.Damaged += e => OnOwnerDamaged(owner, e);
            owner.Healed += (_, __) => OnOwnerHealed(owner);
            owner.Statuses.Changed += OnOwnerStatus;
        }

        /// <summary>
        /// 전투 이벤트를 줄에 세운다. subject 가 null 이면 팀 전체 이벤트(전투 시작, 풀 버스트 시작과 끝, 아군 버스트)다.
        /// source 는 TriggerEnemy, TriggerAlly 대상이 될 쪽이다. 처리하려면 Flush 를 부른다.
        /// </summary>
        public void Raise(SkillTrigger kind, Combatant subject = null, Combatant source = null, StatusDef status = null)
        {
            if (passives.Count == 0) return;
            queue.Enqueue(new Pending(kind, subject, source, status, null));
        }

        /// <summary>
        /// 시간을 진행한다. 쿨다운과 주기형을 세고 최대 중첩 도달을 확인한 뒤 줄을 비운다.
        /// enemiesPresent 가 false 면 적에게 쓰는 효과가 있는 주기형은 준비된 채로 기다린다.
        /// </summary>
        public void Tick(Fixed deltaTime, bool enemiesPresent)
        {
            foreach (Passive p in passives)
            {
                if (!p.Owner.IsAlive) continue;
                SkillBlockDef skill = p.Skill;
                if (p.CooldownLeft > Fixed.Zero) p.CooldownLeft = p.CooldownLeft - deltaTime;

                if (skill.Trigger == SkillTrigger.Periodic)
                {
                    Fixed interval = IntervalOf(p);
                    if (interval <= Fixed.Zero) continue;
                    if (p.Timer < interval) p.Timer = p.Timer + deltaTime;
                    if (p.Timer < interval) continue;
                    if (skill.HasEnemyEffect && !enemiesPresent) continue;
                    if (!ConditionsMet(p)) continue;
                    p.Timer = p.Timer - interval;
                    if (p.Timer > interval) p.Timer = interval; // 기다린 시간을 몰아서 여러 번 쏘지 않는다
                    queue.Enqueue(new Pending(SkillTrigger.Periodic, p.Owner, null, null, p));
                }
                else if (skill.Trigger == SkillTrigger.StatusMaxStacks && skill.TriggerStatus != null)
                {
                    // 중첩은 이벤트 없이 하나씩 풀리기도 하므로 틱마다 직접 확인해 올라가는 순간만 잡는다.
                    bool maxed = p.Owner.Statuses.StacksOf(skill.TriggerStatus) >= skill.TriggerStatus.StackLimit;
                    if (maxed && !p.WasMaxed) queue.Enqueue(new Pending(SkillTrigger.StatusMaxStacks, p.Owner, null, skill.TriggerStatus, p));
                    p.WasMaxed = maxed;
                }
            }
            Flush();
        }

        /// <summary>줄에 선 이벤트를 들어온 순서대로 처리한다. 처리 중에 생긴 이벤트도 이어서 처리한다.</summary>
        public void Flush()
        {
            if (flushing) return;
            flushing = true;
            int processed = 0;
            while (queue.Count > 0)
            {
                if (processed++ >= MaxEventsPerFlush)
                {
                    Dropped += queue.Count;
                    queue.Clear();
                    break;
                }
                Process(queue.Dequeue());
            }
            flushing = false;
        }

        private void Process(in Pending e)
        {
            if (e.Direct != null)
            {
                // 주기형과 최대 중첩은 Tick 에서 이미 때를 확인했다. 조건만 다시 본다(줄에 선 사이에 바뀌었을 수 있다).
                if (e.Direct.Owner.IsAlive && ConditionsMet(e.Direct)) Fire(e.Direct, e.Source);
                return;
            }

            // 발동 중에 패시브가 등록되지는 않는다. 등록 순서(배치 순서, 스킬 순서)대로 본다.
            for (int i = 0; i < passives.Count; i++)
            {
                Passive p = passives[i];
                SkillBlockDef skill = p.Skill;
                if (skill.Trigger != e.Kind || !p.Owner.IsAlive) continue;
                if (e.Subject != null && e.Subject != p.Owner) continue;
                if (SkillTriggers.WatchesStatus(skill.Trigger) && skill.TriggerStatus != e.Status) continue;
                if (!ConditionsMet(p)) continue;

                if (SkillTriggers.IsCounted(skill.Trigger))
                {
                    int need = skill.TriggerCount < 1 ? 1 : skill.TriggerCount;
                    if (++p.Count < need) continue;
                    p.Count = 0;
                }
                Fire(p, e.Source);
            }
        }

        private void Fire(Passive p, Combatant source)
        {
            SkillBlockDef skill = p.Skill;
            if (p.CooldownLeft > Fixed.Zero) return;
            if (skill.Chance < Fixed.One && random.NextUnitFixed() >= skill.Chance) return;

            p.CooldownLeft = skill.Cooldown;
            SkillRunner.Execute(skill, p.Owner, world, random, source);
            Triggered?.Invoke(p.Owner, p.Parent, skill);
        }

        private bool ConditionsMet(Passive p)
        {
            SkillBlockDef skill = p.Skill;
            if (skill.RequireSelfStatus != null && !p.Owner.Statuses.Has(skill.RequireSelfStatus)) return false;
            if (skill.ForbidSelfStatus != null && p.Owner.Statuses.Has(skill.ForbidSelfStatus)) return false;
            if (skill.BurstState == BurstCondition.FullBurstOnly && !world.Enhanced) return false;
            if (skill.BurstState == BurstCondition.NotFullBurst && world.Enhanced) return false;
            return true;
        }

        private static Fixed IntervalOf(Passive p)
        {
            SkillBlockDef skill = p.Skill;
            bool alt = skill.AltIntervalStatus != null && skill.AltInterval > Fixed.Zero && p.Owner.Statuses.Has(skill.AltIntervalStatus);
            return alt ? skill.AltInterval : skill.Interval;
        }

        // ---- 전투 단위가 스스로 아는 이벤트 ----

        private void OnOwnerDamaged(Combatant owner, DamageEvent e)
        {
            queue.Enqueue(new Pending(SkillTrigger.Damaged, owner, e.Source, null, null));

            // 체력 하한은 기준을 넘어 내려가는 순간에만 선다. 쓰러졌으면 서지 않는다.
            foreach (Passive p in passives)
            {
                if (p.Owner != owner || p.Skill.Trigger != SkillTrigger.HealthBelow) continue;
                bool below = owner.Health <= owner.MaxHealth * p.Skill.HealthThreshold;
                if (below && p.Armed && owner.IsAlive) queue.Enqueue(new Pending(SkillTrigger.HealthBelow, owner, e.Source, null, p));
                p.Armed = !below;
            }
        }

        private void OnOwnerHealed(Combatant owner)
        {
            queue.Enqueue(new Pending(SkillTrigger.Healed, owner, null, null, null));
            foreach (Passive p in passives)
            {
                if (p.Owner != owner || p.Skill.Trigger != SkillTrigger.HealthBelow) continue;
                if (owner.Health > owner.MaxHealth * p.Skill.HealthThreshold) p.Armed = true;
            }
        }

        private void OnOwnerStatus(StatusEvent e)
        {
            switch (e.Kind)
            {
                case StatusEventKind.Applied:
                case StatusEventKind.Stacked:
                    queue.Enqueue(new Pending(SkillTrigger.StatusGained, e.Target, e.Caster, e.Def, null));
                    break;
                case StatusEventKind.Expired:
                    queue.Enqueue(new Pending(SkillTrigger.StatusExpired, e.Target, e.Caster, e.Def, null));
                    break;
            }
        }
    }
}
