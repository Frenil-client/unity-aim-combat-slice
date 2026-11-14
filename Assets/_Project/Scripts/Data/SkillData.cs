using System;
using System.Collections.Generic;
using AimCombat.Core.Combat;
using UnityEngine;

namespace AimCombat.Data
{
    [Serializable]
    public sealed class SkillEffectEntry
    {
        public SkillEffectKind kind;
        public SkillTarget target;
        [Tooltip("가까운 적 몇 기(NearestEnemies).")]
        [Min(1)]
        public int count = 1;
        [Tooltip("피해: 공격력의 배수. 회복: 대상 최대 체력의 비율.")]
        public float coefficient;
        [Tooltip("ApplyStatus 가 거는 상태효과.")]
        public StatusData status;
    }

    /// <summary>
    /// 스킬 정의 에셋(SPEC 3.2). 트리거와 효과 목록의 조합이다. 새 스킬은 이 에셋을 하나 더 만들고 분대원에 연결하면 된다.
    /// 실행할 때 Core 의 SkillDef 로 한 번 바꿔 두고 같은 인스턴스를 돌려 쓴다. 실행 중 인스펙터 수정은 바로 반영된다.
    /// </summary>
    [CreateAssetMenu(menuName = "Aim Combat/Skill", fileName = "Skill_")]
    public sealed class SkillData : ScriptableObject
    {
        [SerializeField] private string displayName = "";
        [TextArea(2, 4)]
        [SerializeField] private string description = "";
        [SerializeField] private SkillTrigger trigger;
        [Tooltip("쿨다운(초). 쓰는 순간부터 센다. 버스트는 보통 20, 40, 60초.")]
        [SerializeField] private float cooldown = 10f;
        [Header("버스트")]
        [Tooltip("캐릭터 고유의 버스트 단계(1~3). 연계에서 이 단계가 열렸을 때만 쓸 수 있다. 버스트 트리거만 쓴다.")]
        [Range(1, 3)]
        [SerializeField] private int burstStage = 1;
        [Tooltip("이 버스트를 쓴 뒤 다음 단계 창이 열려 있는 시간(초). 3단계는 쓰지 않는다.")]
        [SerializeField] private float nextStageWindow = 10f;
        [Tooltip("효과 목록. 실행은 종류별 단계 순서(상태효과, 피해, 회복, 탄창)를 따르고 같은 종류 안에서는 이 순서를 따른다.")]
        [SerializeField] private List<SkillEffectEntry> effects = new List<SkillEffectEntry>();

        private SkillDef def;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Description => description;
        public SkillTrigger Trigger => trigger;
        public int BurstStage => burstStage;

        public SkillDef ToCore()
        {
            if (def == null)
            {
                def = new SkillDef();
                CopyTo(def, cached: true);
            }
            return def;
        }

        /// <summary>검증용으로 캐시와 상관없이 새로 만든다. 상태효과도 새로 만든다.</summary>
        public SkillDef BuildFresh()
        {
            var fresh = new SkillDef();
            CopyTo(fresh, cached: false);
            return fresh;
        }

        public List<string> Validate() => SkillValidator.Validate(BuildFresh());

        private void CopyTo(SkillDef target, bool cached)
        {
            target.Id = name;
            target.Name = DisplayName;
            target.Trigger = trigger;
            target.Cooldown = FixedConvert.ToFixed(cooldown);
            target.BurstStage = trigger == SkillTrigger.Burst ? burstStage : 0;
            target.NextStageWindow = FixedConvert.ToFixed(nextStageWindow);
            target.Effects.Clear();
            foreach (SkillEffectEntry entry in effects)
            {
                if (entry == null) continue;
                StatusDef status = null;
                if (entry.status != null) status = cached ? entry.status.ToCore() : entry.status.BuildFresh();
                target.Effects.Add(new SkillEffectDef
                {
                    Kind = entry.kind,
                    Target = entry.target,
                    Count = entry.count,
                    Coefficient = FixedConvert.ToFixed(entry.coefficient),
                    Status = status,
                });
            }
        }

        private void OnValidate()
        {
            if (def != null) CopyTo(def, cached: true);
        }
    }
}
