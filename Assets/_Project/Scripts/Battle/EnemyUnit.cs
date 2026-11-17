using System;
using System.Collections.Generic;
using AimCombat.Core.Combat;
using AimCombat.Data;
using AimCombat.Weapons;
using UnityEngine;

namespace AimCombat.Battle
{
    public enum EnemyKind
    {
        /// <summary>다가와 근접 공격. 수가 많다.</summary>
        Rusher,
        /// <summary>제자리에서 탄환 발사.</summary>
        Ranged,
        /// <summary>약점 부위 1곳, 부술 수 있는 팔 2곳, 패턴 2개. 행동은 BossBrain 이, 부위는 BossPart 가 맡는다.</summary>
        Boss,
    }

    /// <summary>
    /// 임시 도형 적. 수치는 EnemyData 에셋에서 읽고 체력과 상태효과는 Core 의 Combatant 가 든다(SPEC 3.4).
    /// - 돌격형은 노린 아군의 엄폐물 앞까지 다가와 일정 간격으로 때리고, 원거리형은 제자리에서 날아가는 탄환을 쏜다.
    /// - 보스는 BossBrain 에 행동을 넘긴다.
    /// - 감속은 이동 속도를 줄이고, 행동 불가 중에는 움직이지도 공격하지도 않는다.
    /// - 맞으면 번쩍이고, 상태효과가 걸려 있으면 색으로 보여준다. 체력이 다하면 사라진다. 관통탄은 적을 꿰뚫고 지나간다.
    /// </summary>
    public sealed class EnemyUnit : MonoBehaviour, IHittable, IPierceable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashBlendId = Shader.PropertyToID("_FlashBlend");
        private static readonly int RimStrengthId = Shader.PropertyToID("_RimStrength");
        private static readonly List<EnemyUnit> alive = new List<EnemyUnit>();

        /// <summary>살아 있는 적 목록. 자동 사격 아군과 스킬이 대상을 고를 때 쓴다.</summary>
        public static IReadOnlyList<EnemyUnit> Alive => alive;

        /// <summary>사격이 아닌 피해(스킬, 지속 피해)가 들어갔다. 피해 숫자 표시가 구독한다. 사격 피해는 맞은 지점을 아는 쪽이 따로 띄운다.</summary>
        public static event Action<EnemyUnit, DamageEvent> NonShotDamaged;

        [SerializeField] private EnemyKind kind;
        [SerializeField] private EnemyData data;
        [Tooltip("아군이 노리는 높이(몸 중심).")]
        [SerializeField] private float aimHeight = 0.9f;
        [Tooltip("돌격형이 멈춰서 공격하는 깊이(z). 엄폐물 바로 앞. 보스는 돌진이 멈추는 깊이.")]
        [SerializeField] private float attackLineZ = -0.6f;
        [Tooltip("아군 쪽(정면)에서 이동 방향으로 몸을 돌리는 최대 각도(도). 옆이나 뒤를 보이지 않게 막는다.")]
        [SerializeField, Range(0f, 80f)] private float maxTurnAngle = 40f;

        [Header("원거리형, 보스")]
        [SerializeField] private EnemyProjectile projectileTemplate;
        [SerializeField] private Transform muzzle;

        [Header("표시")]
        [SerializeField] private Renderer[] renderers;
        [Tooltip("머리 위 체력 막대(월드 공간 캔버스의 Filled 이미지).")]
        [SerializeField] private UnityEngine.UI.Image healthFill;
        [Tooltip("기본 번쩍임이 사라지는 시간. 타격감 프리셋이 있으면 무기별 값으로 바뀐다.")]
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color stunColor = new Color(1f, 0.95f, 0.35f);
        [SerializeField] private Color slowColor = new Color(0.35f, 0.65f, 1f);
        [SerializeField] private Color burnColor = new Color(1f, 0.45f, 0.1f);

        private AllyUnit target;
        private float nextAttackTime;
        private float flash;
        private float flashFade;
        private float flashRim;
        private Color flashColor = Color.white;
        private Color[] baseColors;
        private MaterialPropertyBlock block;
        private Func<AllyUnit> pickTarget;
        private Combatant combatant;
        private BossBrain boss;
        private Transform healthBar;
        private Quaternion healthBarRotation;

