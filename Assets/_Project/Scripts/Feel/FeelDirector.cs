using AimCombat.Battle;
using AimCombat.Character;
using AimCombat.Core.Combat;
using AimCombat.Data;
using AimCombat.Squad;
using AimCombat.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AimCombat.Feel
{
    /// <summary>
    /// 타격감 연출(SPEC 4절). 분대의 사격, 명중, 스킬과 보스 강타, 아군 피격 사건을 받아 무기별 프리셋대로
    /// 히트스톱, 카메라 흔들림, 피격 플래시, 명중 불꽃, 총구 화염, 탄피, 사운드를 낸다.
    /// - 히트스톱과 흔들림은 조작 중인 아군의 사격에만 건다. 자동 사격 아군의 사격음은 작게 낸다.
    /// - 산탄처럼 한 프레임에 여러 발이 맞으면 명중음은 아군마다 한 번만 낸다.
    /// - F2 로 전부 켜고 끈다(적용 전후 비교 영상용). 끄면 1단계와 같은 흰 번쩍임만 남는다.
    /// 연출은 결과만 보여준다. 피해, 판정, 진행에는 손대지 않는다.
    /// </summary>
    public sealed class FeelDirector : MonoBehaviour
    {
        [SerializeField] private SquadController squad;
        [SerializeField] private FeelSettings settings;
        [Tooltip("무기 종류 순서(연사형, 차지형, 산탄형)의 프리셋.")]
        [SerializeField] private FeelPreset[] presets = new FeelPreset[3];
        [SerializeField] private HitStop hitStop;
        [SerializeField] private CameraShake shake;
        [SerializeField] private HitEffects effects;
        [SerializeField] private FeelAudio feelAudio;
        [SerializeField] private bool feelEnabled = true;

        private int[] muzzles;
        private int[] lastHitSoundFrame;

        public bool FeelEnabled
        {
            get => feelEnabled;
            set
            {
                feelEnabled = value;
                if (!value && shake != null) shake.Clear();
            }
        }

        public FeelSettings Settings => settings;
        public FeelPreset PresetOf(WeaponKind kind) => presets[(int)kind];
        public int PresetCount => presets.Length;
        public FeelPreset PresetAt(int index) => presets[index];

        private void Awake()
        {
            if (shake != null) shake.SetSettings(settings);

            muzzles = new int[squad.Count];
            lastHitSoundFrame = new int[squad.Count];
            for (int i = 0; i < squad.Count; i++)
            {
                CharacterAimRig rig = squad.RigOf(i);
                SpriteRenderer body = rig.GetComponentInChildren<SpriteRenderer>();
                muzzles[i] = effects != null ? effects.RegisterMuzzle(rig.Muzzle, body != null ? body.sharedMaterial : null) : -1;
                lastHitSoundFrame[i] = -1;

                AllyUnit unit = squad.UnitOf(i);
                if (unit != null) unit.Wounded += OnAllyHurt;
            }

            squad.ShotFired += OnShot;
            squad.HitLanded += OnHit;
            squad.SkillUsed += OnSkillUsed;
        }

        private void OnEnable() => BossBrain.SlamLanded += OnSlam;

        private void OnDisable() => BossBrain.SlamLanded -= OnSlam;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f2Key.wasPressedThisFrame) FeelEnabled = !FeelEnabled;
            if (feelAudio != null && settings != null) feelAudio.MasterVolume = settings.masterVolume;
        }

        private bool IsControlled(int member) => member == squad.SelectedIndex;

        // 쏠 때: 총구 화염, 탄피, 사격음, 반동 흔들림.
        private void OnShot(int member, float charge01)
        {
            if (!feelEnabled) return;
            CharacterWeapon weapon = squad.WeaponOf(member);
            FeelPreset preset = PresetOf(weapon.Kind);
            if (preset == null) return;
            bool controlled = IsControlled(member);
            bool charged = weapon.Kind == WeaponKind.Charge;

            if (effects != null)
            {
                effects.Muzzle(muzzles[member], preset.muzzleFlashScale * (charged ? 1f + charge01 : 1f), preset.muzzleFlashDuration);
                Transform muzzle = squad.RigOf(member).Muzzle;
                effects.Shell(muzzle.position - muzzle.right * 0.25f, squad.ViewCamera.transform.right, preset.shellCount);
            }

            if (feelAudio != null)
            {
                AudioClip clip = preset.fireClip != null ? preset.fireClip : feelAudio.DefaultFireClip(weapon.Kind);
                float volume = preset.fireVolume * (controlled ? 1f : settings.autoAllyVolume);
                float pitch = preset.firePitch + Random.Range(-preset.pitchJitter, preset.pitchJitter);
                feelAudio.Play(clip, volume, pitch);
            }

            if (controlled && shake != null) shake.Add(preset.fireShake * (charged ? charge01 : 1f));
        }

        // 맞을 때: 피격 플래시, 불꽃, 명중음. 조작 중인 아군이면 히트스톱과 흔들림.
        private void OnHit(ShotHit hit)
        {
            if (!feelEnabled) return;
            FeelPreset preset = PresetOf(hit.Weapon);
            if (preset == null) return;
            bool controlled = IsControlled(hit.Member);

            if (hit.Target is EnemyUnit enemy)
            {
                enemy.Flash(preset.flashStrength, preset.flashDuration, preset.flashRim, preset.flashColor);
            }

            if (effects != null)
            {
                Color color = hit.WeakPoint ? preset.weakPointSparkColor : preset.sparkColor;
                effects.Spark(hit.Point, squad.ViewCamera.transform, preset.sparkCount, preset.sparkSpeed, preset.sparkSize, color);
            }

            if (feelAudio != null && lastHitSoundFrame[hit.Member] != Time.frameCount)
            {
                lastHitSoundFrame[hit.Member] = Time.frameCount;
                float scale = controlled ? 1f : settings.autoAllyVolume;
                FeelSound sound = hit.Killed ? FeelSound.Kill : hit.WeakPoint ? FeelSound.WeakPoint : FeelSound.Hit;
                feelAudio.Play(sound, preset.hitVolume * scale);
            }

            if (!controlled) return;

            float stop = preset.hitStopOnHit;
            if (hit.WeakPoint) stop = Mathf.Max(stop, preset.hitStopOnWeakPoint);
            if (hit.Killed) stop = Mathf.Max(stop, preset.hitStopOnKill);
            if (preset.scaleByCharge) stop *= hit.Charge01;
            if (hitStop != null) hitStop.Request(stop, preset.hitStopTimeScale);

            if (shake != null) shake.Add(preset.hitShake + (hit.Killed ? preset.killShake : 0f));
        }

        // 버스트: 흔들림과 효과음. 풀 버스트로 넘어가는 3단계는 잠깐 멈춘다.
        private void OnSkillUsed(int member, SkillData skill)
        {
            if (!feelEnabled || !skill.IsBurst) return;
            if (shake != null) shake.Add(settings.burstShake);
            if (feelAudio != null) feelAudio.Play(FeelSound.Burst, 0.7f);
            if (skill.BurstStage >= BurstChain.StageCount && hitStop != null) hitStop.Request(settings.fullBurstHitStop, 0.05f);
        }

        private void OnSlam(BossBrain boss)
        {
            if (!feelEnabled) return;
            if (shake != null) shake.Add(settings.slamShake);
            if (feelAudio != null) feelAudio.Play(FeelSound.Slam, 0.9f);
        }

        private void OnAllyHurt(AllyUnit unit, float damage)
        {
            if (feelEnabled && shake != null) shake.Add(settings.allyHurtShake);
        }
    }
}
