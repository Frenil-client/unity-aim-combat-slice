using AimCombat.Core.Combat;
using UnityEngine;

namespace AimCombat.Data
{
    /// <summary>
    /// 적 정의 에셋(SPEC 3.4). 체력, 방어력, 공격, 이동과 행동 불가를 받는 방식, 보스 패턴 수치를 둔다.
    /// 행동 불가 저항 게이지를 켜면 기절이 걸리지 않고 게이지가 깎이며, 다 깎이면 그로기에 빠진다(SPEC 3.3 보스 예외).
    /// </summary>
    [CreateAssetMenu(menuName = "Aim Combat/Enemy", fileName = "Enemy_")]
    public sealed class EnemyData : ScriptableObject
    {
        [SerializeField] private string displayName = "";
        [SerializeField] private float maxHealth = 160f;
        [Tooltip("방어력. 아군 공격력에서 뺀다(SPEC 3.1).")]
        [SerializeField] private float defense;

        [Header("공격")]
        [Tooltip("돌격형은 근접 한 번, 원거리형과 보스 탄막은 탄환 한 발의 피해.")]
        [SerializeField] private float attackDamage = 8f;
        [SerializeField] private float attackInterval = 1.2f;
        [SerializeField] private float moveSpeed = 2.2f;

        [Header("행동 불가 저항(보스)")]
        [Tooltip("켜면 기절 대신 저항 게이지가 깎인다.")]
        [SerializeField] private bool useControlGauge;
        [SerializeField] private float controlGauge = 60f;
        [Tooltip("게이지가 다 깎였을 때 그로기 시간(초).")]
        [SerializeField] private float breakDuration = 4f;

        [Header("보스 패턴")]
        [Tooltip("패턴 하나가 끝나고 다음 패턴까지 쉬는 시간(초).")]
        [SerializeField] private float patternInterval = 3f;
        [Tooltip("탄막 한 번에 쏘는 탄 수.")]
        [SerializeField] private int barrageShots = 10;
        [SerializeField] private float barrageShotInterval = 0.15f;
        [Tooltip("강타 예고 시간(초). 이 동안 약점을 맞히면 패턴이 취소된다.")]
        [SerializeField] private float slamTelegraph = 3f;
        [SerializeField] private float slamDamage = 240f;
        [Tooltip("예고 중 약점에 이만큼 피해를 주면 강타가 취소된다.")]
        [SerializeField] private float slamCancelDamage = 500f;
        [Tooltip("강타가 취소된 뒤 비틀거리는 시간(초).")]
        [SerializeField] private float staggerDuration = 2f;
        [Tooltip("돌진 속도.")]
        [SerializeField] private float lungeSpeed = 18f;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public float MaxHealth => maxHealth;
        public float AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public float MoveSpeed => moveSpeed;
        public float PatternInterval => patternInterval;
        public int BarrageShots => barrageShots;
        public float BarrageShotInterval => barrageShotInterval;
        public float SlamTelegraph => slamTelegraph;
        public float SlamDamage => slamDamage;
        public float SlamCancelDamage => slamCancelDamage;
        public float StaggerDuration => staggerDuration;
        public float LungeSpeed => lungeSpeed;

        /// <summary>이 정의로 전투 단위를 하나 만든다. 적은 공격력을 쓰지 않는다(아군 피해는 공격 수치를 그대로 준다).</summary>
        public Combatant CreateCombatant(string unitName)
        {
            var combatant = new Combatant(unitName, FixedConvert.ToFixed(maxHealth), FixedConvert.ToFixed(attackDamage), FixedConvert.ToFixed(defense));
            if (useControlGauge) combatant.UseControlGauge(FixedConvert.ToFixed(controlGauge), FixedConvert.ToFixed(breakDuration));
            return combatant;
        }
    }
}
