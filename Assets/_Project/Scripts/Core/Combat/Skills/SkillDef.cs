using System.Collections.Generic;

namespace AimCombat.Core.Combat
{
    /// <summary>
    /// 스킬이 발동하는 계기(SPEC 3.2). Active 와 Burst 는 쓰는 쪽이 직접 실행하고,
    /// 나머지는 패시브 트리거로 SkillTriggerHub 가 전투 이벤트를 받아 실행한다.
    /// 값의 순서는 에셋에 저장되므로 새 항목은 끝에만 덧붙인다.
    /// </summary>
    public enum SkillTrigger
    {
        /// <summary>쿨다운이 차면 쓸 수 있는 액티브 스킬.</summary>
        Active,
        /// <summary>버스트. 연계에서 자기 버스트 단계가 열렸을 때만 쓸 수 있고 쿨다운이 있다(니케 버스트 구조).</summary>
        Burst,

        // ---- 패시브: 시간 ----
        /// <summary>전투 시작 시 한 번.</summary>
        BattleStart,
        /// <summary>Interval 초마다 반복. 적에게 쓰는 효과가 있으면 적이 있을 때까지 아껴 둔다.</summary>
        Periodic,

        // ---- 패시브: 버스트 ----
        /// <summary>자신이 버스트 스킬을 썼을 때.</summary>
        BurstUsed,
        /// <summary>아군 누구든(자신 포함) 버스트 스킬을 썼을 때. 발동 대상은 버스트를 쓴 아군이다.</summary>
        AllyBurstUsed,
        /// <summary>풀 버스트 타임이 시작될 때.</summary>
        FullBurstStart,
        /// <summary>풀 버스트 타임이 끝날 때.</summary>
        FullBurstEnd,

        // ---- 패시브: 사격 ----
        /// <summary>일반 공격(방아쇠 한 번)을 TriggerCount 회 할 때마다. 맞지 않아도 센다.</summary>
        ShotFired,
        /// <summary>일반 공격이 TriggerCount 회 명중할 때마다. 산탄은 탄알 하나, 관통탄은 대상 하나가 1회다.</summary>
        Hit,
        /// <summary>일반 공격이 치명으로 TriggerCount 회 명중할 때마다.</summary>
        CriticalHit,
        /// <summary>일반 공격이 약점(코어)에 TriggerCount 회 명중할 때마다.</summary>
        WeakPointHit,
        /// <summary>풀 차지 공격이 TriggerCount 회 명중할 때마다.</summary>
        FullChargeHit,
        /// <summary>탄창의 마지막 탄환을 쏠 때.</summary>
        LastBulletFired,
        /// <summary>탄창의 마지막 탄환이 명중할 때.</summary>
        LastBulletHit,
        /// <summary>재장전을 끝냈을 때.</summary>
        Reloaded,
        /// <summary>적을 처치했을 때(사격, 스킬, 지속 피해 모두).</summary>
        Kill,

        // ---- 패시브: 생존 ----
        /// <summary>TriggerCount 회 피격될 때마다.</summary>
        Damaged,
        /// <summary>체력 비율이 HealthThreshold 이하로 내려가는 순간. 다시 위로 올라가야 또 발동한다.</summary>
        HealthBelow,
        /// <summary>회복 효과를 받았을 때.</summary>
        Healed,

        // ---- 패시브: 상태효과 ----
        /// <summary>자신에게 TriggerStatus 가 걸리거나 중첩이 늘 때.</summary>
        StatusGained,
        /// <summary>자신의 TriggerStatus 가 최대 중첩에 이르는 순간.</summary>
        StatusMaxStacks,
        /// <summary>자신의 TriggerStatus 가 시간이 다 돼 풀릴 때.</summary>
        StatusExpired,
    }

    /// <summary>풀 버스트 여부 조건. "풀 버스트 타임이라면 / 아니라면".</summary>
    public enum BurstCondition
    {
        Any,
        FullBurstOnly,
        NotFullBurst,
    }

