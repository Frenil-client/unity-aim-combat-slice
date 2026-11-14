using System;
using AimCombat.Character;
using AimCombat.Core;
using AimCombat.Core.Combat;
using AimCombat.Data;
using AimCombat.Weapons;
using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 아군 한 명의 체력, 상태효과와 엄폐 판정. 체력과 버프는 Core 의 Combatant 가 들고 여기서는 엄폐 규칙과 표시를 맡는다.
    /// 엄폐나 재장전 자세일 때는 엄폐물이 공격을 대신 받고, 사격 자세이거나 엄폐물이 부서졌으면 본인이 맞는다.
    /// 체력이 다하면 전투 불능이 된다.
    /// </summary>
    [RequireComponent(typeof(CharacterPoseView))]
    public sealed class AllyUnit : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private CoverObject cover;
        [SerializeField] private Color downTint = new Color(0.35f, 0.35f, 0.4f, 1f);
        [Tooltip("맞았을 때 붉게 번쩍이는 시간.")]
        [SerializeField] private float hurtFlashDuration = 0.15f;
        [SerializeField] private Color healTint = new Color(0.55f, 1f, 0.6f, 1f);

        private CharacterPoseView poseView;
        private SpriteRenderer[] renderers;
        private Combatant combatant;
        private float hurtFlash;
        private float healFlash;

        /// <summary>체력과 버프. 다른 컴포넌트의 Awake 에서 먼저 불릴 수 있어 처음 부를 때 만든다.</summary>
        public Combatant Combatant
        {
            get
            {
                if (combatant == null) CreateCombatant();
                return combatant;
            }
        }

        public float Health => FixedConvert.ToFloat(Combatant.Health);
        public float MaxHealth => maxHealth;
        public bool IsDown => !Combatant.IsAlive;
        public CoverObject Cover => cover;

        /// <summary>적이 노리는 사격 위치(가슴 높이).</summary>
        public Vector3 AimPoint => transform.position + Vector3.up * 0.8f;

        /// <summary>적이 노리는 엄폐 위치. 엄폐물이 있으면 엄폐물 가운데, 없으면 웅크린 몸 높이.</summary>
        public Vector3 CoverPoint => cover != null ? cover.transform.position + Vector3.up * 0.5f : transform.position + Vector3.up * 0.4f;

        /// <summary>사격 자세면 true. 엄폐와 재장전 자세는 엄폐로 친다.</summary>
        public bool InFirePose => poseView.Pose == CharacterPose.Fire;

        /// <summary>전투 불능이 되는 순간 알린다.</summary>
        public event Action<AllyUnit> Downed;

        /// <summary>엄폐물을 뚫고 아군 본인이 피해를 받았다. 인자는 피해량. 화면 흔들림이 구독한다.</summary>
        public event Action<AllyUnit, float> Wounded;

        public void SetCover(CoverObject value) => cover = value;

        private void Awake()
        {
            poseView = GetComponent<CharacterPoseView>();
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            if (combatant == null) CreateCombatant();
        }

        // 공격력은 무기에서 읽는다. 아군은 방어력이 없다(적 공격은 공격 수치를 그대로 준다).
        private void CreateCombatant()
        {
            var weapon = GetComponent<CharacterWeapon>();
            Fixed attack = FixedConvert.ToFixed(weapon != null ? weapon.Attack : 0f);
            combatant = new Combatant(name, FixedConvert.ToFixed(maxHealth), attack, Fixed.Zero) { Owner = this };
            combatant.Died += _ =>
            {
                if (poseView != null) poseView.SetPose(CharacterPose.Cover);
                Downed?.Invoke(this);
            };
            combatant.Healed += (_, __) => healFlash = 1f;
        }

        /// <summary>근접 공격. 엄폐 중이면 엄폐물이 먼저 받는다.</summary>
        public void TakeDamage(float damage)
        {
            if (IsDown) return;

            bool sheltered = !InFirePose && cover != null && !cover.IsBroken;
            if (sheltered)
            {
                damage = cover.Absorb(damage);
                if (damage <= 0f) return;
            }

            Hurt(damage);
        }

        /// <summary>
        /// 적 탄환이 도착했다. aimedAtFirePose 는 쏜 순간 이 아군이 사격 자세였는지다(SPEC 2절).
        /// 그때와 지금 자세가 다르면 아군은 맞지 않는다. 엄폐 위치로 온 탄은 엄폐물이 살아 있으면 엄폐물이 받는다.
        /// </summary>
        public void TakeProjectile(float damage, bool aimedAtFirePose)
        {
            if (IsDown) return;

            if (aimedAtFirePose)
            {
                if (InFirePose) Hurt(damage);
                return;
            }

            if (cover != null && !cover.IsBroken)
            {
                damage = cover.Absorb(damage);
                if (damage <= 0f) return;
            }

            if (!InFirePose) Hurt(damage);
        }

        private void Hurt(float damage)
        {
            hurtFlash = 1f;
            Wounded?.Invoke(this, damage);
            Combatant.ApplyDamage(FixedConvert.ToFixed(damage), DamageSource.Enemy, null);
        }

        private void Update()
        {
            Combatant.Tick(CombatTime.Delta);
        }

        private void LateUpdate()
        {
            Color color = Color.white;
            if (IsDown)
            {
                color = downTint;
            }
            else
            {
                if (healFlash > 0f)
                {
                    healFlash = Mathf.Max(0f, healFlash - Time.deltaTime / 0.4f);
                    color = Color.Lerp(color, healTint, healFlash);
                }
                if (hurtFlash > 0f)
                {
                    hurtFlash = Mathf.Max(0f, hurtFlash - Time.deltaTime / hurtFlashDuration);
                    color = Color.Lerp(color, new Color(1f, 0.45f, 0.45f), hurtFlash);
                }
            }

            foreach (SpriteRenderer r in renderers) r.color = color;
        }
    }
}