        public EnemyKind Kind => kind;
        public EnemyData Data => data;
        public Combatant Combatant => combatant;
        public bool IsDead => combatant == null || !combatant.IsAlive;
        public float AttackLineZ => attackLineZ;
        public Transform Muzzle => muzzle;
        public BossBrain Boss => boss;

        /// <summary>감속을 반영한 이동 속도 배율. 능력치(stat-system)의 MoveSpeed 최종값이다(기본 1, 감속은 최댓값형이라 가장 강한 하나만 모디파이어로 붙는다).</summary>
        public float MoveMultiplier => combatant == null ? 1f : Mathf.Clamp(FixedConvert.ToFloat(combatant.GetStat(StatSystem.StatId.MoveSpeed)), 0.1f, 1f);

        /// <summary>아군이 노리는 지점(몸 중심).</summary>
        public Vector3 AimPoint => transform.position + Vector3.up * aimHeight;

        public event Action<EnemyUnit> Died;

        /// <summary>등장시킨다. pick 은 노릴 아군을 고르는 함수다(노리던 아군이 쓰러지면 다시 부른다).</summary>
        public void Spawn(Vector3 position, Func<AllyUnit> pick)
        {
            transform.position = position;
            pickTarget = pick;
            target = pick();

            combatant = data.CreateCombatant(name);
            combatant.Owner = this;
            combatant.Damaged += OnDamaged;
            combatant.Died += _ => OnDied();

            nextAttackTime = Time.time + data.AttackInterval * UnityEngine.Random.Range(0.5f, 1.5f);
            boss = GetComponent<BossBrain>();
            gameObject.SetActive(true);
            alive.Add(this);
            RefreshHealthBar();
            if (boss != null) boss.Begin(this);
        }

        /// <summary>노릴 아군을 다시 고른다. 살아 있는 아군이 없으면 null.</summary>
        public AllyUnit PickTarget() => pickTarget?.Invoke();

