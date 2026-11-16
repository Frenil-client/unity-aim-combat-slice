using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 보스 모델의 애니메이션. BossBrain 의 패턴 단계가 바뀔 때마다 알맞은 동작으로 넘어간다.
    /// - 탄막: 날개를 펴고 반동을 반복한다. 강타 예고: 몸을 젖혀 힘을 모은 자세로 버틴다. 돌진: 충각을 내지른다.
    /// - 그로기 동안은 기본 동작을 느리게 튼다. 그 밖의 단계는 떠 있는 기본 동작이다.
    /// - 처치되면 EnemyUnit 이 오브젝트를 바로 지우므로, 모델만 떼어 내 패배 동작을 끝까지 보여 준 뒤 지운다.
    /// </summary>
    [RequireComponent(typeof(BossBrain), typeof(EnemyUnit))]
    public sealed class BossAnimator : MonoBehaviour
    {
        private static readonly int MoveState = Animator.StringToHash("Move");
        private static readonly int BarrageState = Animator.StringToHash("Barrage");
        private static readonly int SlamWindupState = Animator.StringToHash("SlamWindup");
        private static readonly int SlamStrikeState = Animator.StringToHash("SlamStrike");
        private static readonly int DefeatState = Animator.StringToHash("Defeat");

        [SerializeField] private Animator animator;
        [Tooltip("패배 동작을 보여 줄 모델 뿌리. 처치되면 보스에서 떼어 낸다.")]
        [SerializeField] private Transform model;
        [SerializeField] private float crossFade = 0.15f;
        [SerializeField] private float brokenSpeed = 0.35f;
        [Tooltip("떼어 낸 모델이 패배 동작을 보여 주고 사라지기까지의 시간(초).")]
        [SerializeField] private float defeatLifetime = 3.6f;

        private BossBrain brain;
        private EnemyUnit unit;
        private BossPhase shownPhase;
        private bool hasShown;

        private void Awake()
        {
            brain = GetComponent<BossBrain>();
            unit = GetComponent<EnemyUnit>();
            unit.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (unit != null) unit.Died -= OnDied;
        }

        private void Update()
        {
            if (animator == null || unit.IsDead) return;
            BossPhase phase = brain.Phase;
            animator.speed = phase == BossPhase.Broken ? brokenSpeed : 1f;
            if (hasShown && phase == shownPhase) return;

            int state = StateFor(phase);
            if (!hasShown || state != StateFor(shownPhase)) animator.CrossFadeInFixedTime(state, crossFade);
            shownPhase = phase;
            hasShown = true;
        }

        private static int StateFor(BossPhase phase)
        {
            switch (phase)
            {
                case BossPhase.Barrage: return BarrageState;
                case BossPhase.SlamTelegraph: return SlamWindupState;
                case BossPhase.SlamLunge: return SlamStrikeState;
                default: return MoveState;
            }
        }

        // 모델에 붙은 충돌체(몸통, 약점, 팔)는 꺼서 쓰러지는 동안 맞지 않게 한다.
        private void OnDied(EnemyUnit _)
        {
            if (model == null || animator == null) return;
            foreach (BossPart part in model.GetComponentsInChildren<BossPart>()) part.Unmark();
            foreach (Collider hit in model.GetComponentsInChildren<Collider>()) hit.enabled = false;
            model.SetParent(null, true);
            animator.speed = 1f;
            animator.CrossFadeInFixedTime(DefeatState, 0.1f);
            Destroy(model.gameObject, defeatLifetime);
        }
    }
}
