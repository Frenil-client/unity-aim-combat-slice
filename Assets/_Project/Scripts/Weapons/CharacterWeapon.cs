using System;
using AimCombat.Character;
using UnityEngine;

namespace AimCombat.Weapons
{
    public enum WeaponKind
    {
        /// <summary>누르는 동안 일정 간격 발사. 연사할수록 탄 퍼짐 증가.</summary>
        Auto,
        /// <summary>누르고 있으면 충전, 떼면 발사. 충전량에 따라 피해 배수, 관통.</summary>
        Charge,
        /// <summary>한 번에 여러 발. 원뿔로 퍼져서 멀수록 넓게 흩어진다.</summary>
        Shotgun,
    }

    /// <summary>한 발이 지나간 경로. 궤적 표시가 구독한다.</summary>
    public readonly struct ShotResult
    {
        public readonly Vector3 From;
        public readonly Vector3 To;
        /// <summary>피해 배수. 차지형은 충전량에 따라 커진다. 피해 계산은 2단계에서 붙는다.</summary>
        public readonly float Multiplier;
        public readonly WeaponKind Kind;

        public ShotResult(Vector3 from, Vector3 to, float multiplier, WeaponKind kind)
        {
            From = from;
            To = to;
            Multiplier = multiplier;
            Kind = kind;
        }
    }

    /// <summary>
    /// 아군 한 명의 무기. 방아쇠 입력과 조준 광선을 받아 무기 종류별 사격 모델대로 히트스캔을 쏜다.
    /// 탄창과 재장전 시간은 여기서 정하고, 재장전 진행은 조작하는 쪽이 맡는다.
    /// 판정은 조준 광선(카메라에서 조준점 방향)으로 하고, 보이는 궤적은 총구에서 맞은 곳까지다.
    /// </summary>
    [RequireComponent(typeof(CharacterAimRig))]
    public sealed class CharacterWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponKind kind = WeaponKind.Auto;

        [Header("공통")]
        [Tooltip("공격력. 피해는 (공격력 - 대상 방어력) x 무기 계수에서 시작한다(SPEC 3.1).")]
        [SerializeField] private float attack = 100f;
        [Tooltip("무기 계수. 한 발(산탄형은 탄알 한 개)이 공격력의 몇 배를 주는지.")]
        [SerializeField] private float weaponCoefficient = 0.1f;
        [SerializeField] private int magazineSize = 30;
        [Tooltip("빈 탄창을 다 채우는 데 걸리는 시간. 단계로 나누면 한 단계에 이 시간 / 단계 수가 걸린다.")]
        [SerializeField] private float reloadDuration = 1.2f;
        [Tooltip("재장전 단계 수. 1이면 한 번에 다 채운다. 3이면 탄창의 1/3(올림)씩 세 번에 나눠 넣고, 도중에 쏘면 끝난 단계만큼 남는다(SPEC 2절 산탄형).")]
        [Min(1)]
        [SerializeField] private int reloadStages = 1;
        [Tooltip("발사 간격(초). 차지형은 발사 뒤 다음 충전까지의 간격.")]
        [SerializeField] private float fireInterval = 0.1f;
        [Tooltip("한 발에 주는 반동 세기. 차지형은 충전량에 따라 이 값의 1배에서 2배.")]
        [SerializeField] private float kickStrength = 1f;
        [Tooltip("손을 떼거나 쏜 뒤에도 사격 자세를 유지하는 시간. 바로 엄폐로 들어가면 쏜 모습이 안 보인다.")]
        [SerializeField] private float postShotHold = 0f;
        [Tooltip("아무것도 맞지 않았을 때 판정 광선이 뻗는 거리.")]
        [SerializeField] private float range = 80f;

        [Header("연사형: 탄 퍼짐")]
        [Tooltip("한 발 쏠 때마다 늘어나는 퍼짐 각도(도).")]
        [SerializeField] private float spreadPerShot = 0.35f;
        [SerializeField] private float maxSpread = 4f;
        [Tooltip("쏘지 않을 때 1초에 줄어드는 퍼짐 각도.")]
        [SerializeField] private float spreadRecovery = 10f;

        [Header("차지형: 충전과 관통")]
        [Tooltip("가득 충전하는 데 걸리는 시간(초).")]
        [SerializeField] private float chargeDuration = 1f;
        [Tooltip("가득 충전했을 때 더해지는 차지 배율. 차지 배율 = 1 + 충전량 x 이 값(SPEC 3.1). 충전 없이 떼면 1배.")]
        [SerializeField] private float chargeBonus = 1.5f;
        [Tooltip("한 발이 꿰뚫고 맞힐 수 있는 대상 수.")]
        [SerializeField] private int pierceCount = 3;

