using System;
using AimCombat.Battle;
using AimCombat.Character;
using AimCombat.Core;
using AimCombat.Core.Combat;
using AimCombat.Core.Random;
using AimCombat.Input;
using AimCombat.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Squad
{
    /// <summary>
    /// 아군 분대 조작. 사격장과 전투 씬이 같이 쓴다.
    /// - 고른 한 명은 입력 계층(CombatInput)의 조준점과 방아쇠를 따른다. 무기는 자기 방식대로 쏜다(연사, 차지, 산탄).
    /// - 나머지는 autoFire 가 켜져 있으면 살아 있는 적을 골라 스스로 쏜다(SPEC 2.1).
    ///   무기와 상관없이 탄이 0이 될 때까지 쏘고, 재장전하면 탄창을 다 채운 뒤 다시 쏜다.
    /// - 사격을 멈추면 재장전하고, 재장전이 끝나야 엄폐로 돌아간다. 재장전 도중 다시 쏘면 재장전은 취소된다.
    ///   단계로 나눠 장전하는 무기(산탄형)는 끝난 단계만큼 남고 다음 단계의 시간만 처음부터 센다.
    /// - 명중하면 Core 의 DamageFormula 로 피해를 계산하고(SPEC 3.1) 팀 공용 궁극기 게이지를 채운다(SPEC 3.2).
    /// - 궁극기는 Q 키나 TryUltimate(HUD 버튼)로 순번대로 발동한다. 강화 상태에서는 자동 사격 아군도
    ///   조작 중인 아군의 조준점을 함께 쏜다.
    /// - 대상은 숫자 키나 Select(초상 버튼)로 바꾸고, 카메라는 고른 아군을 가로 중심으로 따라간다.
    /// - 전투 불능 아군은 쏘지도 고를 수도 없다.
    /// </summary>
    public sealed class SquadController : MonoBehaviour
    {
        [Serializable]
        private sealed class Member
        {
            public CharacterAimRig rig;
            public CharacterPoseView poseView;
            public CharacterWeapon weapon;
            [Tooltip("전투 씬에서만 쓴다. 비어 있으면 쓰러지지 않는다(사격장).")]
            public AllyUnit unit;

            [NonSerialized] public bool reloading;
            [NonSerialized] public float reloadEndTime;
            [NonSerialized] public EnemyUnit autoTarget;
            [NonSerialized] public Vector3 autoAimOffset;
            [NonSerialized] public float autoRetargetTime;

            public bool IsDown => unit != null && unit.IsDown;
        }

        private sealed class Tracer
        {
            public LineRenderer line;
            public float endTime;
            public float width;
        }

        [Header("참조")]
        [SerializeField] private Camera viewCamera;
        [SerializeField] private CombatInput input;
        [SerializeField] private Member[] members;
        [SerializeField] private int selectedIndex;
        [SerializeField] private RectTransform crosshair;
        [SerializeField] private RectTransform chargeBar;
        [SerializeField] private RectTransform chargeFill;
        [Tooltip("궤적 원본. 실행하면 같은 설정으로 여러 개를 만들어 산탄의 탄알마다 하나씩 쓴다.")]
        [SerializeField] private LineRenderer tracer;
        [Tooltip("사격장용 탄약 표시. 전투 씬은 HUD 가 따로 그린다.")]
        [SerializeField] private Text ammoText;

        [Header("자동 사격")]
        [Tooltip("고르지 않은 아군이 살아 있는 적을 스스로 쏜다.")]
        [SerializeField] private bool autoFire;
        [Tooltip("자동 사격 조준이 적 중심에서 벗어나는 최대 거리(유닛). 클수록 잘 빗나간다.")]
        [SerializeField] private float autoAimError = 0.5f;
        [Tooltip("켜면 고른 아군도 자동으로 쏜다(전원 자동 전투).")]
        [SerializeField] private bool autoControlSelected;
        [Tooltip("자동 사격 아군이 대상을 다시 고르는 간격(초).")]
        [SerializeField] private float autoRetargetInterval = 1.5f;

        [Header("피해")]
        [Tooltip("적이 나타나는 가장 먼 선의 깊이(z). 아군 줄에서 여기까지를 적정 사거리 100%로 본다(SPEC 2절).")]
        [SerializeField] private float enemyFarZ = 28f;
        [Tooltip("치명 판정 난수의 시드. 같은 시드면 같은 순서로 치명이 나온다.")]
        [SerializeField] private int critSeed = 20261114;

        [Header("궁극기")]
        [SerializeField] private float ultimateGaugeMax = 10000f;
        [Tooltip("앞 순번을 발동한 뒤 다음 순번을 발동해야 하는 제한 시간(초).")]
        [SerializeField] private float ultimateStageWindow = 10f;
        [Tooltip("마지막 순번까지 발동했을 때 강화 상태가 이어지는 시간(초).")]
        [SerializeField] private float enhancedDuration = 10f;

        [Header("궤적")]
        [SerializeField] private float tracerDuration = 0.05f;
        [Tooltip("차지형 궤적이 보이는 시간. 굵고 오래 남겨 한 발의 무게를 보여준다.")]
        [SerializeField] private float chargeTracerDuration = 0.15f;
        [SerializeField] private int tracerPoolSize = 32;

        [Header("조준점")]
        [Tooltip("조준점 팔이 중심에서 떨어진 기본 거리(픽셀, 1080 기준).")]
        [SerializeField] private float crosshairGap = 22f;
        [Tooltip("퍼짐 1도당 조준점 팔이 벌어지는 거리.")]
        [SerializeField] private float crosshairPixelsPerDegree = 14f;

        [Header("카메라")]
        [Tooltip("고른 아군으로 카메라가 옮겨 가는 빠르기. 클수록 빨리 도착한다.")]
        [SerializeField] private float cameraFollowSharpness = 6f;

        private Tracer[] tracers;
        private int nextTracer;
        private float cameraOffsetX;
        private RectTransform[] crosshairArms;
        private static readonly Vector2[] ArmDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        private float rangeOriginZ;
        private DeterministicRandom critRandom;
        private UltimateChain ultimate;

        /// <summary>팀 공용 궁극기 게이지와 연계 상태(SPEC 3.2). 순번은 분대원 순서다.</summary>
        public UltimateChain Ultimate => ultimate;

        public int Count => members.Length;
        public int SelectedIndex => selectedIndex;
        public string NameOf(int index) => members[index].rig.name;
        public CharacterWeapon WeaponOf(int index) => members[index].weapon;
        public AllyUnit UnitOf(int index) => members[index].unit;
        public bool IsReloading(int index) => members[index].reloading;

        /// <summary>고른 아군까지 자동으로 쏘게 한다(전원 자동 전투).</summary>
        public bool AutoControlSelected
        {
            get => autoControlSelected;
            set => autoControlSelected = value;
        }

        /// <summary>분대원 전원이 전투 불능이면 true. 쓰러지지 않는 사격장에서는 늘 false.</summary>
        public bool AllDown
        {
            get
            {
                foreach (Member m in members) if (!m.IsDown) return false;
                return true;
            }
        }

        private void Awake()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, members.Length - 1);

            // 처음 배치에서 카메라와 고른 아군 사이의 가로 차이를 유지한 채 따라간다.
            cameraOffsetX = viewCamera.transform.position.x - members[selectedIndex].rig.transform.position.x;

            // 적정 사거리는 아군 줄의 깊이에서 잰다.
            rangeOriginZ = 0f;
            foreach (Member member in members) rangeOriginZ += member.rig.transform.position.z;
            rangeOriginZ /= members.Length;

            critRandom = new DeterministicRandom(critSeed);
            ultimate = new UltimateChain(members.Length, ToFixed(ultimateGaugeMax), ToFixed(ultimateStageWindow), ToFixed(enhancedDuration));

            // 궤적 원본을 Instantiate 로 복제하면 같은 오브젝트에 붙은 컴포넌트까지 복제될 수 있다.
            // 빈 오브젝트에 LineRenderer 만 새로 만들고 설정을 옮긴다.
            tracers = new Tracer[Mathf.Max(1, tracerPoolSize)];
            for (int i = 0; i < tracers.Length; i++)
            {
                LineRenderer line = i == 0 ? tracer : CreateTracerLike(tracer, i);
                line.enabled = false;
                tracers[i] = new Tracer { line = line, width = tracer.startWidth };
            }

            foreach (Member member in members)
            {
                CharacterWeapon weapon = member.weapon;
                weapon.Traced += ShowTracer;
                weapon.Hit += (collider, point, charge01) => OnHit(member, collider, point, charge01);
                if (member.unit != null) member.unit.Downed += OnAllyDowned;
            }

            crosshairArms = new RectTransform[4];
            string[] armNames = { "Top", "Bottom", "Left", "Right" };
            for (int i = 0; i < 4; i++) crosshairArms[i] = (RectTransform)crosshair.Find(armNames[i]);

            RefreshAmmoText();
        }

        /// <summary>조작할 아군을 고른다. 전투 불능이면 무시한다.</summary>
        public void Select(int index)
        {
            if (index < 0 || index >= members.Length || index == selectedIndex || members[index].IsDown) return;
            // 충전 중에 대상을 바꾸면 쏘지 않고 버린다. 떼는 순간 발사로 처리되지 않게 한다.
            members[selectedIndex].weapon.CancelCharge();
            selectedIndex = index;
            RefreshAmmoText();
        }

        private void Update()
        {
            float now = Time.time;
            foreach (Tracer t in tracers)
            {
                if (t.line.enabled && now >= t.endTime) t.line.enabled = false;
            }

            if (input.SelectRequest >= 0) Select(input.SelectRequest);

            ultimate.Tick(ToFixed(Time.deltaTime));
            if (input.UltimatePressedThisFrame) TryUltimate();

            Vector2 screenPoint = input.AimScreenPosition;
            crosshair.position = screenPoint;
            Ray ray = viewCamera.ScreenPointToRay(screenPoint);

            Member selected = members[selectedIndex];
            if (!selected.IsDown && !autoControlSelected) AimRig(selected.rig, ray);

            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (member.IsDown) continue;

                if (i == selectedIndex && !autoControlSelected)
                {
                    UpdateMember(member, input.FireHeld, ray);
                }
                else if (autoFire || i == selectedIndex)
                {
                    AutoFire(member, ray);
                }
                else
                {
                    UpdateMember(member, false, ray);
                }
            }

            UpdateCrosshair();
        }

        private void LateUpdate()
        {
            Transform cameraTransform = viewCamera.transform;
            Vector3 position = cameraTransform.position;
            float targetX = members[selectedIndex].rig.transform.position.x + cameraOffsetX;
            position.x = Mathf.Lerp(targetX, position.x, Mathf.Exp(-cameraFollowSharpness * Time.deltaTime));
            cameraTransform.position = position;
        }

        // 총의 각도는 화면에서 본 방향을 따른다. 캐릭터가 놓인 평면과 광선이 만나는 점을 조준시킨다.
        private static void AimRig(CharacterAimRig rig, Ray ray)
        {
            Plane plane = new Plane(rig.transform.forward, rig.transform.position);
            if (plane.Raycast(ray, out float distance)) rig.AimAt(ray.GetPoint(distance));
        }

        /// <summary>
        /// 궁극기 연계의 다음 순번을 발동한다(SPEC 3.2). 그 순번의 아군이 쓰러져 있으면 발동하지 않는다.
        /// 궁극기마다의 효과는 스킬 시스템(트리거와 효과 데이터)이 붙을 때 이 순번에 연결한다.
        /// </summary>
        public bool TryUltimate()
        {
            if (!ultimate.CanActivate || members[ultimate.ActivatingOrder].IsDown) return false;
            return ultimate.Activate() >= 0;
        }

        // 자동 사격(SPEC 2.1): 살아 있는 적을 하나 골라 그쪽으로 쏜다. 방아쇠를 누르고 떼는 시점만 정하고
        // 사격, 엄폐, 재장전은 UpdateMember 의 규칙을 그대로 따른다. 적이 없으면 쏘지 않고 엄폐해 재장전한다.
        // 강화 상태에서는 조작 중인 아군의 조준 광선(playerRay)을 함께 쏜다(점사). 전원 자동 전투이거나
        // 조작 아군이 쓰러졌으면 점사하지 않고 각자 쏜다.
        private void AutoFire(Member member, Ray playerRay)
        {
            if (ultimate.IsEnhanced && !autoControlSelected && !members[selectedIndex].IsDown)
            {
                AimRig(member.rig, playerRay);
                UpdateMember(member, EnemyUnit.Alive.Count > 0 && AutoTrigger(member), playerRay);
                return;
            }

            if (member.autoTarget == null || member.autoTarget.IsDead || Time.time >= member.autoRetargetTime)
            {
                member.autoTarget = PickTarget(member.rig.transform.position);
                member.autoAimOffset = UnityEngine.Random.insideUnitSphere * autoAimError;
                member.autoRetargetTime = Time.time + autoRetargetInterval;
            }

            bool held = false;
            Ray ray = default;
            if (member.autoTarget != null)
            {
                Vector3 aimPoint = member.autoTarget.AimPoint + member.autoAimOffset;
                ray = new Ray(viewCamera.transform.position, (aimPoint - viewCamera.transform.position).normalized);
                AimRig(member.rig, ray);
                held = AutoTrigger(member);
            }

            UpdateMember(member, held, ray);
        }

        // 자동 사격의 방아쇠. 무기와 상관없이 탄이 0이 될 때까지 누르고 있다. 탄이 0이면 무기가 손을 떼게 해 재장전으로 넘어간다.
        // 재장전 중에는 누르지 않아 탄창을 끝까지 채우게 둔다(자동 사격은 재장전을 취소하지 않는다).
        // 차지형은 가득 찬 그 한 프레임만 떼서 쏘고, 다음 프레임부터 다시 눌러 다음 충전을 기다린다.
        private static bool AutoTrigger(Member member)
        {
            if (member.reloading) return false;
            CharacterWeapon weapon = member.weapon;
            return !weapon.IsCharging || weapon.Charge01 < 1f;
        }

        private static EnemyUnit PickTarget(Vector3 from)
        {
            EnemyUnit best = null;
            float bestDistance = float.MaxValue;
            foreach (EnemyUnit enemy in EnemyUnit.Alive)
            {
                if (enemy.IsDead) continue;
                // 가까이 다가온 적을 먼저 쏜다.
                float distance = (enemy.transform.position - from).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }
            return best;
        }

        private void UpdateMember(Member member, bool held, Ray ray)
        {
            CharacterWeapon weapon = member.weapon;
            int ammoBefore = weapon.Ammo;

            // 무기가 사격 자세를 원하면(누르는 중, 충전 중, 쏜 직후) 사격. 진행 중이던 재장전은 버린다.
            if (weapon.Tick(held, ray))
            {
                member.reloading = false;
                member.poseView.SetPose(CharacterPose.Fire);
                if (weapon.Ammo != ammoBefore) RefreshAmmoText();
                return;
            }

            // 쏘지 않는 동안: 탄이 가득 차 있으면 엄폐, 아니면 재장전을 끝까지 진행한 뒤 엄폐.
            // 탄이 0이면 누르고 있어도 여기로 와서 재장전하고, 끝나면 다시 사격으로 넘어간다.
            if (weapon.Ammo >= weapon.MagazineSize)
            {
                member.poseView.SetPose(CharacterPose.Cover);
                return;
            }

            if (!member.reloading)
            {
                member.reloading = true;
                member.reloadEndTime = Time.time + weapon.StageDuration;
            }

            if (Time.time < member.reloadEndTime)
            {
                member.poseView.SetPose(CharacterPose.Reload);
                return;
            }

            if (weapon.ReloadStages > 1)
            {
                weapon.LoadStage();
                RefreshAmmoText();
                if (weapon.Ammo < weapon.MagazineSize)
                {
                    // 다음 단계. 재장전 자세를 유지한다.
                    member.reloadEndTime = Time.time + weapon.StageDuration;
                    member.poseView.SetPose(CharacterPose.Reload);
                    return;
                }
            }
            else
            {
                weapon.Refill();
                RefreshAmmoText();
            }

            member.reloading = false;
            member.poseView.SetPose(CharacterPose.Cover);
        }

        private void OnAllyDowned(AllyUnit unit)
        {
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].unit != unit) continue;
                members[i].weapon.CancelCharge();
                members[i].reloading = false;
                if (i != selectedIndex) return;

                // 조작하던 아군이 쓰러지면 살아 있는 다음 아군으로 넘긴다.
                for (int step = 1; step < members.Length; step++)
                {
                    int next = (selectedIndex + step) % members.Length;
                    if (members[next].IsDown) continue;
                    selectedIndex = next;
                    RefreshAmmoText();
                    return;
                }
                return;
            }
        }

        // 명중 한 번의 피해를 SPEC 3.1 순서로 계산해 준다. 자동 사격 아군도 조작 아군과 같은 피해를 준다(SPEC 2.1).
        // 피해와 상관없이 명중 한 번마다 궁극기 게이지를 채운다(SPEC 3.2).
        private void OnHit(Member member, Collider collider, Vector3 point, float charge01)
        {
            IHittable target = collider.GetComponentInParent<IHittable>();
            if (target == null) return;

            CharacterWeapon weapon = member.weapon;
            DamageInput hit = DamageInput.Shot(ToFixed(weapon.Attack), ToFixed(weapon.WeaponCoefficient));
            hit.Defense = ToFixed(target.Defense);
            hit.Critical = critRandom.NextUnitFixed() < DamageFormula.BaseCritChance;
            hit.WeakPoint = collider.GetComponent<WeakPoint>() != null;
            hit.InEffectiveRange = InEffectiveRange(weapon, point);
            hit.Enhanced = ultimate.IsEnhanced;
            hit.Charge = ToFixed(charge01);

            target.TakeHit(FromFixed(DamageFormula.Compute(hit)), weapon.ChargeMultiplierAt(charge01), point);
            ultimate.AddGauge(ToFixed(weapon.GaugePerHit));
        }

        // 맞은 지점의 깊이를 아군 줄(0)에서 적이 나타나는 가장 먼 선(1)까지의 비율로 바꿔 무기의 적정 사거리와 비교한다.
        private bool InEffectiveRange(CharacterWeapon weapon, Vector3 point)
        {
            float depth = enemyFarZ - rangeOriginZ;
            float ratio = depth > 0f ? Mathf.Clamp01((point.z - rangeOriginZ) / depth) : 0f;
            Vector2 band = weapon.EffectiveRange;
            return ratio >= band.x && ratio <= band.y;
        }

        private static Fixed ToFixed(float value) => Fixed.FromMilli((long)Mathf.Round(value * Fixed.Scale));

        private static float FromFixed(Fixed value) => value.raw / (float)Fixed.Scale;

        private LineRenderer CreateTracerLike(LineRenderer source, int index)
        {
            var go = new GameObject("Tracer_" + index);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = source.sharedMaterial;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = source.startWidth;
            line.endWidth = source.endWidth;
            line.startColor = source.startColor;
            line.endColor = source.endColor;
            line.shadowCastingMode = source.shadowCastingMode;
            line.receiveShadows = source.receiveShadows;
            return line;
        }

        private void ShowTracer(ShotResult shot)
        {
            Tracer t = tracers[nextTracer];
            nextTracer = (nextTracer + 1) % tracers.Length;

            bool charged = shot.Kind == WeaponKind.Charge;
            float width = t.width * (charged ? 1f + shot.Multiplier : 1f);
            t.line.startWidth = width;
            t.line.endWidth = width * 0.4f;
            t.line.SetPosition(0, shot.From);
            t.line.SetPosition(1, shot.To);
            t.line.enabled = true;
            t.endTime = Time.time + (charged ? chargeTracerDuration : tracerDuration);
        }

        private void UpdateCrosshair()
        {
            CharacterWeapon weapon = members[selectedIndex].weapon;

            // 퍼짐만큼 조준점 팔을 벌린다. 연사형은 쏠수록 벌어지고, 산탄형은 원뿔 크기만큼 늘 벌어져 있다.
            float gap = crosshairGap + weapon.SpreadDegrees * crosshairPixelsPerDegree;
            for (int i = 0; i < 4; i++) crosshairArms[i].anchoredPosition = ArmDirections[i] * gap;

            // 차지형 충전 막대는 충전 중에만 보인다.
            bool charging = weapon.IsCharging;
            if (chargeBar.gameObject.activeSelf != charging) chargeBar.gameObject.SetActive(charging);
            if (charging) chargeFill.localScale = new Vector3(weapon.Charge01, 1f, 1f);
        }

        private void RefreshAmmoText()
        {
            if (ammoText == null) return;
            Member member = members[selectedIndex];
            ammoText.text = member.rig.name + "  " + member.weapon.Ammo + " / " + member.weapon.MagazineSize;
        }
    }
}
