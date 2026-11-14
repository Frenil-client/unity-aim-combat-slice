using AimCombat.Weapons;
using UnityEngine;

namespace AimCombat.Feel
{
    public enum FeelSound
    {
        Hit,
        WeakPoint,
        Kill,
        Burst,
        Slam,
    }

    /// <summary>
    /// 타격감 사운드(SPEC 4절 사운드 타이밍). 사운드 파일이 아직 없어 실행할 때 짧은 소리를 합성해 쓴다(임시).
    /// 나중에 FeelPreset.fireClip 이나 여기의 클립 칸에 실제 파일을 넣으면 그쪽을 쓴다.
    /// - 소리는 2D(거리 감쇠 없음)로 낸다. 고정 시점이라 위치감보다 타이밍이 중요하다.
    /// - 소스를 여러 개 돌려 써서 연사가 앞 소리를 끊지 않게 한다. 높이(pitch)는 소스마다 따로 준다.
    /// </summary>
    public sealed class FeelAudio : MonoBehaviour
    {
        [SerializeField] private int voices = 16;
        [Header("직접 넣는 클립(비어 있으면 합성음)")]
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip weakPointClip;
        [SerializeField] private AudioClip killClip;
        [SerializeField] private AudioClip burstClip;
        [SerializeField] private AudioClip slamClip;

        private AudioSource[] sources;
        private int next;
        private AudioClip autoFire, chargeFire, shotgunFire;

        public float MasterVolume { get; set; } = 1f;

        private void Awake()
        {
            sources = new AudioSource[Mathf.Max(1, voices)];
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                sources[i] = source;
            }

            autoFire = SoundSynth.Gunshot("AutoFire", 0.07f, 0.55f, 2600f, 11);
            chargeFire = SoundSynth.Gunshot("ChargeFire", 0.32f, 0.35f, 900f, 12);
            shotgunFire = SoundSynth.Gunshot("ShotgunFire", 0.2f, 0.75f, 1500f, 13);
            if (hitClip == null) hitClip = SoundSynth.Click("Hit", 0.035f, 3200f, 14);
            if (weakPointClip == null) weakPointClip = SoundSynth.Ping("WeakPoint", 0.12f, 1500f, 1300f);
            if (killClip == null) killClip = SoundSynth.Ping("Kill", 0.18f, 900f, 300f);
            if (burstClip == null) burstClip = SoundSynth.Ping("Burst", 0.45f, 300f, 1200f);
            if (slamClip == null) slamClip = SoundSynth.Gunshot("Slam", 0.5f, 0.2f, 400f, 15);
        }

        public AudioClip DefaultFireClip(WeaponKind kind)
        {
            switch (kind)
            {
                case WeaponKind.Charge: return chargeFire;
                case WeaponKind.Shotgun: return shotgunFire;
                default: return autoFire;
            }
        }

        public void Play(FeelSound sound, float volume, float pitch = 1f)
        {
            Play(ClipOf(sound), volume, pitch);
        }

        public void Play(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || volume <= 0f || sources == null) return;
            AudioSource source = sources[next];
            next = (next + 1) % sources.Length;
            source.pitch = pitch;
            source.PlayOneShot(clip, Mathf.Clamp01(volume * MasterVolume));
        }

        private AudioClip ClipOf(FeelSound sound)
        {
            switch (sound)
            {
                case FeelSound.WeakPoint: return weakPointClip;
                case FeelSound.Kill: return killClip;
                case FeelSound.Burst: return burstClip;
                case FeelSound.Slam: return slamClip;
                default: return hitClip;
            }
        }
    }

    /// <summary>임시 합성음. 잡음 폭발(총성, 충격), 짧은 딸깍(명중), 음높이가 미끄러지는 사인파(약점, 처치, 버스트).</summary>
    public static class SoundSynth
    {
        private const int Rate = 44100;

        // 잡음을 한 번 거른(저역 통과) 폭발음. noiseMix 가 클수록 거칠고, cutoff 가 낮을수록 둔탁하다. 아래에 짧은 저음 펀치를 더한다.
        public static AudioClip Gunshot(string name, float length, float noiseMix, float cutoff, int seed)
        {
            int count = Mathf.CeilToInt(length * Rate);
            var data = new float[count];
            var random = new System.Random(seed);
            float alpha = Mathf.Clamp01(2f * Mathf.PI * cutoff / Rate);
            float filtered = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float envelope = Mathf.Exp(-t / (length * 0.25f));
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += alpha * (noise - filtered);
                float punch = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(140f, 50f, t / length) * t) * Mathf.Exp(-t / (length * 0.15f));
                data[i] = Mathf.Clamp((filtered * noiseMix * 2.5f + punch * (1f - noiseMix)) * envelope, -1f, 1f);
            }
            return Make(name, data);
        }

        public static AudioClip Click(string name, float length, float cutoff, int seed)
        {
            int count = Mathf.CeilToInt(length * Rate);
            var data = new float[count];
            var random = new System.Random(seed);
            float alpha = Mathf.Clamp01(2f * Mathf.PI * cutoff / Rate);
            float filtered = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += alpha * (noise - filtered);
                data[i] = filtered * 1.5f * Mathf.Exp(-t / (length * 0.2f));
            }
            return Make(name, data);
        }

        public static AudioClip Ping(string name, float length, float fromHz, float toHz)
        {
            int count = Mathf.CeilToInt(length * Rate);
            var data = new float[count];
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / Rate;
                float hz = Mathf.Lerp(fromHz, toHz, t / length);
                phase += 2f * Mathf.PI * hz / Rate;
                float attack = Mathf.Clamp01(t / 0.005f);
                data[i] = Mathf.Sin(phase) * attack * Mathf.Exp(-t / (length * 0.35f)) * 0.6f;
            }
            return Make(name, data);
        }

        private static AudioClip Make(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
