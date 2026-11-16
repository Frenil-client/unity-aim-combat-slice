using System;
using UnityEngine;

namespace AimCombat.Battle
{
    public enum BossPartStage
    {
        /// <summary>체력 50% 초과.</summary>
        Intact,
        /// <summary>체력 50% 이하.</summary>
        Damaged,
        /// <summary>체력 0. 판정이 사라진다.</summary>
        Destroyed,
    }

    /// <summary>
    /// 보스의 부위 하나(SPEC 3.4 부위 파괴와 저지). 이 컴포넌트가 붙은 충돌체에 맞은 사격 피해를 따로 센다.
    /// 보스 체력은 부모의 EnemyUnit 이 깎고, 여기서는 부위 체력과 저지 게이지만 깎는다.
    /// - 부술 수 있는 부위(팔): 체력이 있고 온전, 손상, 파괴 단계마다 모습을 바꾼다. 부서지면 충돌체를 끈다.
    /// - 저지 지점(본체): 체력이 없고, 저지 표적이 떠 있는 동안에만 충돌체가 켜진다.
    /// 저지 표적은 어느 쪽에든 뜰 수 있다. 표적이 뜨면 맞은 피해만큼 저지 게이지가 깎인다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class BossPart : MonoBehaviour
    {
        /// <summary>이 비율 이하로 체력이 내려가면 손상 단계다.</summary>
        public const float DamagedRatio = 0.5f;

        [SerializeField] private string displayName = "";
        [Tooltip("켜면 체력이 있고 부술 수 있다(팔). 끄면 저지 표적이 뜰 때만 맞는 본체의 저지 지점이다.")]
        [SerializeField] private bool destructible = true;

        [Header("단계별 모습(부술 수 있는 부위)")]
        [SerializeField] private GameObject intactVisual;
        [SerializeField] private GameObject damagedVisual;
        [SerializeField] private GameObject destroyedVisual;

        [Header("위치")]
        [Tooltip("탄막을 쏘는 포문. 팔에만 둔다.")]
        [SerializeField] private Transform muzzle;
        [Tooltip("이름, 체력 막대, 저지 표적을 띄우는 지점. 비우면 이 오브젝트의 위치.")]
        [SerializeField] private Transform markAnchor;
        [Tooltip("저지 표적 원의 반지름(월드 단위).")]
        [SerializeField] private float markRadius = 0.6f;

        private Collider hitCollider;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public bool Destructible => destructible;
        public Transform Muzzle => muzzle;
        public Vector3 MarkPoint => markAnchor != null ? markAnchor.position : transform.position;
        public float MarkRadius => markRadius;

        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public BossPartStage Stage { get; private set; }
        public bool IsDestroyed => Stage == BossPartStage.Destroyed;
        public float HealthRatio => MaxHealth > 0f ? Mathf.Clamp01(Health / MaxHealth) : 0f;

        /// <summary>저지 표적이 떠 있다.</summary>
        public bool Marked { get; private set; }
        public float MarkMaxHealth { get; private set; }
        public float MarkHealth { get; private set; }
        public float MarkRatio => MarkMaxHealth > 0f ? Mathf.Clamp01(MarkHealth / MarkMaxHealth) : 0f;

        /// <summary>단계가 바뀌었다(손상, 파괴).</summary>
        public event Action<BossPart, BossPartStage> StageChanged;

        /// <summary>떠 있던 저지 표적이 다 깎였다. 표적이 뜬 팔이 부서져도 알린다.</summary>
        public event Action<BossPart> MarkCleared;

        /// <summary>보스가 등장할 때 한 번 부른다. 저지 지점은 체력 없이 꺼진 채로 시작한다.</summary>
        public void Setup(float maxHealth)
        {
            hitCollider = GetComponent<Collider>();
            MaxHealth = destructible ? Mathf.Max(1f, maxHealth) : 0f;
            Health = MaxHealth;
            Marked = false;
            SetStage(BossPartStage.Intact, false);
            hitCollider.enabled = destructible;
        }

        /// <summary>저지 표적을 띄운다. 부서진 부위에는 띄울 수 없다.</summary>
        public void Mark(float health)
        {
            if (IsDestroyed || health <= 0f) return;
            Marked = true;
            MarkMaxHealth = health;
            MarkHealth = health;
            hitCollider.enabled = true;
        }

        /// <summary>저지 표적을 거둔다(예고가 끝났다). 저지 지점은 다시 맞지 않게 된다.</summary>
        public void Unmark()
        {
            Marked = false;
            if (hitCollider != null) hitCollider.enabled = destructible && !IsDestroyed;
        }

        /// <summary>이 부위에 맞은 사격 피해. 부위 체력과 저지 게이지를 같은 양만큼 깎는다.</summary>
        public void TakeShot(float damage)
        {
            if (damage <= 0f || IsDestroyed) return;

            bool markHit = Marked;
            if (markHit) MarkHealth = Mathf.Max(0f, MarkHealth - damage);

            if (destructible)
            {
                Health = Mathf.Max(0f, Health - damage);
                BossPartStage next = Health <= 0f ? BossPartStage.Destroyed
                    : Health <= MaxHealth * DamagedRatio ? BossPartStage.Damaged
                    : BossPartStage.Intact;
                if (next != Stage) SetStage(next, true);
            }

            // 표적이 다 깎였거나 표적이 뜬 채로 부서졌다. 부서짐을 먼저 알린 뒤에 알린다.
            if (markHit && Marked && (MarkHealth <= 0f || IsDestroyed))
            {
                MarkHealth = 0f;
                Unmark();
                MarkCleared?.Invoke(this);
            }
        }

        private void SetStage(BossPartStage stage, bool notify)
        {
            Stage = stage;
            if (intactVisual != null) intactVisual.SetActive(stage == BossPartStage.Intact);
            if (damagedVisual != null) damagedVisual.SetActive(stage == BossPartStage.Damaged);
            if (destroyedVisual != null) destroyedVisual.SetActive(stage == BossPartStage.Destroyed);
            if (stage == BossPartStage.Destroyed && hitCollider != null) hitCollider.enabled = false;
            if (notify) StageChanged?.Invoke(this, stage);
        }
    }
}
