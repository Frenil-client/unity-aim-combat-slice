using System;
using System.Collections.Generic;
using AimCombat.Battle;
using AimCombat.Character;
using AimCombat.Core;
using AimCombat.Core.Combat;
using AimCombat.Core.Random;
using AimCombat.Data;
using AimCombat.Input;
using AimCombat.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Squad
{
    /// <summary>사격 한 발의 명중 결과. 피해 계산이 끝난 뒤의 값이다.</summary>
    public readonly struct ShotHit
    {
        /// <summary>쏜 아군의 배치 순서.</summary>
        public readonly int Member;
        public readonly WeaponKind Weapon;
        /// <summary>맞은 대상(적이면 EnemyUnit, 사격장이면 RangeTarget).</summary>
        public readonly IHittable Target;
        public readonly Vector3 Point;
        public readonly float Damage;
        public readonly bool Critical;
        public readonly bool WeakPoint;
        /// <summary>이 한 발로 대상이 쓰러졌다.</summary>
        public readonly bool Killed;
        public readonly float Charge01;

        public ShotHit(int member, WeaponKind weapon, IHittable target, Vector3 point, float damage, bool critical, bool weakPoint, bool killed, float charge01)
        {
            Member = member;
            Weapon = weapon;
            Target = target;
            Point = point;
            Damage = damage;
            Critical = critical;
            WeakPoint = weakPoint;
            Killed = killed;
            Charge01 = charge01;
        }
    }

    /// <summary>
    /// 아군 분대 조작. 사격장과 전투 씬이 같이 쓴다.
    /// - 고른 한 명은 입력 계층(CombatInput)의 조준점과 방아쇠를 따른다. 무기는 자기 방식대로 쏜다(연사, 차지, 산탄).
    /// - 나머지는 autoFire 가 켜져 있으면 살아 있는 적을 골라 스스로 쏜다(SPEC 2.1).
    ///   무기와 상관없이 탄이 0이 될 때까지 쏘고, 재장전하면 탄창을 다 채운 뒤 다시 쏜다.
    /// - 사격을 멈추면 재장전하고, 재장전이 끝나야 엄폐로 돌아간다. 재장전 도중 다시 쏘면 재장전은 취소된다.
    ///   단계로 나눠 장전하는 무기(산탄형)는 끝난 단계만큼 남고 다음 단계의 시간만 처음부터 센다.
    /// - 명중하면 Core 의 DamageFormula 로 피해를 계산하고(SPEC 3.1) 팀 공용 버스트 게이지를 채운다(SPEC 3.2).
    /// - 버스트(니케 구조): 아군마다 고유 버스트 단계(1~3)와 버스트 스킬이 있다. 연계가 열리면 지금 단계와 같은 단계이고
    ///   쿨다운이 끝난 살아 있는 아군만 후보가 되고, 후보는 배치 순서대로 BurstCandidates 에 놓인다.
    ///   같은 단계지만 쿨다운 중인 아군은 BurstWaiting 에 따로 모아 HUD 가 남은 시간을 보여 준다.
    ///   숫자 1-5 키나 HUD 후보 버튼으로 후보의 n번째를, Q 로 첫 번째를 쓴다. 후보가 없으면 그 단계는 쓸 수 없다.
    ///   자동 버스트(Tab)를 켜면 후보의 첫 번째를 잠시 뒤 스스로 쓴다. 풀 버스트 중에는 자동 사격 아군도 조작 중인 아군의 조준점을 함께 쏜다.
    /// - 스킬: 아군마다 캐릭터 에셋(CharacterData)의 스킬 1, 스킬 2, 버스트 세 칸을 가진다. 스킬은 발동 블록의 목록이다(SPEC 3.2).
    ///   액티브 블록은 쿨다운이 차는 대로 각자 쓰고(적이 없으면 아껴 둔다), 패시브 블록은 SkillTriggerHub 가 전투 이벤트를 받아 실행한다.
    /// - 스킬의 대상 고르기(조준점 아래의 적, 가까운 적)는 ISkillWorld 로 Core 의 SkillRunner 에 공급한다.
    /// - 대상은 Z, X, C, V, B 키나 Select(초상 버튼)로 바꾸고, 왼쪽 Shift 로 고른 아군의 자동 전투를 켜고 끈다.
    ///   자동 전투 중에도 누르는 동안은 수동이다. 손을 떼면 조준점이 고른 아군의 대상으로 옮겨 가고, 장전이나 엄폐 중에는 멈춘다.
    ///   카메라는 고른 아군을 가로 중심으로 따라간다.
    /// - 전투 불능 아군은 쏘지도 고를 수도 스킬을 쓸 수도 없다.
    /// </summary>
    public sealed class SquadController : MonoBehaviour, ISkillWorld
    {
        [Serializable]
        private sealed class Member
        {
            public CharacterAimRig rig;
            public CharacterPoseView poseView;
            public CharacterWeapon weapon;
            [Tooltip("전투 씬에서만 쓴다. 비어 있으면 쓰러지지 않는다(사격장).")]
            public AllyUnit unit;
            [Tooltip("캐릭터의 스킬 구성(SPEC 3.2): 스킬 1, 스킬 2, 버스트 스킬. 비어 있으면 스킬이 없다(사격장).")]
            public CharacterData character;
            [Tooltip("HUD 초상과 버스트 후보 버튼에 쓰는 얼굴 그림.")]
            public Sprite face;
            [Tooltip("버스트 컷인 그림. 비어 있으면 엄폐 포즈 그림을 쓴다.")]
            public Sprite cutIn;

            [NonSerialized] public Combatant combatant;
            /// <summary>스킬 1, 2에 든 액티브 발동 블록. 블록마다 쿨다운을 따로 센다.</summary>
            [NonSerialized] public readonly List<ActiveBlock> actives = new List<ActiveBlock>();
            /// <summary>캐릭터의 버스트 스킬. 없으면 null.</summary>
            [NonSerialized] public SkillData burstSkill;
            [NonSerialized] public SkillCooldown burstCooldown;

            public int BurstStage => burstSkill != null ? burstSkill.BurstStage : 0;
            [NonSerialized] public bool reloading;
            [NonSerialized] public float reloadEndTime;
            [NonSerialized] public EnemyUnit autoTarget;
            [NonSerialized] public Vector3 autoAimOffset;
            [NonSerialized] public float autoRetargetTime;
            [NonSerialized] public bool autoHeld;
            [NonSerialized] public bool autoAiming;
            /// <summary>지금 재장전이 탄이 0이 돼서 시작됐는지. 자동 사격은 이 재장전만 끝까지 채운다.</summary>
            [NonSerialized] public bool emptyReload;

            public bool IsDown => unit != null && unit.IsDown;
        }

        private sealed class ActiveBlock
        {
            public SkillData skill;
            public SkillBlockDef block;
            public SkillCooldown cooldown;
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
        [Tooltip("조준점의 위치. 모양과 잔탄 표시는 여기에 붙은 CrosshairView 가 그린다(없으면 실행할 때 붙인다).")]
        [SerializeField] private RectTransform crosshair;
        [Tooltip("궤적 원본. 실행하면 같은 설정으로 여러 개를 만들어 산탄의 탄알마다 하나씩 쓴다.")]
        [SerializeField] private LineRenderer tracer;
        [Tooltip("사격장용 탄약 표시. 전투 씬은 HUD 가 따로 그린다.")]
        [SerializeField] private Text ammoText;
        [Tooltip("피해 숫자 표시. 비어 있으면 띄우지 않는다.")]
        [SerializeField] private DamageNumbers damageNumbers;

        [Header("자동 사격")]
        [Tooltip("고르지 않은 아군이 살아 있는 적을 스스로 쏜다.")]
        [SerializeField] private bool autoFire;
        [Tooltip("자동 사격 조준이 적 중심에서 벗어나는 최대 거리(유닛). 클수록 잘 빗나간다.")]
        [SerializeField] private float autoAimError = 0.5f;
        [Tooltip("켜면 고른 아군도 자동으로 쏜다(전원 자동 전투).")]
        [SerializeField] private bool autoControlSelected;
        [Tooltip("자동 사격 아군이 대상을 다시 고르는 간격(초).")]
        [SerializeField] private float autoRetargetInterval = 1.5f;
        [Tooltip("자동 전투에서 손을 뗐을 때 조준점이 대상으로 옮겨 가는 속도(초당 픽셀, 1080 기준).")]
        [SerializeField] private float autoReticleSpeed = 2200f;
        [Tooltip("자동 전투에서 조준점이 대상에 이만큼 가까워지면 쏘기 시작한다(픽셀, 1080 기준).")]
        [SerializeField] private float autoReticleTolerance = 24f;

        [Header("자동 버스트")]
        [Tooltip("켜면 버스트 후보의 첫 번째(배치 순서로 가장 왼쪽)를 스스로 쓴다(Tab).")]
        [SerializeField] private bool autoBurst;
        [Tooltip("자동 버스트가 단계가 열린 뒤 쓰기까지 기다리는 시간(초). 니케의 자동 버스트도 수동보다 느리다.")]
        [SerializeField] private float autoBurstDelay = 0.5f;

        [Header("피해")]
        [Tooltip("적이 나타나는 가장 먼 선의 깊이(z). 아군 줄에서 여기까지를 적정 사거리 100%로 본다(SPEC 2절).")]
        [SerializeField] private float enemyFarZ = 28f;
        [Tooltip("치명 판정 난수의 시드. 같은 시드면 같은 순서로 치명이 나온다.")]
        [SerializeField] private int critSeed = 20261114;

        [Header("버스트")]
        [SerializeField] private float burstGaugeMax = 10000f;
        [Tooltip("3단계 버스트를 쓴 뒤 풀 버스트가 이어지는 시간(초).")]
        [SerializeField] private float fullBurstDuration = 10f;

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
        private CrosshairView crosshairView;
        private float rangeOriginZ;
        private DeterministicRandom critRandom;
        private BurstChain burst;
        private SkillTriggerHub triggers;
        private static readonly SkillSlot[] SkillSlots = { SkillSlot.Skill1, SkillSlot.Skill2, SkillSlot.Burst };
        private bool wasFullBurst;
        private readonly List<int> burstCandidates = new List<int>();
        private readonly List<int> burstWaiting = new List<int>();
        private float autoBurstTime = -1f;
        private readonly List<Combatant> allies = new List<Combatant>();
        private readonly List<Combatant> enemies = new List<Combatant>();
        private readonly List<EnemyUnit> enemyBuffer = new List<EnemyUnit>();

        /// <summary>스킬이나 버스트를 썼다. 인자는 배치 순서와 스킬. HUD 알림이 구독한다.</summary>
        public event Action<int, SkillData> SkillUsed;

        /// <summary>
        /// 패시브 스킬이 발동했다. 인자는 배치 순서와 스킬. 명중마다 발동하는 패시브도 있어 SkillUsed 와 따로 알린다.
        /// </summary>
        public event Action<int, SkillData> PassiveTriggered;

        /// <summary>아군이 방아쇠 한 번을 쐈다. 인자는 배치 순서와 충전량. 타격감(총구 화염, 사격음, 흔들림)이 구독한다.</summary>
        public event Action<int, float> ShotFired;

        /// <summary>사격 한 발이 대상에 맞았다(피해 계산 뒤). 타격감(히트스톱, 플래시, 명중 이펙트)이 구독한다.</summary>
        public event Action<ShotHit> HitLanded;

        public Camera ViewCamera => viewCamera;
        public CharacterAimRig RigOf(int index) => members[index].rig;

        /// <summary>팀 공용 버스트 게이지와 연계 상태(SPEC 3.2).</summary>
        public BurstChain Burst => burst;

        /// <summary>지금 단계에서 버스트를 쓸 수 있는 아군의 배치 순서. 왼쪽부터 정렬돼 있다. 숫자 1-5 키가 이 목록의 n번째를 고른다.</summary>
        public IReadOnlyList<int> BurstCandidates => burstCandidates;

        /// <summary>
        /// 지금 단계와 버스트 단계는 같지만 버스트 쿨다운이 남아 쓸 수 없는 살아 있는 아군의 배치 순서(SPEC 3.2).
        /// HUD 가 후보 뒤에 남은 시간과 함께 보여 주기만 한다. 숫자 키와 자동 버스트는 세지 않는다.
        /// </summary>
        public IReadOnlyList<int> BurstWaiting => burstWaiting;

        public SkillData BurstSkillOf(int index) => members[index].burstSkill;
        public Sprite FaceOf(int index) => members[index].face;
        public Sprite CutInOf(int index) => members[index].cutIn;
        public SkillCooldown BurstCooldownOf(int index) => members[index].burstCooldown;

        public int Count => members.Length;
        public int SelectedIndex => selectedIndex;
        public string NameOf(int index) => members[index].rig.name;
        public CharacterWeapon WeaponOf(int index) => members[index].weapon;
        public AllyUnit UnitOf(int index) => members[index].unit;
        public bool IsReloading(int index) => members[index].reloading;
        public Combatant CombatantOf(int index) => members[index].combatant;
        /// <summary>액티브 발동 블록이 든 스킬(스킬 1을 먼저 본다). HUD 의 쿨다운 줄이 쓴다. 없으면 null.</summary>
        public SkillData ActiveSkillOf(int index) => members[index].actives.Count > 0 ? members[index].actives[0].skill : null;

        /// <summary>캐릭터의 스킬 구성. 연결되지 않았으면 null.</summary>
        public CharacterData CharacterOf(int index) => members[index].character;

        /// <summary>칸별 스킬(스킬 1, 스킬 2, 버스트). 모든 캐릭터가 같은 세 칸이라 UI 가 같은 모양으로 보여 줄 수 있다. 비어 있으면 null.</summary>
        public SkillData SkillOf(int index, SkillSlot slot) => members[index].character != null ? members[index].character.SkillOf(slot) : null;

        /// <summary>패시브 발동 판정(SPEC 3.2 패시브 트리거).</summary>
        public SkillTriggerHub Triggers => triggers;
        public SkillCooldown CooldownOf(int index) => members[index].actives.Count > 0 ? members[index].actives[0].cooldown : null;

        /// <summary>고른 아군까지 자동으로 쏘게 한다(전원 자동 전투).</summary>
        public bool AutoControlSelected
        {
            get => autoControlSelected;
            set => autoControlSelected = value;
        }

        /// <summary>버스트 후보의 첫 번째를 스스로 쓰게 한다.</summary>
        public bool AutoBurst
        {
            get => autoBurst;
            set => autoBurst = value;
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
            burst = new BurstChain(ToFixed(burstGaugeMax), ToFixed(fullBurstDuration));
            triggers = new SkillTriggerHub(this, critRandom);
            triggers.Triggered += OnPassiveTriggered;

            // 궤적 원본을 Instantiate 로 복제하면 같은 오브젝트에 붙은 컴포넌트까지 복제될 수 있다.
            // 빈 오브젝트에 LineRenderer 만 새로 만들고 설정을 옮긴다.
            tracers = new Tracer[Mathf.Max(1, tracerPoolSize)];
            for (int i = 0; i < tracers.Length; i++)
            {
                LineRenderer line = i == 0 ? tracer : CreateTracerLike(tracer, i);
                line.enabled = false;
                tracers[i] = new Tracer { line = line, width = tracer.startWidth };
            }

            // 전투 단위와 스킬. 사격장처럼 AllyUnit 이 없으면 버프만 받는 전투 단위를 따로 만든다.
            foreach (Member member in members)
            {
                member.combatant = member.unit != null
                    ? member.unit.Combatant
                    : new Combatant(member.rig.name, Fixed.One, ToFixed(member.weapon.Attack), Fixed.Zero) { Owner = member.rig };
                allies.Add(member.combatant);

                CharacterData character = member.character;
                if (character == null) continue;
                CheckCharacter(character);

                // 스킬 1, 스킬 2, 버스트 순서로 등록한다. 패시브 블록은 발동 판정(triggers)에,
                // 액티브 블록은 쿨다운과 함께 따로 모으고, 버스트 블록은 TryBurst 가 직접 실행한다.
                member.burstSkill = character.BurstSkill;
                member.burstCooldown = member.burstSkill != null ? new SkillCooldown(member.burstSkill.ToCore().Cooldown, startReady: true) : null;
                foreach (SkillSlot slot in SkillSlots)
                {
                    SkillData skill = character.SkillOf(slot);
                    if (skill == null) continue;
                    SkillDef def = skill.ToCore();
                    triggers.Add(member.combatant, def);
                    foreach (SkillBlockDef block in def.Blocks)
                    {
                        if (block.Trigger != SkillTrigger.Active) continue;
                        member.actives.Add(new ActiveBlock { skill = skill, block = block, cooldown = new SkillCooldown(block.Cooldown) });
                    }
                }
            }

            for (int i = 0; i < members.Length; i++)
            {
                int index = i;
                Member member = members[i];
                CharacterWeapon weapon = member.weapon;
                weapon.Fired += charge01 =>
                {
                    ShotFired?.Invoke(index, charge01);
                    // 이 시점에는 아직 탄을 빼지 않았다. 남은 탄이 1이면 지금 쏜 것이 마지막 탄환이다.
                    triggers.Raise(SkillTrigger.ShotFired, member.combatant);
                    if (weapon.Ammo <= 1) triggers.Raise(SkillTrigger.LastBulletFired, member.combatant);
                    triggers.Flush();
                };
            }

            // 스킬과 지속 피해로 적이 쓰러진 것도 처치로 센다. 사격 처치는 OnHit 에서 센다.
            EnemyUnit.NonShotDamaged += OnEnemyNonShotDamaged;

            foreach (Member member in members)
            {
                CharacterWeapon weapon = member.weapon;
                weapon.Traced += ShowTracer;
                weapon.Hit += (collider, point, charge01) => OnHit(member, collider, point, charge01);
                if (member.unit != null) member.unit.Downed += OnAllyDowned;
            }

            crosshairView = crosshair.GetComponent<CrosshairView>();
            if (crosshairView == null) crosshairView = crosshair.gameObject.AddComponent<CrosshairView>();

            RefreshAmmoText();
        }

        // 다른 컴포넌트의 Awake(적 배치 등)가 끝난 뒤에 전투 시작 패시브를 세운다. 실행은 첫 Update 의 Tick 에서 한다.
        private void Start()
        {
            triggers.Raise(SkillTrigger.BattleStart);
        }

        private void OnDestroy()
        {
            EnemyUnit.NonShotDamaged -= OnEnemyNonShotDamaged;
        }

        private void OnPassiveTriggered(Combatant caster, SkillDef skill, SkillBlockDef block)
        {
            SkillData data = SkillData.Find(skill);
            int index = IndexOf(caster);
            if (data != null && index >= 0) PassiveTriggered?.Invoke(index, data);
        }

        private void OnEnemyNonShotDamaged(EnemyUnit enemy, DamageEvent e)
        {
            // 스킬 실행 도중에 불린다. 줄에만 세우고 실행은 그 스킬이 끝난 뒤의 Flush 가 한다.
            if (e.Target == null || e.Target.IsAlive || e.Source == null || IndexOf(e.Source) < 0) return;
            triggers.Raise(SkillTrigger.Kill, e.Source, e.Target);
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
            if (input.AutoBurstTogglePressed) autoBurst = !autoBurst;
            if (input.AutoBattleTogglePressed) autoControlSelected = !autoControlSelected;

            Fixed deltaTime = CombatTime.Delta;
            burst.Tick(deltaTime);
            if (wasFullBurst && !burst.IsFullBurst) triggers.Raise(SkillTrigger.FullBurstEnd);
            wasFullBurst = burst.IsFullBurst;
            triggers.Tick(deltaTime, EnemyUnit.Alive.Count > 0);
            UpdateSkills(deltaTime);
            if (input.BurstRequest >= 0) TryBurstCandidate(input.BurstRequest);
            UpdateAutoBurst();

            Member selected = members[selectedIndex];
            bool selectedAuto = autoControlSelected && !input.FireHeld;
            if (!selected.IsDown && selectedAuto) UpdateAutoSelected(selected);

            Vector2 screenPoint = input.AimScreenPosition;
            crosshair.position = screenPoint;
            Ray ray = viewCamera.ScreenPointToRay(screenPoint);
            if (!selected.IsDown) AimRig(selected.rig, ray);

            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (member.IsDown) continue;

                if (i == selectedIndex)
                {
                    // 수동이면 방아쇠는 입력을 따르고, 자동 전투 중에도 누르는 동안은 수동이다(니케 자동 조준).
                    // 손을 떼면 자동이 조준점을 목표로 옮기고 다 옮겼을 때 방아쇠를 누른다.
                    UpdateMember(member, selectedAuto ? member.autoHeld : input.FireHeld, ray, selectedAuto && member.autoAiming);
                }
                else if (autoFire)
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

        /// <summary>버스트 후보 목록의 n번째(0부터) 아군의 버스트를 쓴다. 후보가 그만큼 없으면 쓰지 않는다.</summary>
        public bool TryBurstCandidate(int candidate)
        {
            if (candidate < 0 || candidate >= burstCandidates.Count) return false;
            return TryBurst(burstCandidates[candidate]);
        }

        /// <summary>
        /// 배치 순서 index 아군의 버스트를 쓴다(SPEC 3.2). 연계가 그 아군의 버스트 단계를 기다리고 있고, 쿨다운이 끝났고,
        /// 쓰러지지 않았을 때만 쓴다. 쓰면 그 순간부터 버스트 쿨다운을 센다.
        /// 스킬을 연계 처리보다 먼저 실행한다. 3단계 버스트 피해는 풀 버스트가 시작되기 전에 들어가야 하기 때문이다(SPEC 3.1).
        /// </summary>
        public bool TryBurst(int index)
        {
            if (index < 0 || index >= members.Length) return false;
            Member member = members[index];
            if (!CanBurst(member) || !member.burstCooldown.TryUse()) return false;

            SkillDef skill = member.burstSkill.ToCore();
            foreach (SkillBlockDef block in skill.Blocks)
            {
                if (block.Trigger == SkillTrigger.Burst) SkillRunner.Execute(block, member.combatant, this, critRandom);
            }
            SkillUsed?.Invoke(index, member.burstSkill);

            // 버스트 사용 패시브도 연계 처리보다 먼저 실행한다. 그래야 "버스트 사용 시 얻는 상태"를
            // "풀 버스트 타임 시작 시 그 상태라면" 조건이 읽을 수 있다.
            triggers.Raise(SkillTrigger.BurstUsed, member.combatant);
            triggers.Raise(SkillTrigger.AllyBurstUsed, null, member.combatant);
            triggers.Flush();

            burst.Use(skill.BurstStage, skill.NextStageWindow);
            if (burst.IsFullBurst && !wasFullBurst)
            {
                wasFullBurst = true;
                triggers.Raise(SkillTrigger.FullBurstStart);
                triggers.Flush();
            }
            RefreshBurstCandidates();
            return true;
        }

        private bool CanBurst(Member member)
        {
            return !member.IsDown && member.burstSkill != null && member.burstCooldown != null
                && member.burstCooldown.IsReady && burst.Accepts(member.BurstStage);
        }

        // 지금 단계에서 쓸 수 있는 아군을 배치 순서대로 모은다. 같은 단계라도 쓰러졌으면 빠지고,
        // 쿨다운 중이면 후보가 아니라 대기 목록(HUD 표시 전용)에 들어간다.
        private void RefreshBurstCandidates()
        {
            burstCandidates.Clear();
            burstWaiting.Clear();
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (CanBurst(member)) burstCandidates.Add(i);
                else if (IsBurstCoolingDown(member)) burstWaiting.Add(i);
            }
        }

        private bool IsBurstCoolingDown(Member member)
        {
            return !member.IsDown && member.burstSkill != null && member.burstCooldown != null
                && !member.burstCooldown.IsReady && burst.Accepts(member.BurstStage);
        }

        // 자동 버스트: 후보가 생기면 잠시 기다렸다가 첫 번째(가장 왼쪽)를 쓴다. 적이 없으면 아껴 둔다.
        private void UpdateAutoBurst()
        {
            if (!autoBurst || burstCandidates.Count == 0 || EnemyUnit.Alive.Count == 0)
            {
                autoBurstTime = -1f;
                return;
            }
            if (autoBurstTime < 0f) autoBurstTime = Time.time + autoBurstDelay;
            if (Time.time < autoBurstTime) return;
            autoBurstTime = -1f;
            TryBurstCandidate(0);
        }

        /// <summary>
        /// 배치 순서 index 아군의 액티브 발동 블록 중 쿨다운이 찬 것을 모두 쓴다. 하나라도 썼으면 true.
        /// 쓰러졌으면 쓰지 않는다.
        /// </summary>
        public bool TryActiveSkill(int index)
        {
            if (index < 0 || index >= members.Length) return false;
            Member member = members[index];
            if (member.IsDown) return false;

            bool used = false;
            foreach (ActiveBlock active in member.actives)
            {
                if (!active.cooldown.TryUse()) continue;
                SkillRunner.Execute(active.block, member.combatant, this, critRandom);
                SkillUsed?.Invoke(index, active.skill);
                triggers.Flush(); // 이 스킬이 일으킨 처치, 버프 획득 패시브
                used = true;
                if (member.IsDown) break;
            }
            return used;
        }

        // 액티브 블록과 버스트의 쿨다운을 돌린다. 액티브는 조작 여부와 상관없이 각자 쿨다운이 차는 대로 쓴다(니케의 스킬 1, 2처럼).
        // 적이 없으면 아껴 둔다. 끝에 버스트 후보 목록을 새로 만든다.
        private void UpdateSkills(Fixed deltaTime)
        {
            bool enemiesPresent = EnemyUnit.Alive.Count > 0;
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (member.IsDown) continue;
                if (member.burstCooldown != null) member.burstCooldown.Tick(deltaTime);

                bool ready = false;
                foreach (ActiveBlock active in member.actives)
                {
                    active.cooldown.Tick(deltaTime);
                    ready |= active.cooldown.IsReady;
                }
                if (enemiesPresent && ready) TryActiveSkill(i);
            }
            RefreshBurstCandidates();
        }

        // 분대원에 잘못 연결된 스킬을 실행 전에 알린다. 에디터 인스펙터, 스킬 관리 창과 같은 검증 규칙이다.
        private static void CheckCharacter(CharacterData character)
        {
            List<string> errors = character.Validate();
            if (errors.Count > 0) Debug.LogWarning("[Squad] " + character.name + " 스킬 검증 실패\n" + string.Join("\n", errors), character);
        }

        // ---- ISkillWorld: 스킬 실행기가 묻는 전투 상황 ----

        IReadOnlyList<Combatant> ISkillWorld.Allies => allies;

        IReadOnlyList<Combatant> ISkillWorld.Enemies
        {
            get
            {
                enemies.Clear();
                foreach (EnemyUnit enemy in EnemyUnit.Alive)
                {
                    if (!enemy.IsDead) enemies.Add(enemy.Combatant);
                }
                return enemies;
            }
        }

        bool ISkillWorld.Enhanced => burst.IsFullBurst;

        // 조작 중인 아군은 조준점 아래의 적, 자동 사격 아군은 노리던 적. 없으면 가장 가까운 적.
        Combatant ISkillWorld.FocusEnemy(Combatant caster)
        {
            int index = IndexOf(caster);
            if (index < 0) return null;
            Member member = members[index];

            EnemyUnit focus = null;
            if (index == selectedIndex && !autoControlSelected) focus = EnemyUnderCrosshair();
            else if (member.autoTarget != null && !member.autoTarget.IsDead) focus = member.autoTarget;
            if (focus == null) focus = PickTarget(member.rig.transform.position);
            return focus != null ? focus.Combatant : null;
        }

        void ISkillWorld.NearestEnemies(Combatant caster, int count, List<Combatant> result)
        {
            int index = IndexOf(caster);
            Vector3 from = index >= 0 ? members[index].rig.transform.position : Vector3.zero;

            enemyBuffer.Clear();
            foreach (EnemyUnit enemy in EnemyUnit.Alive)
            {
                if (!enemy.IsDead) enemyBuffer.Add(enemy);
            }
            enemyBuffer.Sort((a, b) => (a.transform.position - from).sqrMagnitude.CompareTo((b.transform.position - from).sqrMagnitude));
            for (int i = 0; i < count && i < enemyBuffer.Count; i++) result.Add(enemyBuffer[i].Combatant);
        }

        void ISkillWorld.RefillAmmo(Combatant ally)
        {
            int index = IndexOf(ally);
            if (index < 0) return;
            members[index].weapon.Refill();
            members[index].reloading = false;
            RefreshAmmoText();
        }

        private int IndexOf(Combatant combatant)
        {
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].combatant == combatant) return i;
            }
            return -1;
        }

        private EnemyUnit EnemyUnderCrosshair()
        {
            Ray ray = viewCamera.ScreenPointToRay(input.AimScreenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 200f)) return null;
            EnemyUnit enemy = hit.collider.GetComponentInParent<EnemyUnit>();
            return enemy != null && !enemy.IsDead ? enemy : null;
        }

        // 대상을 다시 고를 때가 됐으면 가장 가까운 적을 고르고 조준 오차를 새로 뽑는다(SPEC 2.1).
        private void RefreshAutoTarget(Member member)
        {
            if (member.autoTarget != null && !member.autoTarget.IsDead && Time.time < member.autoRetargetTime) return;
            member.autoTarget = PickTarget(member.rig.transform.position);
            member.autoAimOffset = UnityEngine.Random.insideUnitSphere * autoAimError;
            member.autoRetargetTime = Time.time + autoRetargetInterval;
        }

        // 자동 전투 중 손을 뗀 조작 아군(SPEC 2.1 조준점). 대상은 자동 사격과 같은 규칙으로 고른다.
        // - 쏠 차례(대상이 있고 장전 중이 아님)이면 조준점을 대상의 조준 지점 쪽으로 일정 속도로 옮기고, 닿으면 방아쇠를 누른다.
        //   그래서 화면의 조준점이 곧 실제 사격 방향이다.
        // - 탄이 0이 돼서 장전하는 중이거나 대상이 없어 엄폐하는 동안에는 조준점을 그 자리에 멈춰 둔다.
        //   잔탄이 남은 채 장전하던 중에 대상이 생기면 바로 조준점을 옮겨 쏜다(장전은 취소된다).
        // 플레이어가 다시 누르면 그 순간부터 입력이 조준점을 넘겨받는다(Update).
        private void UpdateAutoSelected(Member member)
        {
            member.autoHeld = false;
            member.autoAiming = false;
            RefreshAutoTarget(member);
            if (member.autoTarget == null || (member.reloading && member.emptyReload)) return;

            Vector3 screen = viewCamera.WorldToScreenPoint(member.autoTarget.AimPoint + member.autoAimOffset);
            if (screen.z <= 0f) return;

            float scale = Screen.height / 1080f;
            Vector2 goal = screen;
            Vector2 next = Vector2.MoveTowards(input.AimScreenPosition, goal, autoReticleSpeed * scale * Time.deltaTime);
            input.SetAim(next);
            member.autoAiming = true; // 목표로 옮기는 중에는 손을 뗀 것으로 치지 않는다(재장전하지 않음)

            float tolerance = autoReticleTolerance * scale;
            member.autoHeld = (next - goal).sqrMagnitude <= tolerance * tolerance && AutoTrigger(member);
        }

        // 자동 사격(SPEC 2.1): 살아 있는 적을 하나 골라 그쪽으로 쏜다. 방아쇠를 누르고 떼는 시점만 정하고
        // 사격, 엄폐, 재장전은 UpdateMember 의 규칙을 그대로 따른다. 적이 없으면 쏘지 않고 엄폐해 재장전한다.
        // 풀 버스트 중에는 조작 중인 아군의 조준 광선(playerRay)을 함께 쏜다(점사). 전원 자동 전투이거나
        // 조작 아군이 쓰러졌으면 점사하지 않고 각자 쏜다.
        private void AutoFire(Member member, Ray playerRay)
        {
            if (burst.IsFullBurst && !autoControlSelected && !members[selectedIndex].IsDown)
            {
                AimRig(member.rig, playerRay);
                UpdateMember(member, EnemyUnit.Alive.Count > 0 && AutoTrigger(member), playerRay);
                return;
            }

            RefreshAutoTarget(member);

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
        // 탄이 0이 돼서 시작한 재장전은 누르지 않아 끝까지 채운다(산탄형도 3단계를 다 넣는다).
        // 잔탄이 남은 채 시작한 재장전(쏠 대상이 없어 엄폐하며 넣던 것)은 대상이 생기면 바로 눌러 취소하고 남은 탄으로 쏜다(SPEC 2.1).
        // 차지형은 가득 찬 그 한 프레임만 떼서 쏘고, 다음 프레임부터 다시 눌러 다음 충전을 기다린다.
        private static bool AutoTrigger(Member member)
        {
            if (member.reloading && member.emptyReload) return false;
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

        // keepAiming: 방아쇠는 떼고 있지만 다음 목표로 조준점을 옮기는 중이다(자동 전투의 조작 아군).
        // 이때는 손을 뗀 것으로 치지 않아 재장전을 시작하지 않고 사격 자세를 유지한다. 탄이 0이면 그대로 재장전한다.
        private void UpdateMember(Member member, bool held, Ray ray, bool keepAiming = false)
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

            if (keepAiming && !member.reloading && weapon.Ammo > 0)
            {
                member.poseView.SetPose(CharacterPose.Fire);
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
                member.emptyReload = weapon.Ammo == 0;
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
            triggers.Raise(SkillTrigger.Reloaded, member.combatant);
            triggers.Flush();
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
        // 피해와 상관없이 명중 한 번마다 버스트 게이지를 채운다(SPEC 3.2).
        private void OnHit(Member member, Collider collider, Vector3 point, float charge01)
        {
            IHittable target = collider.GetComponentInParent<IHittable>();
            if (target == null) return;
            Combatant victim = target.Combatant;
            if (victim != null && !victim.IsAlive) return;

            // 공격력은 무기에서 읽는다. 실행 중 인스펙터에서 바꾼 값도 바로 들어간다.
            CharacterWeapon weapon = member.weapon;
            Combatant attacker = member.combatant;
            attacker.Attack = ToFixed(weapon.Attack);

            DamageInput hit = DamageInput.Shot(attacker.Attack, ToFixed(weapon.WeaponCoefficient));
            hit.ApplyStatuses(attacker, victim);
            hit.Critical = critRandom.NextUnitFixed() < DamageFormula.CritChance(attacker);
            hit.WeakPoint = collider.GetComponent<WeakPoint>() != null;
            hit.InEffectiveRange = InEffectiveRange(weapon, point);
            hit.Enhanced = burst.IsFullBurst;
            hit.Charge = ToFixed(charge01);

            Fixed damage = DamageFormula.Compute(hit);
            if (victim != null) victim.ApplyDamage(damage, DamageSource.Shot, attacker, hit.Critical, hit.WeakPoint);

            // 보스의 부위(팔, 저지 표적)에 맞았으면 같은 피해로 부위 체력과 저지 게이지도 깎는다(SPEC 3.4).
            BossPart part = collider.GetComponent<BossPart>();
            if (part != null && victim != null && victim.IsAlive) part.TakeShot(FromFixed(damage));

            target.OnShot(weapon.ChargeMultiplierAt(charge01), point);
            if (damageNumbers != null)
            {
                bool controlled = member == members[selectedIndex] || autoControlSelected;
                damageNumbers.Add(target, point, FromFixed(damage), hit.Critical, hit.WeakPoint, controlled);
            }
            burst.AddGauge(ToFixed(weapon.GaugePerHit));

            bool killed = victim != null && !victim.IsAlive;
            HitLanded?.Invoke(new ShotHit(
                Array.IndexOf(members, member), weapon.Kind, target, point, FromFixed(damage),
                hit.Critical, hit.WeakPoint, killed, charge01));

            // 명중 패시브. 대상(victim)은 TriggerEnemy 가 고른다. 사격장 표적처럼 전투 단위가 없으면 횟수만 센다.
            // 탄은 발사 처리가 끝난 뒤에 빼므로 남은 탄이 1이면 지금 맞은 것이 마지막 탄환이다.
            triggers.Raise(SkillTrigger.Hit, attacker, victim);
            if (hit.Critical) triggers.Raise(SkillTrigger.CriticalHit, attacker, victim);
            if (hit.WeakPoint) triggers.Raise(SkillTrigger.WeakPointHit, attacker, victim);
            if (weapon.Kind == WeaponKind.Charge && charge01 >= 1f) triggers.Raise(SkillTrigger.FullChargeHit, attacker, victim);
            if (weapon.Ammo <= 1) triggers.Raise(SkillTrigger.LastBulletHit, attacker, victim);
            if (killed) triggers.Raise(SkillTrigger.Kill, attacker, victim);
            triggers.Flush();
        }

        // 맞은 지점의 깊이를 아군 줄(0)에서 적이 나타나는 가장 먼 선(1)까지의 비율로 바꿔 무기의 적정 사거리와 비교한다.
        private bool InEffectiveRange(CharacterWeapon weapon, Vector3 point)
        {
            float depth = enemyFarZ - rangeOriginZ;
            float ratio = depth > 0f ? Mathf.Clamp01((point.z - rangeOriginZ) / depth) : 0f;
            Vector2 band = weapon.EffectiveRange;
            return ratio >= band.x && ratio <= band.y;
        }

        private static Fixed ToFixed(float value) => FixedConvert.ToFixed(value);

        private static float FromFixed(Fixed value) => FixedConvert.ToFloat(value);

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
            Member member = members[selectedIndex];
            CharacterWeapon weapon = member.weapon;

            // 조준점은 조작 중인 아군의 무기 종류에 맞는 모양으로 그린다(SPEC 2절 조준점 표시).
            // 퍼짐만큼 커진다: 연사형은 쏠수록 팔이 벌어지고, 산탄형은 원뿔 크기만큼의 원이다. 잔탄과 재장전 진행을 옆에 띄운다.
            float stage = weapon.StageDuration;
            var state = new CrosshairState
            {
                Kind = weapon.Kind,
                SpreadRadius = crosshairGap + weapon.SpreadDegrees * crosshairPixelsPerDegree,
                Charging = weapon.IsCharging,
                Charge01 = weapon.Charge01,
                Ammo = weapon.Ammo,
                MagazineSize = weapon.MagazineSize,
                Reloading = member.reloading,
                Reload01 = member.reloading && stage > 0f ? Mathf.Clamp01(1f - (member.reloadEndTime - Time.time) / stage) : 0f,
            };
            crosshairView.Show(state);
        }

        private void RefreshAmmoText()
        {
            if (ammoText == null) return;
            Member member = members[selectedIndex];
            ammoText.text = member.rig.name + "  " + member.weapon.Ammo + " / " + member.weapon.MagazineSize;
        }
    }
}
