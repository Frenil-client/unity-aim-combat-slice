using System.Collections.Generic;

namespace AimCombat.Core.Combat
{
    /// <summary>스킬이 발동하는 계기(SPEC 3.2).</summary>
    public enum SkillTrigger
    {
        /// <summary>쿨다운이 차면 쓸 수 있는 액티브 스킬.</summary>
        Active,
        /// <summary>버스트. 연계에서 자기 버스트 단계가 열렸을 때만 쓸 수 있고 쿨다운이 있다(니케 버스트 구조).</summary>
        Burst,
    }

    /// <summary>효과를 받을 대상을 고르는 방법.</summary>
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
    }

    /// <summary>효과의 종류. 실행 순서도 이 순서다(SkillRunner).</summary>
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
    }

    public sealed class SkillEffectDef
    {
        public SkillEffectKind Kind;
        public SkillTarget Target;
        /// <summary>NearestEnemies 의 대상 수.</summary>
        public int Count = 1;
        public Fixed Coefficient;
        /// <summary>ApplyStatus 가 거는 효과.</summary>
        public StatusDef Status;

        public bool TargetsEnemies => SkillTargets.IsEnemySide(Target);
    }

    /// <summary>
    /// 스킬 하나의 정의. 트리거와 효과 목록의 조합이고 데이터(Unity 의 SkillData 에셋)에서 만들어진다.
    /// 새 스킬은 이 조합을 데이터로 새로 짜서 만든다. 코드를 고치지 않는다(SPEC 3.2).
    /// </summary>
    public sealed class SkillDef
    {
        public string Id = "";
        public string Name = "";
        public SkillTrigger Trigger;
        /// <summary>쿨다운(초). 액티브와 버스트 모두 쓴다.</summary>
        public Fixed Cooldown;
        /// <summary>버스트 단계(1~3). Burst 트리거만 쓴다. 캐릭터 고유의 값이다.</summary>
        public int BurstStage;
        /// <summary>이 버스트를 쓴 뒤 다음 단계 창이 열려 있는 시간(초). 3단계는 쓰지 않는다(풀 버스트로 넘어간다).</summary>
        public Fixed NextStageWindow;
        public readonly List<SkillEffectDef> Effects = new List<SkillEffectDef>();

        public override string ToString() => string.IsNullOrEmpty(Name) ? Id : Name;
    }

    public static class SkillTargets
    {
        public static bool IsEnemySide(SkillTarget target)
        {
            return target == SkillTarget.FocusEnemy || target == SkillTarget.NearestEnemies || target == SkillTarget.AllEnemies;
        }
    }
}