    /// <summary>효과를 받을 대상을 고르는 방법. 값의 순서는 에셋에 저장되므로 끝에만 덧붙인다.</summary>
    public enum SkillTarget
    {
        Self,
        AllAllies,
        /// <summary>남은 체력 비율이 가장 낮은 살아 있는 아군.</summary>
        LowestHealthAlly,
        /// <summary>시전자가 지금 노리는 적. 조작 중이면 조준점 아래의 적, 없으면 가장 가까운 적.</summary>
        FocusEnemy,
        /// <summary>시전자에게서 가까운 적 Count 기.</summary>
        NearestEnemies,
        AllEnemies,
        /// <summary>무작위 적 Count 기. 같은 적을 두 번 고르지 않는다.</summary>
        RandomEnemies,
        /// <summary>트리거를 일으킨 적. 명중, 처치 트리거에서는 맞은 적이다. 그런 대상이 없는 트리거에서는 아무도 고르지 않는다.</summary>
        TriggerEnemy,
        /// <summary>트리거를 일으킨 아군. AllyBurstUsed 에서는 버스트를 쓴 아군이다.</summary>
        TriggerAlly,
    }

    /// <summary>효과의 종류. 실행 순서도 Stages(SkillRunner)의 단계 순서를 따른다. 끝에만 덧붙인다.</summary>
    public enum SkillEffectKind
    {
        /// <summary>상태효과를 건다.</summary>
        ApplyStatus,
        /// <summary>스킬 피해. 계수는 공격력의 배수(SPEC 3.1 스킬 계수).</summary>
        Damage,
        /// <summary>회복. 계수는 대상 최대 체력의 비율.</summary>
        Heal,
        /// <summary>탄창을 즉시 다 채운다.</summary>
        RefillAmmo,
        /// <summary>걸려 있는 상태효과를 지운다. "풀 버스트 종료 시 중첩 제거" 같은 정리용이다.</summary>
        RemoveStatus,
    }

    public sealed class SkillEffectDef
    {
        public SkillEffectKind Kind;
        public SkillTarget Target;
        /// <summary>NearestEnemies, RandomEnemies 의 대상 수.</summary>
        public int Count = 1;
        public Fixed Coefficient;
        /// <summary>ApplyStatus 가 걸거나 RemoveStatus 가 지우는 효과.</summary>
        public StatusDef Status;
        /// <summary>비어 있지 않으면 이 상태효과가 걸린 대상만 받는다("약점 개방 상태의 적에게").</summary>
        public StatusDef TargetStatus;
        /// <summary>Damage 를 약점(코어) 명중으로 친다. 주요 배율에 약점 항목이 들어간다.</summary>
        public bool CoreHit;

        public bool TargetsEnemies => SkillTargets.IsEnemySide(Target);
    }

    /// <summary>캐릭터의 스킬 칸. 모든 캐릭터가 같은 세 칸을 가진다(SPEC 3.2).</summary>
    public enum SkillSlot
    {
        Skill1,
        Skill2,
        Burst,
    }

    /// <summary>
    /// 스킬 하나의 정의(스킬 1, 스킬 2, 버스트 스킬). 발동 블록의 목록이고 데이터(Unity 의 SkillData 에셋)에서 만들어진다.
    /// 발동 시점이 여러 개인 스킬("전투 시작 시 ~, 버스트 스킬 사용 시 ~")은 블록을 여러 개 둔다.
    /// 새 스킬은 블록의 조합을 데이터로 새로 짜서 만든다. 코드를 고치지 않는다.
    /// </summary>
    public sealed class SkillDef
    {
        public string Id = "";
        public string Name = "";
        public SkillSlot Slot;
        /// <summary>버스트 쿨다운(초). 쓰는 순간부터 센다. 버스트 칸만 쓴다.</summary>
        public Fixed Cooldown;
        /// <summary>버스트 단계(1~3). 버스트 칸만 쓴다. 캐릭터 고유의 값이다.</summary>
        public int BurstStage;
        /// <summary>이 버스트를 쓴 뒤 다음 단계 창이 열려 있는 시간(초). 3단계는 쓰지 않는다(풀 버스트로 넘어간다).</summary>
        public Fixed NextStageWindow;
        public readonly List<SkillBlockDef> Blocks = new List<SkillBlockDef>();

        public bool IsBurst => Slot == SkillSlot.Burst;

        public override string ToString() => string.IsNullOrEmpty(Name) ? Id : Name;
    }

