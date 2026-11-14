using System;

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
    /// </summary>
    public sealed class Combatant
    {
        private StatusDef breakStatus;

        public Combatant(string name, Fixed maxHealth, Fixed attack, Fixed defense)
        {
            Name = name;
            MaxHealth = maxHealth;
            Health = maxHealth;
            Attack = attack;
            Defense = defense;
            Statuses = new StatusContainer(this);
        }

        public string Name { get; }

        /// <summary>이 단위를 그리는 바깥 오브젝트. Core 는 내용을 모른다.</summary>
        public object Owner { get; set; }

        public Fixed MaxHealth { get; private set; }
        public Fixed Health { get; private set; }
        public Fixed Attack { get; set; }
        public Fixed Defense { get; set; }
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
            if (IsAlive) Statuses.Tick(deltaTime);
        }

        // 지속 피해 한 틱. 시전자의 지금 버프로 SPEC 3.1 의 1, 2, 5, 6단계를 거친다(주요 배율과 차지는 받지 않는다).
        internal Fixed TakeDotTick(StatusInstance dot)
        {
            Combatant caster = dot.Caster;
            Fixed attack = caster != null ? caster.Attack : Fixed.Zero;
            Fixed attackUp = caster != null ? caster.Statuses.Sum(StatKind.AttackUp) : Fixed.Zero;
            Fixed amount = DamageFormula.ComputeDot(attack, attackUp, Defense, dot.Def.Value, Statuses.Sum(StatKind.DamageTakenUp));
            ApplyDamage(amount, DamageSource.Dot, caster);
            return amount;
        }

        // 게이지형 대상에게 행동 불가가 들어왔다. 그로기 중에는 깎지 않는다. 깎은 양을 돌려준다.
        internal Fixed AbsorbControl(StatusDef def)
        {
            if (IsControlled || ControlGaugeMax <= Fixed.Zero) return Fixed.Zero;

            Fixed cut = def.Value > ControlGauge ? ControlGauge : def.Value;
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
