using System;
using System.Collections.Generic;
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
    }

    /// <summary>
    /// 임시 도형 적. 돌격형은 노린 아군의 엄폐물 앞까지 다가와 일정 간격으로 때리고,
    /// 원거리형은 제자리에서 날아가는 탄환을 쏜다. 아군 사격에 맞으면 번쩍이고 체력이 다하면 사라진다.
    /// 관통탄은 적을 꿰뚫고 지나간다.
    /// </summary>
    public sealed class EnemyUnit : MonoBehaviour, IHittable, IPierceable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly List<EnemyUnit> alive = new List<EnemyUnit>();

        /// <summary>살아 있는 적 목록. 자동 사격 아군이 대상을 고를 때 쓴다.</summary>
        public static IReadOnlyList<EnemyUnit> Alive => alive;

        [SerializeField] private EnemyKind kind;
        [SerializeField] private float maxHealth = 60f;
        [Tooltip("방어력. 아군 공격력에서 뺀다(SPEC 3.1).")]
        [SerializeField] private float defense;

        [Header("공격")]
        [SerializeField] private float attackDamage = 20f;
        [SerializeField] private float attackInterval = 1.2f;

        [Header("돌격형")]
        [SerializeField] private float moveSpeed = 2.2f;
        [Tooltip("멈춰서 공격하는 깊이(z). 엄폐물 바로 앞.")]
        [SerializeField] private float attackLineZ = -0.6f;

        [Header("원거리형")]
        [SerializeField] private EnemyProjectile projectileTemplate;
        [SerializeField] private Transform muzzle;

        [Header("표시")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Transform healthFill;
        [SerializeField] private float flashDuration = 0.1f;

        private AllyUnit target;
        private float nextAttackTime;
        private float health;
        private float flash;
        private Color[] baseColors;
        private MaterialPropertyBlock block;
        private Func<AllyUnit> pickTarget;

        public EnemyKind Kind => kind;
        public bool IsDead => health <= 0f;
        public float Defense => defense;

        /// <summary>아군이 노리는 지점(몸 중심).</summary>
        public Vector3 AimPoint => transform.position + Vector3.up * (kind == EnemyKind.Rusher ? 0.45f : 0.9f);

        public event Action<EnemyUnit> Died;

        /// <summary>등장시킨다. pick 은 노릴 아군을 고르는 함수다(노리던 아군이 쓰러지면 다시 부른다).</summary>
        public void Spawn(Vector3 position, Func<AllyUnit> pick)
        {
            transform.position = position;
            pickTarget = pick;
            target = pick();
            health = maxHealth;
            nextAttackTime = Time.time + attackInterval * UnityEngine.Random.Range(0.5f, 1.5f);
            gameObject.SetActive(true);
            alive.Add(this);
            RefreshHealthBar();
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].sharedMaterial.GetColor(BaseColorId);
        }

        // 컴포넌트를 끄는 것(전투 종료 시 멈춤)으로는 목록에서 빠지지 않는다. 죽거나 사라질 때만 빠진다.
        private void OnDestroy()
        {
            alive.Remove(this);
        }

        public void TakeHit(float damage, float multiplier, Vector3 point)
        {
            if (IsDead) return;
            health -= damage;
            flash = 1f;
            RefreshHealthBar();
            if (IsDead)
            {
                alive.Remove(this);
                Died?.Invoke(this);
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (target == null || target.IsDown)
            {
                target = pickTarget?.Invoke();
                if (target == null) return;
            }

            if (kind == EnemyKind.Rusher)
            {
                // 노린 아군의 엄폐물 앞까지 다가간다. 도착하기 전에는 때리지 않는다.
                Vector3 goal = new Vector3(target.transform.position.x, 0f, attackLineZ);
                Vector3 position = transform.position;
                Vector3 next = Vector3.MoveTowards(new Vector3(position.x, 0f, position.z), goal, moveSpeed * Time.deltaTime);
                transform.position = new Vector3(next.x, position.y, next.z);
                if ((next - goal).sqrMagnitude > 0.01f) return;
            }

            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + attackInterval;

            if (kind == EnemyKind.Rusher)
            {
                target.TakeDamage(attackDamage);
                flash = 0.6f; // 때리는 순간도 살짝 번쩍여 공격을 알린다
            }
            else if (projectileTemplate != null)
            {
                EnemyProjectile projectile = Instantiate(projectileTemplate);
                projectile.Launch(muzzle.position, target, attackDamage);
            }
        }

        private void LateUpdate()
        {
            if (flash <= 0f) return;
            flash = flashDuration > 0f ? Mathf.Max(0f, flash - Time.deltaTime / flashDuration) : 0f;
            for (int i = 0; i < renderers.Length; i++)
            {
                block.SetColor(BaseColorId, Color.Lerp(baseColors[i], Color.white, flash));
                renderers[i].SetPropertyBlock(block);
            }
        }

        private void RefreshHealthBar()
        {
            if (healthFill == null) return;
            float ratio = Mathf.Clamp01(health / maxHealth);
            Vector3 scale = healthFill.localScale;
            scale.x = ratio;
            healthFill.localScale = scale;
            healthFill.localPosition = new Vector3((ratio - 1f) * 0.5f, 0f, 0f);
        }
    }
}