        [Header("산탄형: 다발")]
        [SerializeField] private int pelletCount = 8;
        [Tooltip("탄알이 퍼지는 원뿔의 반각(도). 각도가 같으니 멀수록 넓게 흩어진다.")]
        [SerializeField] private float pelletSpread = 5f;

        private CharacterAimRig rig;
        private float nextFireTime;
        private float holdUntil;
        private float spread;
        private float charge;
        private bool charging;
        private readonly RaycastHit[] pierceHits = new RaycastHit[16];

        /// <summary>한 발이 지나갈 때마다 알린다. 산탄형은 한 번 쏠 때 탄알 수만큼 알린다.</summary>
        public event Action<ShotResult> Traced;

        /// <summary>한 발이 무언가를 맞힐 때마다 알린다. 관통탄은 맞힌 대상마다 알린다. 인자는 충돌체, 지점, 충전량(0에서 1, 차지형이 아니면 0).</summary>
        public event Action<Collider, Vector3, float> Hit;

        public WeaponKind Kind => kind;
        public int Ammo { get; private set; }
        public int MagazineSize => magazineSize;
        public float Attack => attack;
        public float WeaponCoefficient => weaponCoefficient;
        public int ReloadStages => Mathf.Max(1, reloadStages);

        /// <summary>재장전 한 단계에 걸리는 시간. 단계가 1이면 전체 재장전 시간이다.</summary>
        public float StageDuration => reloadDuration / ReloadStages;

        /// <summary>적정 사거리(SPEC 2절). 아군 줄에서 적이 나타나는 가장 먼 선까지를 1로 본 비율 구간.</summary>
        public Vector2 EffectiveRange => EffectiveRangeOf(kind);

        /// <summary>명중 한 번(산탄형은 탄알 하나, 관통탄은 대상 하나)에 차는 궁극기 게이지(SPEC 3.2).</summary>
        public float GaugePerHit => GaugePerHitOf(kind);

        public static Vector2 EffectiveRangeOf(WeaponKind weaponKind)
        {
            switch (weaponKind)
            {
                case WeaponKind.Auto: return new Vector2(0.25f, 0.45f);
                case WeaponKind.Charge: return new Vector2(0.45f, 1f);
                default: return new Vector2(0f, 0.25f);
            }
        }

        // 니케의 무기별 1발당 버스트 게이지를 시작값으로 쓴다(NIKKE_COMBAT_ANALYSIS.md 5절, 게이지 최대 10000).
        public static float GaugePerHitOf(WeaponKind weaponKind)
        {
            switch (weaponKind)
            {
                case WeaponKind.Auto: return 20f;
                case WeaponKind.Charge: return 280f;
                default: return 45f;
            }
        }

        /// <summary>충전량에 따른 차지 배율. 반동과 궤적, 명중 반응 크기에도 쓴다.</summary>
        public float ChargeMultiplierAt(float charge01) => 1f + Mathf.Clamp01(charge01) * chargeBonus;

        /// <summary>충전 비율(0에서 1). 충전 중이 아니면 0.</summary>
        public float Charge01 => charging ? charge : 0f;
        public bool IsCharging => charging;

        /// <summary>지금 퍼짐 각도(도). 연사형은 쌓인 값, 산탄형은 원뿔 반각, 차지형은 0.</summary>
        public float SpreadDegrees => kind == WeaponKind.Auto ? spread : kind == WeaponKind.Shotgun ? pelletSpread : 0f;

        private void Awake()
        {
            rig = GetComponent<CharacterAimRig>();
            Ammo = magazineSize;
        }

        public void Refill()
        {
            Ammo = magazineSize;
        }

        /// <summary>재장전 한 단계만큼(탄창 / 단계 수, 올림) 넣는다. 탄창을 넘치지 않는다.</summary>
        public void LoadStage()
        {
            int perStage = (magazineSize + ReloadStages - 1) / ReloadStages;
            Ammo = Mathf.Min(Ammo + perStage, magazineSize);
        }

        /// <summary>충전을 쏘지 않고 버린다. 조작 대상이 바뀔 때 쓴다.</summary>
        public void CancelCharge()
        {
            charging = false;
            charge = 0f;
        }

