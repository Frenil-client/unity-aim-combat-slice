using System;
using AimCombat.Core.Combat;
using AimCombat.Data;
using UnityEngine;

namespace AimCombat.Battle
{
    public enum BossPhase
    {
        /// <summary>등장 위치에서 전투 위치로 걸어 나온다.</summary>
        Approach,
        /// <summary>다음 패턴을 기다린다.</summary>
        Idle,
        /// <summary>탄막: 짧은 간격으로 아군들에게 탄환을 연달아 쏜다.</summary>
        Barrage,
        /// <summary>강타 예고. 이 동안 약점에 일정 피해를 주면 패턴이 취소된다.</summary>
        SlamTelegraph,
        /// <summary>돌진해 노린 아군을 강타한다.</summary>
        SlamLunge,
        /// <summary>강타 뒤 제자리로 돌아간다.</summary>
        Return,
        /// <summary>강타가 취소돼 비틀거린다.</summary>
        Stagger,
        /// <summary>저항 게이지가 다 깎여 그로기 상태다(Core 의 행동 불가).</summary>
        Broken,
    }

    /// <summary>
    /// 보스 행동(SPEC 3.4). 패턴 2개(탄막, 돌진 예고 후 강타)를 번갈아 쓴다.
    /// - 강타 예고 중 약점에 slamCancelDamage 만큼 피해를 주면 패턴이 취소되고 비틀거린다.
    /// - 기절은 걸리지 않고 저항 게이지가 깎인다. 게이지가 다 깎이면 그로기에 빠지고 진행 중인 패턴이 끊긴다(SPEC 3.3).
    /// 수치는 EnemyData 의 보스 패턴 항목에서 읽는다.
    /// </summary>
    [RequireComponent(typeof(EnemyUnit))]
    public sealed class BossBrain : MonoBehaviour
    {
        [Tooltip("약점 부위의 렌더러. 강타 예고 중에 빛난다.")]
        [SerializeField] private Renderer weakPointRenderer;
        [SerializeField] private Color weakPointIdleColor = new Color(1f, 0.6f, 0.15f);
        [SerializeField] private Color weakPointGlowColor = new Color(1f, 1f, 0.6f);
        [Tooltip("강타가 떨어질 자리 표시. 예고 중에 노린 아군의 엄폐물 앞에 놓인다.")]
        [SerializeField] private Transform slamMarker;
        [Tooltip("전투 위치의 깊이(z). 등장한 뒤 여기까지 걸어 나와 패턴을 쓴다.")]
        [SerializeField] private float battleZ = 18f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private EnemyUnit unit;
        private EnemyData data;
        private MaterialPropertyBlock block;
        private Vector3 home;
        private float phaseEnd;
        private float nextShotTime;
        private int shotsLeft;
        private bool nextIsSlam;
        private float weakPointDamage;
        private Vector3 lungeGoal;

        public BossPhase Phase { get; private set; }
        public AllyUnit SlamTarget { get; private set; }

        /// <summary>강타 예고의 진행 0~1.</summary>
        public float TelegraphProgress { get; private set; }

        /// <summary>강타 취소까지 약점에 준 피해의 비율 0~1.</summary>
        public float CancelProgress => data != null && data.SlamCancelDamage > 0f ? Mathf.Clamp01(weakPointDamage / data.SlamCancelDamage) : 0f;

        /// <summary>몸 위에 얹는 경고색. 알파가 섞는 양이다.</summary>
        public Color WarningColor { get; private set; }

        /// <summary>강타가 약점 사격으로 취소됐다.</summary>
        public event Action<BossBrain> SlamCancelled;

        /// <summary>패턴이 바뀌었다.</summary>
        public event Action<BossBrain, BossPhase> PhaseChanged;

        public void Begin(EnemyUnit owner)
        {
            unit = owner;
            data = owner.Data;
            block = new MaterialPropertyBlock();
            home = new Vector3(owner.transform.position.x, owner.transform.position.y, battleZ);
            unit.Combatant.Damaged += OnDamaged;
            unit.Combatant.Broken += _ => Enter(BossPhase.Broken, 0f);
            if (slamMarker != null) slamMarker.gameObject.SetActive(false);
            Enter(BossPhase.Approach, 0f);
        }