        /// <summary>
        /// 이동 방향 쪽으로 몸을 기울이되 늘 아군을 향한다. 적은 아군에게 옆이나 등을 보이지 않는다(보스 포함).
        /// - 기준은 아군 쪽(월드 -Z, 모델 정면 -Z 와 같은 방향)이다. 이동의 좌우 성분만큼 돌리고 maxTurnAngle 에서 멈춘다.
        /// - 아군에게서 멀어지는 이동(+Z)은 뒷걸음질로 보고 앞뒤를 뒤집어 같은 쪽으로 기울인다. 그래서 돌아서지 않는다.
        /// 정지하면 마지막 방향을 유지한다.
        /// </summary>
        public void FaceMovement(Vector3 movement)
        {
            movement.y = 0f;
            if (movement.sqrMagnitude <= 0.00000001f) return;
            float toward = Mathf.Max(Mathf.Abs(movement.z), 0.0001f);   // 아군 쪽으로 다가가는 성분(뒷걸음질은 뒤집는다)
            float yaw = Mathf.Atan2(-movement.x, toward) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, Mathf.Clamp(yaw, -maxTurnAngle, maxTurnAngle), 0f);
        }

        /// <summary>대상 아군에게 탄환을 쏜다(SPEC 2절: 쏜 순간의 자세를 기억한다). from 을 주면 그 위치에서 쏜다(보스 팔의 포문).</summary>
        public void FireProjectile(AllyUnit ally, float damage, Transform from = null)
        {
            if (projectileTemplate == null || ally == null) return;
            EnemyProjectile projectile = Instantiate(projectileTemplate);
            projectile.Launch((from != null ? from : muzzle).position, ally, damage);
        }

        /// <summary>몸을 번쩍인다. 1이 가장 밝다.</summary>
        public void Flash(float strength) => Flash(strength, flashDuration, 0f, Color.white);

        /// <summary>
        /// 번쩍인다(SPEC 4절 피격 플래시). strength 는 처음 밝기(0~1), duration 은 사라지는 시간, rim 은 윤곽 빛 세기.
        /// 타격감 연출이 무기별 프리셋으로 부른다. 부르지 않으면 1단계와 같은 흰 번쩍임(윤곽 빛 없음)이다.
        /// </summary>
        public void Flash(float strength, float duration, float rim, Color color)
        {
            flash = Mathf.Clamp01(strength);
            flashFade = duration;
            flashRim = rim;
            flashColor = color;
        }

        private void Awake()
        {
            if (healthFill != null && healthFill.canvas != null)
            {
                healthBar = healthFill.canvas.transform;
                healthBarRotation = healthBar.rotation;
            }
            block = new MaterialPropertyBlock();
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].sharedMaterial.GetColor(BaseColorId);
        }

        // 컴포넌트를 끄는 것(전투 종료 시 멈춤)으로는 목록에서 빠지지 않는다. 죽거나 사라질 때만 빠진다.
        private void OnDestroy()
        {
            alive.Remove(this);
        }

        public void OnShot(float multiplier, Vector3 point)
        {
            // 피해와 번쩍임은 Combatant 의 Damaged 에서 처리한다. 사격만의 반응은 아직 없다(3단계 타격감).
        }

        private void OnDamaged(DamageEvent e)
        {
            Flash(1f);
            RefreshHealthBar();
            if (e.Kind != DamageSource.Shot) NonShotDamaged?.Invoke(this, e);
        }

        private void OnDied()
        {
            alive.Remove(this);
            Died?.Invoke(this);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (IsDead) return;
            combatant.Tick(CombatTime.Delta);
            if (IsDead) return; // 지속 피해로 쓰러졌다.

            if (boss != null)
            {
                boss.Tick();
                return;
            }

            if (combatant.IsControlled) return;

            if (target == null || target.IsDown)
            {
                target = PickTarget();
                if (target == null) return;
            }

            if (kind == EnemyKind.Rusher)
            {
                // 노린 아군의 엄폐물 앞까지 다가간다. 도착하기 전에는 때리지 않는다.
                Vector3 goal = new Vector3(target.transform.position.x, 0f, attackLineZ);
                Vector3 position = transform.position;
                Vector3 next = Vector3.MoveTowards(new Vector3(position.x, 0f, position.z), goal, data.MoveSpeed * MoveMultiplier * Time.deltaTime);
                transform.position = new Vector3(next.x, position.y, next.z);
                FaceMovement(transform.position - position);
                if ((next - goal).sqrMagnitude > 0.01f) return;
            }

            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + data.AttackInterval;

            if (kind == EnemyKind.Rusher)
            {
                target.TakeDamage(data.AttackDamage);
                Flash(0.6f); // 때리는 순간도 살짝 번쩍여 공격을 알린다
            }
            else
            {
                FireProjectile(target, data.AttackDamage);
            }
        }

        // 상태효과 색(기절 > 화상 > 감속)을 깔고 그 위에 피격 번쩍임을 더한다. 보스는 BossBrain 이 예고 색을 더 얹는다.
        private void LateUpdate()
        {
            // 몸이 회전해도 체력 막대는 기존 화면 방향을 유지한다.
            if (healthBar != null) healthBar.rotation = healthBarRotation;
            if (IsDead) return;
            if (flash > 0f) flash = flashFade > 0f ? Mathf.Max(0f, flash - Time.deltaTime / flashFade) : 0f;

            Color tint = Color.clear;
            float tintAmount = 0f;
            StatusContainer statuses = combatant.Statuses;
            if (statuses.IsControlled)
            {
                tint = stunColor;
                tintAmount = 0.6f;
            }
            else if (HasDot(statuses))
            {
                tint = burnColor;
                tintAmount = 0.25f + 0.25f * Mathf.PingPong(Time.time * 4f, 1f);
            }
            else if (statuses.Sum(StatKind.Slow).raw > 0)
            {
                tint = slowColor;
                tintAmount = 0.4f;
            }

            Color warning = boss != null ? boss.WarningColor : Color.clear;
            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = Color.Lerp(baseColors[i], tint, tintAmount);
                color = Color.Lerp(color, warning, warning.a);
                block.SetColor(BaseColorId, color);
                block.SetFloat(FlashAmountId, flash);
                block.SetColor(FlashColorId, flashColor);
                block.SetFloat(FlashBlendId, 1f);
                block.SetFloat(RimStrengthId, flashRim);
                renderers[i].SetPropertyBlock(block);
            }
        }

        private static bool HasDot(StatusContainer statuses)
        {
            foreach (StatusInstance s in statuses.Active) if (s.Def.Rule == StatusRule.DamageOverTime) return true;
            return false;
        }

        private void RefreshHealthBar()
        {
            if (healthFill == null || combatant == null) return;
            healthFill.fillAmount = Mathf.Clamp01(FixedConvert.ToFloat(combatant.Health) / FixedConvert.ToFloat(combatant.MaxHealth));
        }
    }
}
