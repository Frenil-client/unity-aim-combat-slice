using System;
using StatSystem;

namespace AimCombat.Core.Combat
{
    /// <summary>행동 불가 효과를 받는 방식(SPEC 3.3).</summary>
    public enum ControlResponse
    {
        /// <summary>그대로 걸린다.</summary>
        Normal,
        /// <summary>걸리지 않고 저항 게이지가 깎인다. 게이지가 다 깎이면 그로기(정해진 시간 행동 불가)에 빠지고 게이지가 다시 찬다. 보스용.</summary>
        Gauge,
    }

    public enum DamageSource
    {
        /// <summary>아군 사격.</summary>
        Shot,
        /// <summary>스킬, 궁극기.</summary>
        Skill,
        /// <summary>지속 피해 틱.</summary>
        Dot,
        /// <summary>적의 공격.</summary>
        Enemy,
    }

    /// <summary>들어간 피해 한 번. 화면 표시와 연출이 구독한다.</summary>
    public readonly struct DamageEvent
    {
        public readonly Combatant Target;
        public readonly Combatant Source;
        public readonly DamageSource Kind;
        public readonly Fixed Amount;
        public readonly bool Critical;
        public readonly bool WeakPoint;

        public DamageEvent(Combatant target, Combatant source, DamageSource kind, Fixed amount, bool critical, bool weakPoint)
        {
            Target = target;
            Source = source;
            Kind = kind;
            Amount = amount;
            Critical = critical;
            WeakPoint = weakPoint;
        }
    }

    /// <summary>
    /// 전투 단위 하나(아군, 적, 보스)의 체력, 공격력, 방어력, 상태효과. Unity 오브젝트는 이것을 들고 결과만 그린다.
    /// 피해 계산 자체는 DamageFormula 가 하고, 여기서는 들어온 피해를 깎고 이벤트를 낸다.
    ///
    /// 능력치는 재사용 패키지 stat-system(com.frenil.stat-system)의 <see cref="Stat"/> 에 둔다.
    /// - 기본값: 공격력, 방어력, 치명 확률(기본 15%), 이동 속도(1).
    /// - 상태효과가 올리고 내리는 능력치는 모디파이어다. 상태효과의 규칙(중첩, 갱신, 최댓값)은 그대로 StatusContainer 가 판정하고,
    ///   그 결과인 능력치별 합계를 능력치마다 모디파이어 하나로 붙인다. 상태효과가 바뀌거나(이벤트) 시간이 흐르면(중첩 하나가 풀리는 경우) 다시 맞춘다.
    ///   stat-system 은 모디파이어를 뗄 때 기본값부터 다시 계산하므로, 붙였다 떼는 사이에 값이 어긋나지 않는다.
    /// - stat-system 에 해당하는 능력치가 없는 약점 항목 증가와 받는 피해 증가는 StatusContainer.Sum 으로 바로 읽는다.
    /// </summary>
    public sealed class Combatant
    {
        // 상태효과 능력치와 stat-system 능력치의 대응. Percent 표시는 피해 계산 층이 쓰는 비율 합계라 Flat 으로 더한다.
        // 이동 속도 감소는 기본값 1 에 음수 PercentAdd 로 붙여 최종값이 곧 이동 배율이 되게 한다.
        private static readonly (StatKind kind, StatId id, StatModifierType type, bool negate)[] StatLinks =
        {
            (StatKind.AttackUp, StatId.AttackPowerPercent, StatModifierType.Flat, false),
            (StatKind.CritChanceUp, StatId.CriticalRate, StatModifierType.Flat, false),
            (StatKind.CritBonusUp, StatId.CriticalDamage, StatModifierType.Flat, false),
            (StatKind.DamageUp, StatId.Damage, StatModifierType.Flat, false),
            (StatKind.Slow, StatId.MoveSpeed, StatModifierType.PercentAdd, true),
        };

        private StatusDef breakStatus;
        private readonly ModifierHandle[] linkHandles = new ModifierHandle[StatLinks.Length];
        private readonly Fixed[] linkSums = new Fixed[StatLinks.Length];

        public Combatant(string name, Fixed maxHealth, Fixed attack, Fixed defense)
        {
            Name = name;
            MaxHealth = maxHealth;
            Health = maxHealth;
            Stats = new Stat();
            Stats.SetBaseValue(StatId.CriticalRate, StatBridge.ToStat(DamageFormula.BaseCritChance));
            Stats.SetBaseValue(StatId.MoveSpeed, StatValue.One);
            Attack = attack;
            Defense = defense;
            Statuses = new StatusContainer(this);
            Statuses.Changed += _ => SyncStats();
        }

        /// <summary>능력치(stat-system). 기본값과 상태효과 모디파이어를 접은 최종값을 캐시해 둔다.</summary>
        public Stat Stats { get; }

        /// <summary>능력치 최종값을 이 프로젝트의 고정소수점으로 읽는다.</summary>
        public Fixed GetStat(StatId id) => StatBridge.ToFixed(Stats.GetValue(id));

        public string Name { get; }

        /// <summary>이 단위를 그리는 바깥 오브젝트. Core 는 내용을 모른다.</summary>
        public object Owner { get; set; }

        public Fixed MaxHealth { get; private set; }
        public Fixed Health { get; private set; }

        /// <summary>기본 공격력(stat-system 의 AttackPower 기본값). 공격력 증가 버프는 피해 계산 1단계에서 따로 곱한다.</summary>
        public Fixed Attack
        {
            get => StatBridge.ToFixed(Stats.GetBaseValue(StatId.AttackPower));
            set => Stats.SetBaseValue(StatId.AttackPower, StatBridge.ToStat(value));
        }