        public void Tick()
        {
            if (unit == null) return;

            // 그로기(행동 불가) 중에는 아무것도 하지 않는다. 풀리면 제자리로 돌아간다.
            if (unit.Combatant.IsControlled)
            {
                if (Phase != BossPhase.Broken) Enter(BossPhase.Broken, 0f);
                UpdateVisuals();
                return;
            }

            switch (Phase)
            {
                case BossPhase.Approach:
                    if (MoveTo(home, data.MoveSpeed * unit.MoveMultiplier)) Enter(BossPhase.Idle, data.PatternInterval * 0.5f);
                    break;

                case BossPhase.Idle:
                    if (Time.time >= phaseEnd) StartNextPattern();
                    break;

                case BossPhase.Barrage:
                    if (Time.time >= nextShotTime)
                    {
                        AllyUnit ally = unit.PickTarget();
                        if (ally != null) unit.FireProjectile(ally, data.AttackDamage);
                        unit.Flash(0.4f);
                        shotsLeft--;
                        nextShotTime = Time.time + data.BarrageShotInterval;
                        if (shotsLeft <= 0) Enter(BossPhase.Idle, data.PatternInterval);
                    }
                    break;

                case BossPhase.SlamTelegraph:
                    if (SlamTarget == null || SlamTarget.IsDown) SlamTarget = unit.PickTarget();
                    if (SlamTarget == null)
                    {
                        Enter(BossPhase.Idle, data.PatternInterval);
                        break;
                    }
                    PlaceMarker();
                    TelegraphProgress = Mathf.Clamp01(1f - (phaseEnd - Time.time) / Mathf.Max(0.01f, data.SlamTelegraph));
                    if (Time.time >= phaseEnd)
                    {
                        lungeGoal = new Vector3(SlamTarget.transform.position.x, transform.position.y, unit.AttackLineZ);
                        Enter(BossPhase.SlamLunge, 0f);
                    }
                    break;

                case BossPhase.SlamLunge:
                    if (MoveTo(lungeGoal, data.LungeSpeed))
                    {
                        if (SlamTarget != null && !SlamTarget.IsDown) SlamTarget.TakeDamage(data.SlamDamage);
                        unit.Flash(1f);
                        Enter(BossPhase.Return, 0f);
                    }
                    break;

                case BossPhase.Return:
                case BossPhase.Broken:
                    if (MoveTo(home, data.LungeSpeed * 0.35f)) Enter(BossPhase.Idle, data.PatternInterval);
                    break;

                case BossPhase.Stagger:
                    if (Time.time >= phaseEnd) Enter(BossPhase.Return, 0f);
                    break;
            }

            UpdateVisuals();
        }

        private void StartNextPattern()
        {
            if (nextIsSlam)
            {
                SlamTarget = unit.PickTarget();
                weakPointDamage = 0f;
                TelegraphProgress = 0f;
                Enter(BossPhase.SlamTelegraph, data.SlamTelegraph);
            }
            else
            {
                shotsLeft = data.BarrageShots;
                nextShotTime = Time.time;
                Enter(BossPhase.Barrage, 0f);
            }
            nextIsSlam = !nextIsSlam;
        }

        // 약점 사격 피해를 예고 중에만 모은다. 스킬과 지속 피해는 약점 판정이 없어 세지 않는다.
        private void OnDamaged(DamageEvent e)
        {
            if (Phase != BossPhase.SlamTelegraph || !e.WeakPoint) return;
            weakPointDamage += FixedConvert.ToFloat(e.Amount);
            if (weakPointDamage < data.SlamCancelDamage) return;

            SlamCancelled?.Invoke(this);
            Enter(BossPhase.Stagger, data.StaggerDuration);
        }

        private void Enter(BossPhase phase, float duration)
        {
            Phase = phase;
            phaseEnd = Time.time + duration;
            if (phase != BossPhase.SlamTelegraph) TelegraphProgress = 0f;
            if (slamMarker != null) slamMarker.gameObject.SetActive(phase == BossPhase.SlamTelegraph || phase == BossPhase.SlamLunge);
            PhaseChanged?.Invoke(this, phase);
        }

        private bool MoveTo(Vector3 goal, float speed)
        {
            transform.position = Vector3.MoveTowards(transform.position, goal, speed * Time.deltaTime);
            return (transform.position - goal).sqrMagnitude < 0.0004f;
        }

        private void PlaceMarker()
        {
            if (slamMarker == null || SlamTarget == null) return;
            Vector3 p = SlamTarget.transform.position;
            slamMarker.position = new Vector3(p.x, 0.02f, unit.AttackLineZ);
        }

        // 예고 중에는 몸이 붉게 맥동하고 약점이 밝게 빛난다. 비틀거릴 때는 몸을 기울인다.
        private void UpdateVisuals()
        {
            bool telegraph = Phase == BossPhase.SlamTelegraph;
            float pulse = Mathf.PingPong(Time.time * (2f + 6f * TelegraphProgress), 1f);
            WarningColor = telegraph ? new Color(1f, 0.1f, 0.1f, 0.25f + 0.35f * pulse) : Color.clear;

            if (weakPointRenderer != null)
            {
                Color c = telegraph ? Color.Lerp(weakPointIdleColor, weakPointGlowColor, pulse) : weakPointIdleColor;
                block.SetColor(BaseColorId, c);
                weakPointRenderer.SetPropertyBlock(block);
            }

            float tilt = Phase == BossPhase.Stagger ? Mathf.Sin(Time.time * 18f) * 8f : 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);
        }
    }
}
