using System.Collections.Generic;
using AimCombat.Core.Combat;
using UnityEngine;

namespace AimCombat.Data
{
    /// <summary>
    /// 상태효과 정의 에셋(SPEC 3.3). 실행할 때 Core 의 StatusDef 로 한 번 바꿔 두고 같은 인스턴스를 돌려 쓴다.
    /// 실행 중에 인스펙터에서 값을 바꾸면 OnValidate 가 그 인스턴스를 고쳐 바로 반영된다(3단계 튜닝 패널도 이 길을 쓴다).
    /// </summary>
    [CreateAssetMenu(menuName = "Aim Combat/Status", fileName = "Status_")]
    public sealed class StatusData : ScriptableObject
    {
        private static readonly Dictionary<StatusDef, StatusData> byDef = new Dictionary<StatusDef, StatusData>();

        [SerializeField] private string displayName = "";
        [Tooltip("같은 효과가 다시 걸릴 때의 규칙.")]
        [SerializeField] private StatusRule rule;
        [Tooltip("바꾸는 능력치. 지속 피해와 행동 불가는 None.")]
        [SerializeField] private StatKind stat;
        [Tooltip("능력치 효과: 중첩 하나당 올리는 양(0.1 = 10%). 지속 피해: 틱 하나의 스킬 계수. 행동 불가: 보스 저항 게이지를 깎는 양.")]
        [SerializeField] private float value;
        [Tooltip("지속 시간(초).")]
        [SerializeField] private float duration = 5f;
        [Tooltip("지속형. 시간이 지나도 풀리지 않는다(대상이 쓰러지거나 스킬이 지울 때만 없어진다). 켜면 지속 시간은 쓰지 않는다.")]
        [SerializeField] private bool permanent;
        [Tooltip("중첩형의 최대 중첩 수.")]
        [Min(1)]
        [SerializeField] private int maxStacks = 1;
        [Tooltip("지속 피해의 틱 간격(초).")]
        [SerializeField] private float tickInterval = 1f;
        [Tooltip("행동 불가가 풀린 뒤 같은 효과에 면역인 시간(초).")]
        [SerializeField] private float immunityAfter;
        [Tooltip("HUD 와 적 색 표시에 쓰는 색.")]
        [SerializeField] private Color color = Color.white;

        private StatusDef def;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Color Color => color;

        /// <summary>Core 정의. 처음 부를 때 만들고 이후에는 같은 인스턴스를 돌려준다.</summary>
        public StatusDef ToCore()
        {
            if (def == null)
            {
                def = new StatusDef();
                CopyTo(def);
                byDef[def] = this;
            }
            return def;
        }

        /// <summary>Core 정의로 원래 에셋을 찾는다(색, 표시 이름). 데이터에서 만들지 않은 정의(그로기 등)는 null.</summary>
        public static StatusData Find(StatusDef coreDef)
        {
            return coreDef != null && byDef.TryGetValue(coreDef, out StatusData data) ? data : null;
        }

        /// <summary>검증용으로 캐시와 상관없이 새로 만든다.</summary>
        public StatusDef BuildFresh()
        {
            var fresh = new StatusDef();
            CopyTo(fresh);
            return fresh;
        }

        private void CopyTo(StatusDef target)
        {
            target.Id = name;
            target.Name = DisplayName;
            target.Rule = rule;
            target.Stat = stat;
            target.Value = FixedConvert.ToFixed(value);
            target.Duration = FixedConvert.ToFixed(duration);
            target.Permanent = permanent;
            target.MaxStacks = maxStacks;
            target.TickInterval = FixedConvert.ToFixed(tickInterval);
            target.ImmunityAfter = FixedConvert.ToFixed(immunityAfter);
        }

        private void OnValidate()
        {
            if (def != null) CopyTo(def);
        }
    }
}