        /// <summary>
        /// 한 프레임 진행한다. held 는 방아쇠를 누르고 있는지, aim 은 카메라에서 조준점으로 가는 광선이다.
        /// 사격 자세를 보여야 하면(누르는 중, 충전 중, 쏜 직후) true 를 돌려준다.
        /// </summary>
        public bool Tick(bool held, Ray aim)
        {
            float now = Time.time;
            if (Ammo <= 0) held = false;

            switch (kind)
            {
                case WeaponKind.Auto:
                    if (held && now >= nextFireTime)
                    {
                        nextFireTime = now + fireInterval;
                        FireSingle(Scatter(aim, spread), 0f, 1);
                        spread = Mathf.Min(spread + spreadPerShot, maxSpread);
                        Consume(kickStrength);
                    }
                    else if (!held)
                    {
                        spread = Mathf.Max(0f, spread - spreadRecovery * Time.deltaTime);
                    }
                    break;

                case WeaponKind.Charge:
                    if (held && now >= nextFireTime)
                    {
                        charging = true;
                        charge = Mathf.Min(1f, charge + (chargeDuration > 0f ? Time.deltaTime / chargeDuration : 1f));
                    }
                    else if (charging && !held)
                    {
                        FireSingle(aim, charge, pierceCount);
                        Consume(kickStrength * (1f + charge));
                        CancelCharge();
                        nextFireTime = now + fireInterval;
                    }
                    break;

                case WeaponKind.Shotgun:
                    if (held && now >= nextFireTime)
                    {
                        nextFireTime = now + fireInterval;
                        for (int i = 0; i < pelletCount; i++) FireSingle(Scatter(aim, pelletSpread), 0f, 1);
                        Consume(kickStrength);
                    }
                    break;
            }

            return held || charging || now < holdUntil;
        }

        private void Consume(float kick)
        {
            Ammo--;
            rig.Kick(kick);
            holdUntil = Time.time + postShotHold;
        }

        // 광선 방향을 반각 degrees 의 원뿔 안에서 고르게 흩뜨린다.
        private static Ray Scatter(Ray ray, float degrees)
        {
            if (degrees <= 0f) return ray;
            Vector2 offset = UnityEngine.Random.insideUnitCircle * Mathf.Tan(degrees * Mathf.Deg2Rad);
            Vector3 direction = Quaternion.LookRotation(ray.direction) * new Vector3(offset.x, offset.y, 1f);
            return new Ray(ray.origin, direction.normalized);
        }

        // 한 발. pierce 가 1보다 크면 맞힌 대상을 꿰뚫고 다음 대상까지 간다. 대상이 아닌 벽이나 바닥에서는 멈춘다.
        // charge01 은 충전량이다. 명중 알림에는 그대로, 궤적에는 차지 배율로 넘긴다.
        private void FireSingle(Ray ray, float charge01, int pierce)
        {
            Vector3 from = rig.Muzzle.position;
            float multiplier = ChargeMultiplierAt(charge01);

            if (pierce <= 1)
            {
                Vector3 to = ray.GetPoint(range);
                if (Physics.Raycast(ray, out RaycastHit hit, range))
                {
                    to = hit.point;
                    Hit?.Invoke(hit.collider, hit.point, charge01);
                }
                Traced?.Invoke(new ShotResult(from, to, multiplier, kind));
                return;
            }

            int count = Physics.RaycastNonAlloc(ray, pierceHits, range);
            Array.Sort(pierceHits, 0, count, HitDistanceComparer.Instance);

            Vector3 end = ray.GetPoint(range);
            int pierced = 0;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = pierceHits[i];
                Hit?.Invoke(hit.collider, hit.point, charge01);
                bool pierceable = hit.collider.GetComponentInParent<IPierceable>() != null;
                if (!pierceable || ++pierced >= pierce)
                {
                    end = hit.point;
                    break;
                }
            }

            // 꿰뚫은 대상이 여럿이어도 궤적은 마지막 지점까지 한 줄이다.
            Traced?.Invoke(new ShotResult(from, end, multiplier, kind));
        }

        private sealed class HitDistanceComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly HitDistanceComparer Instance = new HitDistanceComparer();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }
    }

    /// <summary>관통탄이 멈추지 않고 지나가는 대상. 이것이 없는 충돌체(벽, 바닥)에서는 관통탄도 멈춘다.</summary>
    public interface IPierceable
    {
    }
}
