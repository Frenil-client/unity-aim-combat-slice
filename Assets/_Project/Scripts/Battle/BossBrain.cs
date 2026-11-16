using System;
using System.Collections.Generic;
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
        /// <summary>강타 예고. 이 동안 저지 표적을 모두 부수면 패턴이 취소된다.</summary>
        SlamTelegraph,
        /// <summary>돌진해 노린 아군을 강타한다.</summary>
        SlamLunge,
        /// <summary>강타 뒤 제자리로 돌아간다.</summary>
        Return,
        /// <summary>강타가 취소됐거나 탄막 중 팔이 부서져 비틀거린다.</summary>
        Stagger,
        /// <summary>저항 게이지가 다 깎여 그로기 상태다(Core 의 행동 불가).</summary>
        Broken,
    }

    /// <summary>
    /// 보스 행동(SPEC 3.4). 패턴 2개(탄막, 돌진 예고 후 강타)를 번갈아 쓴다.
    /// - 탄막은 살아 있는 팔의 포문에서 번갈아 쏜다. 팔이 부서진 만큼 탄 수가 줄고, 양팔이 다 부서지면 강타만 쓴다.
    /// - 강타 예고가 시작되면 저지 표적을 띄운다(살아 있는 팔 먼저, 모자라면 본체의 저지 지점). 예고 중 모두 깎으면 패턴이 취소되고 비틀거린다.
    /// - 팔이 부서지면 저항 게이지가 깎이고, 탄막 중이었다면 탄막이 끊긴다.
    /// - 기절은 걸리지 않고 저항 게이지가 깎인다. 게이지가 다 깎이면 그로기에 빠지고 진행 중인 패턴이 끊긴다(SPEC 3.3).
    /// 수치는 EnemyData 의 보스 패턴, 저지, 부위 항목에서 읽는다.
    /// </summary>
    [RequireComponent(typeof(EnemyUnit))]
    public sealed class BossBrain : MonoBehaviour
    {
        [Tooltip("부술 수 있는 팔. 탄막을 쏘고, 저지 표적이 먼저 뜬다.")]
        [SerializeField] private BossPart[] arms = new BossPart[0];
        [Tooltip("본체의 저지 지점. 살아 있는 팔이 모자랄 때 저지 표적이 뜬다.")]
        [SerializeField] private BossPart[] interruptPoints = new BossPart[0];
        [Tooltip("강타가 떨어질 자리 표시. 예고 중에 노린 아군의 엄폐물 앞에 놓인다.")]
        [SerializeField] private Transform slamMarker;
        [Tooltip("전투 위치의 깊이(z). 등장한 뒤 여기까지 걸어 나와 패턴을 쓴다.")]
        [SerializeField] private float battleZ = 18f;
        [Tooltip("경고 표시를 띄우는 높이(발 기준).")]
        [SerializeField] private float warningHeight = 3.6f;

        private readonly List<BossPart> parts = new List<BossPart>();
        private readonly List<BossPart> marks = new List<BossPart>();

        private EnemyUnit unit;
        private EnemyData data;
        private Vector3 home;
        private float phaseEnd;
        private float nextShotTime;
        private int shotsLeft;
        private bool nextIsSlam;
        private int nextArm;
        private Vector3 lungeGoal;

        public BossPhase Phase { get; private set; }
        public AllyUnit SlamTarget { get; private set; }

        /// <summary>강타 예고의 진행 0~1.</summary>
        public float TelegraphProgress { get; private set; }

        /// <summary>강타 예고의 남은 시간(초). 예고 중이 아니면 0.</summary>
        public float TelegraphTimeLeft => Phase == BossPhase.SlamTelegraph ? Mathf.Max(0f, phaseEnd - Time.time) : 0f;

        /// <summary>팔과 본체 저지 지점 전부. 부위 표시가 읽는다.</summary>
        public IReadOnlyList<BossPart> Parts => parts;

        /// <summary>경고 표시를 띄울 지점(머리 위).</summary>
        public Vector3 WarningPoint => transform.position + Vector3.up * warningHeight;

        /// <summary>이번 예고에 뜬 저지 표적 중 아직 남은 수.</summary>
        public int MarksLeft
        {
            get
            {
                int left = 0;
                foreach (BossPart mark in marks) if (mark.Marked) left++;
                return left;
            }
        }

        /// <summary>강타 저지의 진행 0~1. 이번 예고에 뜬 표적들의 저지 게이지가 깎인 비율이다.</summary>
        public float CancelProgress
        {
            get
            {
                float max = 0f, left = 0f;
                foreach (BossPart mark in marks)
                {
                    max += mark.MarkMaxHealth;
                    if (mark.Marked) left += mark.MarkHealth;
                }
                return max > 0f ? Mathf.Clamp01(1f - left / max) : 0f;
            }
        }

        /// <summary>몸 위에 얹는 경고색. 알파가 섞는 양이다.</summary>
        public Color WarningColor { get; private set; }

        /// <summary>저지 표적을 모두 부숴 강타가 취소됐다.</summary>
        public event Action<BossBrain> SlamCancelled;

        /// <summary>어느 보스든 강타가 떨어졌다. 화면 흔들림이 구독한다.</summary>
        public static event Action<BossBrain> SlamLanded;

        /// <summary>어느 보스든 팔이 부서졌다. 화면 연출과 알림이 구독한다.</summary>
        public static event Action<BossBrain, BossPart> PartDestroyed;

        /// <summary>패턴이 바뀌었다.</summary>
        public event Action<BossBrain, BossPhase> PhaseChanged;

        public void Begin(EnemyUnit owner)
        {
            unit = owner;
            data = owner.Data;
            home = new Vector3(owner.transform.position.x, owner.transform.position.y, battleZ);

            parts.Clear();
            foreach (BossPart arm in arms)
            {
                if (arm == null) continue;
                arm.Setup(data.PartHealth);
                arm.StageChanged += OnArmStageChanged;
                arm.MarkCleared += OnMarkCleared;
                parts.Add(arm);
            }
            foreach (BossPart point in interruptPoints)
            {
                if (point == null) continue;
                point.Setup(0f);
                point.MarkCleared += OnMarkCleared;
                parts.Add(point);
            }

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
                        // 살아 있는 팔의 포문에서 번갈아 쏜다. 팔을 두지 않은 보스는 몸의 총구에서 쏜다.
                        BossPart arm = NextArm();
                        if (arms.Length > 0 && arm == null)
                        {
                            Enter(BossPhase.Idle, data.PatternInterval);
                            break;
                        }
                        AllyUnit ally = unit.PickTarget();
                        if (ally != null) unit.FireProjectile(ally, data.AttackDamage, arm != null ? arm.Muzzle : null);
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
                        SlamLanded?.Invoke(this);
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
            // 양팔이 다 부서지면 탄막을 쓰지 못하고 강타만 쓴다.
            int armsAlive = ArmsAlive();
            bool slam = nextIsSlam || (arms.Length > 0 && armsAlive == 0);
            if (slam)
            {
                SlamTarget = unit.PickTarget();
                TelegraphProgress = 0f;
                Enter(BossPhase.SlamTelegraph, data.SlamTelegraph);
                PlaceMarks();
            }
            else
            {
                // 탄 수는 살아 있는 팔의 비율만큼이다(한쪽이 부서지면 절반).
                shotsLeft = arms.Length > 0 ? Mathf.CeilToInt(data.BarrageShots * armsAlive / (float)arms.Length) : data.BarrageShots;
                nextShotTime = Time.time;
                Enter(BossPhase.Barrage, 0f);
            }
            nextIsSlam = !slam;
        }

        private int ArmsAlive()
        {
            int alive = 0;
            foreach (BossPart arm in arms) if (arm != null && !arm.IsDestroyed) alive++;
            return alive;
        }

        // 다음에 쏠 팔. 살아 있는 팔을 차례로 돈다. 없으면 null.
        private BossPart NextArm()
        {
            for (int step = 0; step < arms.Length; step++)
            {
                int index = (nextArm + step) % arms.Length;
                BossPart arm = arms[index];
                if (arm == null || arm.IsDestroyed) continue;
                nextArm = (index + 1) % arms.Length;
                return arm;
            }
            return null;
        }

        // 저지 표적을 정해진 수만큼 띄운다. 살아 있는 팔에 먼저, 모자라면 본체의 저지 지점에.
        private void PlaceMarks()
        {
            marks.Clear();
            int need = Mathf.Max(1, data.InterruptTargets);
            foreach (BossPart arm in arms)
            {
                if (marks.Count >= need) break;
                if (arm == null || arm.IsDestroyed) continue;
                arm.Mark(data.InterruptHealth);
                marks.Add(arm);
            }
            foreach (BossPart point in interruptPoints)
            {
                if (marks.Count >= need) break;
                if (point == null) continue;
                point.Mark(data.InterruptHealth);
                marks.Add(point);
            }
        }

        private void ClearMarks()
        {
            foreach (BossPart mark in marks) mark.Unmark();
        }

        // 표적 하나가 다 깎였다. 이번 예고의 표적이 모두 깎였으면 강타를 취소하고 비틀거린다.
        private void OnMarkCleared(BossPart part)
        {
            if (Phase != BossPhase.SlamTelegraph || MarksLeft > 0) return;
            SlamCancelled?.Invoke(this);
            Enter(BossPhase.Stagger, data.StaggerDuration);
        }

        // 팔이 부서지면 저항 게이지가 깎인다(다 깎이면 그로기). 탄막 중이었다면 탄막이 끊기고 비틀거린다.
        private void OnArmStageChanged(BossPart part, BossPartStage stage)
        {
            unit.Flash(1f);
            if (stage != BossPartStage.Destroyed) return;

            PartDestroyed?.Invoke(this, part);
            unit.Combatant.CutControlGauge(FixedConvert.ToFixed(data.PartBreakGaugeCut));
            if (Phase == BossPhase.Barrage && !unit.Combatant.IsControlled) Enter(BossPhase.Stagger, data.StaggerDuration);
        }

        private void Enter(BossPhase phase, float duration)
        {
            Phase = phase;
            phaseEnd = Time.time + duration;
            if (phase != BossPhase.SlamTelegraph)
            {
                TelegraphProgress = 0f;
                ClearMarks();
            }
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

        // 예고 중에는 몸이 붉게 맥동한다. 비틀거릴 때는 몸을 기울인다.
        private void UpdateVisuals()
        {
            bool telegraph = Phase == BossPhase.SlamTelegraph;
            float pulse = Mathf.PingPong(Time.time * (2f + 6f * TelegraphProgress), 1f);
            WarningColor = telegraph ? new Color(1f, 0.1f, 0.1f, 0.25f + 0.35f * pulse) : Color.clear;

            float tilt = Phase == BossPhase.Stagger ? Mathf.Sin(Time.time * 18f) * 8f : 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);
        }
    }
}