        /// <summary>방어력(stat-system 의 Defense 기본값).</summary>
        public Fixed Defense
        {
            get => StatBridge.ToFixed(Stats.GetBaseValue(StatId.Defense));
            set => Stats.SetBaseValue(StatId.Defense, StatBridge.ToStat(value));
        }
        public StatusContainer Statuses { get; }

        public bool IsAlive => Health > Fixed.Zero;
        public bool IsControlled => Statuses.IsControlled;

        public ControlResponse ControlResponse { get; private set; }
        public Fixed ControlGaugeMax { get; private set; }
        public Fixed ControlGauge { get; private set; }

        public event Action<DamageEvent> Damaged;
        public event Action<Combatant, Fixed> Healed;
        public event Action<Combatant> Died;

        /// <summary>저항 게이지가 다 깎여 그로기에 빠지는 순간 알린다.</summary>
        public event Action<Combatant> Broken;

        /// <summary>보스처럼 행동 불가 대신 저항 게이지가 깎이게 한다. 게이지가 다 깎이면 breakDuration 동안 행동 불가.</summary>
        public void UseControlGauge(Fixed gaugeMax, Fixed breakDuration)
        {
            ControlResponse = ControlResponse.Gauge;
            ControlGaugeMax = gaugeMax;
            ControlGauge = gaugeMax;
            breakStatus = new StatusDef
            {
                Id = "break",
                Name = "그로기",
                Rule = StatusRule.Control,
                Duration = breakDuration,
            };
        }

        /// <summary>피해를 깎는다. 실제로 깎인 양(남은 체력을 넘지 않는다)을 돌려준다.</summary>
        public Fixed ApplyDamage(Fixed amount, DamageSource kind, Combatant source, bool critical = false, bool weakPoint = false)
        {
            if (!IsAlive || amount <= Fixed.Zero) return Fixed.Zero;

            Fixed dealt = amount > Health ? Health : amount;
            Health = Health - dealt;
            Damaged?.Invoke(new DamageEvent(this, source, kind, amount, critical, weakPoint));

            if (!IsAlive)
            {
                Statuses.Clear();
                Died?.Invoke(this);
            }
            return dealt;
        }

        public void Heal(Fixed amount)
        {
            if (!IsAlive || amount <= Fixed.Zero) return;
            Fixed missing = MaxHealth - Health;
            Fixed healed = amount > missing ? missing : amount;
            if (healed <= Fixed.Zero) return;
            Health = Health + healed;
            Healed?.Invoke(this, healed);
        }

        /// <summary>상태효과 시간을 진행한다.</summary>
        public void Tick(Fixed deltaTime)
        {
            if (!IsAlive) return;
            Statuses.Tick(deltaTime);
            SyncStats();   // 중첩 하나가 시간이 다 돼 빠지는 것은 이벤트가 없어서 시간이 흐를 때마다 맞춘다
        }

        // 상태효과의 능력치 합계를 stat-system 모디파이어로 맞춘다. 합계가 그대로면 손대지 않는다.
        private void SyncStats()
        {
            for (int i = 0; i < StatLinks.Length; i++)
            {
                Fixed sum = Statuses.Sum(StatLinks[i].kind);
                if (sum == linkSums[i] && (linkHandles[i].IsValid || sum == Fixed.Zero)) continue;
                linkSums[i] = sum;
                if (linkHandles[i].IsValid) Stats.RemoveModifier(linkHandles[i]);
                linkHandles[i] = default;
                if (sum == Fixed.Zero) continue;
                StatValue value = StatBridge.ToStat(StatLinks[i].negate ? -sum : sum);
                linkHandles[i] = Stats.AddModifier(StatLinks[i].id, StatLinks[i].type, value, Statuses);
            }
        }

        // 지속 피해 한 틱. 시전자의 지금 버프로 SPEC 3.1 의 1, 2, 5, 6단계를 거친다(주요 배율과 차지는 받지 않는다).
        internal Fixed TakeDotTick(StatusInstance dot)
        {
            Combatant caster = dot.Caster;
            Fixed attack = caster != null ? caster.Attack : Fixed.Zero;
            Fixed attackUp = caster != null ? caster.GetStat(StatId.AttackPowerPercent) : Fixed.Zero;
            Fixed amount = DamageFormula.ComputeDot(attack, attackUp, Defense, dot.Def.Value, Statuses.Sum(StatKind.DamageTakenUp));
            ApplyDamage(amount, DamageSource.Dot, caster);
            return amount;
        }

        // 게이지형 대상에게 행동 불가가 들어왔다. 효과의 값만큼 게이지를 깎는다.
        internal Fixed AbsorbControl(StatusDef def) => CutControlGauge(def.Value);

        /// <summary>
        /// 저항 게이지를 깎는다(행동 불가 효과, 부위 파괴). 그로기 중에는 깎지 않는다. 다 깎이면 그로기에 빠지고 게이지가 다시 찬다.
        /// 깎은 양을 돌려준다. 게이지형이 아닌 대상에게는 아무 일도 없다.
        /// </summary>
        public Fixed CutControlGauge(Fixed amount)
        {
            if (ControlResponse != ControlResponse.Gauge || !IsAlive || IsControlled) return Fixed.Zero;
            if (ControlGaugeMax <= Fixed.Zero || amount <= Fixed.Zero) return Fixed.Zero;

            Fixed cut = amount > ControlGauge ? ControlGauge : amount;
            ControlGauge = ControlGauge - cut;
            if (ControlGauge <= Fixed.Zero)
            {
                ControlGauge = ControlGaugeMax;
                Statuses.ForceControl(breakStatus);
                Broken?.Invoke(this);
            }
            return cut;
        }
    }
}
