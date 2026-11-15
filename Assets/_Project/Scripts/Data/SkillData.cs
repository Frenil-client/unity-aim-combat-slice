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
        [Tooltip("대상 적 수(NearestEnemies, RandomEnemies).")]
        [Min(1)]
        public int count = 1;
        [Tooltip("피해: 공격력의 배수. 회복: 대상 최대 체력의 비율.")]
        public float coefficient;
        [Tooltip("ApplyStatus 가 걸거나 RemoveStatus 가 지우는 상태효과.")]
        public StatusData status;
        [Tooltip("비어 있지 않으면 이 상태효과가 걸린 대상만 받는다(예: 약점 개방 상태의 적에게).")]
        public StatusData targetStatus;
        [Tooltip("피해를 약점(코어) 명중으로 친다. 주요 배율에 약점 항목이 들어간다.")]
        public bool coreHit;
    }

    /// <summary>
    /// 발동 블록 하나. 트리거, 발동 조건, 효과 목록의 조합이다. 니케 스킬 설명의 "■ ~ 시 발동" 한 덩어리에 해당한다.
    /// </summary>
    [Serializable]
    public sealed class SkillBlockEntry
    {
        public SkillTrigger trigger;
        [Tooltip("Active: 쿨다운(초). 패시브: 한 번 발동한 뒤 다시 발동하기까지의 최소 간격(0이면 없음). Burst 는 쓰지 않는다.")]
        public float cooldown;

        [Tooltip("횟수형 트리거(사격, 명중, 치명, 약점, 풀 차지, 피격, 처치)가 몇 회마다 발동하는지. 1이면 매번.")]
        [Min(1)]
        public int triggerCount = 1;
        [Tooltip("Periodic 의 반복 주기(초).")]
        public float interval;
        [Tooltip("아래 상태효과가 자신에게 걸려 있는 동안 쓰는 주기(초). '반복 주기 감소' 버프를 이렇게 표현한다.")]
        public float altInterval;
        public StatusData altIntervalStatus;
        [Tooltip("StatusGained, StatusMaxStacks, StatusExpired 가 지켜보는 상태효과.")]
        public StatusData triggerStatus;
        [Tooltip("HealthBelow 의 체력 비율(0~1).")]
        [Range(0f, 1f)]
        public float healthThreshold = 0.5f;

        [Tooltip("발동 확률(0~1). 1이면 늘 발동. 패시브만 쓴다.")]
        [Range(0f, 1f)]
        public float chance = 1f;
        [Tooltip("자신에게 이 상태효과가 걸려 있어야 발동한다. 조건이 맞지 않는 동안은 횟수도 세지 않는다.")]
        public StatusData requireSelfStatus;
        [Tooltip("자신에게 이 상태효과가 걸려 있으면 발동하지 않는다.")]
        public StatusData forbidSelfStatus;
        [Tooltip("풀 버스트 타임 여부 조건.")]
        public BurstCondition burstState;

        [Tooltip("효과 목록. 실행은 종류별 단계 순서(상태효과 지우기, 상태효과, 피해, 회복, 탄창)를 따르고 같은 종류 안에서는 이 순서를 따른다.")]
        public List<SkillEffectEntry> effects = new List<SkillEffectEntry>();
    }

    /// <summary>
    /// 스킬 정의 에셋(SPEC 3.2). 캐릭터의 스킬 1, 스킬 2, 버스트 스킬 중 하나이고 발동 블록의 목록이다.
    /// 발동 시점이 여러 개인 스킬은 블록을 여러 개 둔다. 캐릭터 에셋(CharacterData)의 해당 칸에 연결한다.
    /// 실행할 때 Core 의 SkillDef 로 한 번 바꿔 두고 같은 인스턴스를 돌려 쓴다.
    /// 실행 중 인스펙터에서 값을 고치면 바로 반영된다. 블록을 더하거나 빼는 것은 다음 전투부터 반영된다.
    /// </summary>
    [CreateAssetMenu(menuName = "Aim Combat/Skill", fileName = "Skill_")]
    public sealed class SkillData : ScriptableObject
    {
        private static readonly Dictionary<SkillDef, SkillData> byDef = new Dictionary<SkillDef, SkillData>();

        [SerializeField] private string displayName = "";
        [TextArea(2, 4)]
        [Tooltip("{interval}, {Effect_1_count}, {Effect_1_status.displayName}, {Block_2_chance:percent}\n블록과 효과는 화면 순서대로 1부터 센다. Block_ 접두사를 생략하면 첫 블록이다. 순서를 바꾸면 템플릿의 번호를 확인한다.")]
        [SerializeField] private string description = "";
        [Tooltip("이 스킬이 들어갈 칸. 캐릭터 에셋의 같은 칸에만 연결한다.")]
        [SerializeField] private SkillSlot slot;

        [Header("버스트 스킬 칸만 쓴다")]
        [Tooltip("버스트 쿨다운(초). 쓰는 순간부터 센다. 보통 20, 40, 60초.")]
        [SerializeField] private float cooldown = 20f;
        [Tooltip("캐릭터 고유의 버스트 단계(1~3). 연계에서 이 단계가 열렸을 때만 쓸 수 있다.")]
        [Range(1, 3)]
        [SerializeField] private int burstStage = 1;
        [Tooltip("이 버스트를 쓴 뒤 다음 단계 창이 열려 있는 시간(초). 3단계는 쓰지 않는다.")]
        [SerializeField] private float nextStageWindow = 10f;

        [Tooltip("발동 블록 목록. 블록마다 트리거, 조건, 효과를 따로 가진다.")]
        [SerializeField] private List<SkillBlockEntry> blocks = new List<SkillBlockEntry>();

        private SkillDef def;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Description => string.IsNullOrEmpty(description) || description.IndexOf('{') < 0
            ? description : SkillDescription.Resolve(description, ToCore());
        public string DescriptionTemplate => description;
        public SkillSlot Slot => slot;
        public bool IsBurst => slot == SkillSlot.Burst;
        public int BurstStage => burstStage;
        public int BlockCount => blocks.Count;

        public SkillDef ToCore()
        {
            if (def == null)
            {
                def = new SkillDef();
                CopyTo(def, cached: true);
                byDef[def] = this;
            }
            return def;
        }

        /// <summary>Core 정의로 원래 에셋을 찾는다(표시 이름, 설명). 데이터에서 만들지 않은 정의는 null.</summary>
        public static SkillData Find(SkillDef coreDef)
        {
            return coreDef != null && byDef.TryGetValue(coreDef, out SkillData data) ? data : null;
        }

        /// <summary>검증용으로 캐시와 상관없이 새로 만든다. 상태효과도 새로 만든다.</summary>
        public SkillDef BuildFresh() => BuildFresh(null);

        public SkillDef BuildFresh(Func<StatusData, StatusDef> buildStatus)
        {
            var fresh = new SkillDef();
            CopyTo(fresh, cached: false, buildStatus);
            return fresh;
        }

        public List<string> Validate() => SkillValidator.Validate(BuildFresh());

        private void CopyTo(SkillDef target, bool cached, Func<StatusData, StatusDef> buildStatus = null)
        {
            // 검증용 빌드에서는 상태효과를 새로 만들되 같은 에셋은 같은 인스턴스로 맞춘다.
            // 그래야 "필요 조건과 금지 조건이 같은 효과다" 같은 비교가 맞는다.
            Dictionary<StatusData, StatusDef> freshCache = cached ? null : new Dictionary<StatusData, StatusDef>();
            StatusDef Convert(StatusData data)
            {
                if (data == null) return null;
                if (cached) return data.ToCore();
                if (!freshCache.TryGetValue(data, out StatusDef built))
                {
                    built = buildStatus != null ? buildStatus(data) : data.BuildFresh();
                    freshCache[data] = built;
                }
                return built;
            }

            target.Id = name;
            target.Name = DisplayName;
            target.Slot = slot;
            target.Cooldown = FixedConvert.ToFixed(cooldown);
            target.BurstStage = IsBurst ? burstStage : 0;
            target.NextStageWindow = FixedConvert.ToFixed(nextStageWindow);

            // 블록 수가 그대로면 같은 인스턴스를 고쳐 쓴다. 전투 중에 등록된 패시브가 고친 값을 바로 읽는다.
            // 수가 바뀌면 새로 만든다. 이미 등록된 패시브는 예전 블록을 계속 쓰고 다음 전투부터 새 구성이 적용된다.
            if (target.Blocks.Count != blocks.Count)
            {
                target.Blocks.Clear();
                for (int i = 0; i < blocks.Count; i++) target.Blocks.Add(new SkillBlockDef());
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                SkillBlockEntry entry = blocks[i];
                SkillBlockDef block = target.Blocks[i];
                if (entry == null) continue;

                block.Trigger = entry.trigger;
                block.Cooldown = FixedConvert.ToFixed(entry.cooldown);
                block.TriggerCount = entry.triggerCount;
                block.Interval = FixedConvert.ToFixed(entry.interval);
                block.AltInterval = FixedConvert.ToFixed(entry.altInterval);
                block.AltIntervalStatus = Convert(entry.altIntervalStatus);
                block.TriggerStatus = Convert(entry.triggerStatus);
                block.HealthThreshold = FixedConvert.ToFixed(entry.healthThreshold);
                block.Chance = FixedConvert.ToFixed(entry.chance);
                block.RequireSelfStatus = Convert(entry.requireSelfStatus);
                block.ForbidSelfStatus = Convert(entry.forbidSelfStatus);
                block.BurstState = entry.burstState;

                block.Effects.Clear();
                foreach (SkillEffectEntry effect in entry.effects)
                {
                    if (effect == null) continue;
                    block.Effects.Add(new SkillEffectDef
                    {
                        Kind = effect.kind,
                        Target = effect.target,
                        Count = effect.count,
                        Coefficient = FixedConvert.ToFixed(effect.coefficient),
                        Status = Convert(effect.status),
                        TargetStatus = Convert(effect.targetStatus),
                        CoreHit = effect.coreHit,
                    });
                }
            }
        }

        private void OnValidate()
        {
            if (def != null) CopyTo(def, cached: true);
        }
    }
}