    /// <summary>
    /// 발동 블록 하나. 트리거, 발동 조건, 효과 목록의 조합이다. 니케 스킬 설명의 "■ ~ 시 발동. ~ 에게 적용" 한 덩어리에 해당한다.
    /// </summary>
    public sealed class SkillBlockDef
    {
        public SkillTrigger Trigger;
        /// <summary>
        /// 액티브: 쿨다운(초, 0보다 커야 한다). 패시브: 한 번 발동한 뒤 다시 발동하기까지의 최소 간격(0이면 없음).
        /// 버스트 블록은 쓰지 않는다(버스트 쿨다운은 스킬에 있다).
        /// </summary>
        public Fixed Cooldown;

        // ---- 패시브 트리거 설정 ----

        /// <summary>횟수형 트리거가 몇 회마다 발동하는지("일반 공격 10회 명중 시"). 1이면 매번.</summary>
        public int TriggerCount = 1;
        /// <summary>Periodic 의 반복 주기(초).</summary>
        public Fixed Interval;
        /// <summary>AltIntervalStatus 가 자신에게 걸려 있는 동안 쓰는 주기(초). "반복 주기 감소" 버프를 이렇게 표현한다.</summary>
        public Fixed AltInterval;
        public StatusDef AltIntervalStatus;
        /// <summary>StatusGained, StatusMaxStacks, StatusExpired 가 지켜보는 상태효과.</summary>
        public StatusDef TriggerStatus;
        /// <summary>HealthBelow 의 체력 비율(0~1).</summary>
        public Fixed HealthThreshold;

        // ---- 발동 조건. 조건이 맞지 않는 동안 일어난 이벤트는 횟수로도 세지 않는다 ----

        /// <summary>발동 확률(0~1). 1이면 늘 발동.</summary>
        public Fixed Chance = Fixed.One;
        /// <summary>자신에게 이 상태효과가 걸려 있어야 발동한다("~ 상태라면").</summary>
        public StatusDef RequireSelfStatus;
        /// <summary>자신에게 이 상태효과가 걸려 있으면 발동하지 않는다("~ 상태가 아니라면").</summary>
        public StatusDef ForbidSelfStatus;
        public BurstCondition BurstState;

        public readonly List<SkillEffectDef> Effects = new List<SkillEffectDef>();

        public bool IsPassive => SkillTriggers.IsPassive(Trigger);

        /// <summary>적에게 쓰는 효과가 하나라도 있는지. 주기형이 적이 없을 때 아껴 두는 기준이다.</summary>
        public bool HasEnemyEffect
        {
            get
            {
                foreach (SkillEffectDef effect in Effects) if (effect != null && effect.TargetsEnemies) return true;
                return false;
            }
        }

        public override string ToString() => Trigger.ToString();
    }

    public static class SkillTargets
    {
        public static bool IsEnemySide(SkillTarget target)
        {
            return target == SkillTarget.FocusEnemy || target == SkillTarget.NearestEnemies || target == SkillTarget.AllEnemies
                || target == SkillTarget.RandomEnemies || target == SkillTarget.TriggerEnemy;
        }
    }

    public static class SkillTriggers
    {
        public static bool IsPassive(SkillTrigger trigger) => trigger != SkillTrigger.Active && trigger != SkillTrigger.Burst;

        /// <summary>TriggerCount 를 쓰는 횟수형 트리거인지.</summary>
        public static bool IsCounted(SkillTrigger trigger)
        {
            switch (trigger)
            {
                case SkillTrigger.ShotFired:
                case SkillTrigger.Hit:
                case SkillTrigger.CriticalHit:
                case SkillTrigger.WeakPointHit:
                case SkillTrigger.FullChargeHit:
                case SkillTrigger.Damaged:
                case SkillTrigger.Kill:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>TriggerStatus 가 필요한 트리거인지.</summary>
        public static bool WatchesStatus(SkillTrigger trigger)
        {
            return trigger == SkillTrigger.StatusGained || trigger == SkillTrigger.StatusMaxStacks || trigger == SkillTrigger.StatusExpired;
        }

        /// <summary>TriggerEnemy 대상이 생기는 트리거인지(맞은 적, 처치한 적).</summary>
        public static bool HasTriggerEnemy(SkillTrigger trigger)
        {
            switch (trigger)
            {
                case SkillTrigger.Hit:
                case SkillTrigger.CriticalHit:
                case SkillTrigger.WeakPointHit:
                case SkillTrigger.FullChargeHit:
                case SkillTrigger.LastBulletHit:
                case SkillTrigger.Kill:
                    return true;
                default:
                    return false;
            }
        }
    }
}
