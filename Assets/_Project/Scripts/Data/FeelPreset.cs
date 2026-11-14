using UnityEngine;

namespace AimCombat.Data
{
    /// <summary>
    /// 무기별 타격감 프리셋(SPEC 4절). 히트스톱, 카메라 흔들림, 피격 플래시, 명중 이펙트, 총구 화염과 탄피, 사운드를 한 에셋에 둔다.
    /// 실행 중 튜닝 패널이 이 값을 바꾸고 저장한다. 값마다 어떤 느낌을 바꾸는지는 툴팁에 적는다.
    /// 히트스톱과 카메라 흔들림은 조작 중인 아군의 사격에만 건다. 자동 사격 아군까지 걸면 화면이 쉬지 않고 멈추고 흔들린다.
    /// </summary>
    [CreateAssetMenu(menuName = "Aim Combat/Feel Preset", fileName = "Feel_")]
    public sealed class FeelPreset : ScriptableObject
    {
        [Header("히트스톱(조작 중인 아군만, 실제 시간 초)")]
        [Tooltip("일반 명중 한 번에 멈추는 시간. 연사형은 0이 아니면 끊겨 보인다.")]
        [Range(0f, 0.2f)] public float hitStopOnHit;
        [Tooltip("약점 명중에 멈추는 시간. 약점을 맞혔다는 손맛을 준다.")]
        [Range(0f, 0.2f)] public float hitStopOnWeakPoint = 0.02f;
        [Tooltip("처치한 한 발에 멈추는 시간. 마무리의 무게를 준다.")]
        [Range(0f, 0.3f)] public float hitStopOnKill = 0.04f;
        [Tooltip("멈추는 동안의 시간 배율. 0에 가까울수록 딱 멈춘 느낌, 크면 슬로모션에 가깝다.")]
        [Range(0f, 1f)] public float hitStopTimeScale = 0.05f;
        [Tooltip("켜면 히트스톱 시간에 충전량을 곱한다(차지형). 덜 충전한 한 발은 가볍게 느껴진다.")]
        public bool scaleByCharge;

        [Header("카메라 흔들림(조작 중인 아군만, 충격량 0~1)")]
        [Tooltip("쏠 때마다 더하는 충격. 반동이 화면까지 전해지는 느낌. 차지형은 충전량을 곱한다.")]
        [Range(0f, 1f)] public float fireShake = 0.04f;
        [Tooltip("명중할 때 더하는 충격.")]
        [Range(0f, 1f)] public float hitShake;
        [Tooltip("처치할 때 더하는 충격.")]
        [Range(0f, 1f)] public float killShake = 0.12f;

        [Header("피격 플래시(적 몸)")]
        [Tooltip("번쩍임 세기. 0이면 번쩍이지 않는다.")]
        [Range(0f, 1f)] public float flashStrength = 0.8f;
        [Tooltip("번쩍임이 사라지는 시간. 길수록 맞은 게 오래 남지만 연사형은 계속 하얗게 보인다.")]
        [Range(0.02f, 0.5f)] public float flashDuration = 0.08f;
        [Tooltip("가장자리 빛(림) 세기. 몸 윤곽이 빛나 맞은 대상이 배경에서 떨어져 보인다.")]
        [Range(0f, 3f)] public float flashRim = 1f;
        public Color flashColor = Color.white;

        [Header("명중 이펙트")]
        [Tooltip("한 발에 튀는 불꽃 수.")]
        [Range(0, 30)] public int sparkCount = 4;
        [Tooltip("불꽃이 튀는 속도(카메라에서 10유닛 떨어진 곳 기준, 멀수록 비례해 빨라져 화면에서는 비슷하게 보인다).")]
        [Range(0f, 20f)] public float sparkSpeed = 6f;
        [Tooltip("불꽃 굵기(카메라에서 10유닛 떨어진 곳 기준, 멀수록 비례해 커져 화면에서는 비슷하게 보인다).")]
        [Range(0.01f, 0.5f)] public float sparkSize = 0.08f;
        public Color sparkColor = new Color(1f, 0.85f, 0.45f);
        [Tooltip("약점 명중의 불꽃 색. 약점을 맞혔는지 색으로 바로 읽히게 한다.")]
        public Color weakPointSparkColor = new Color(1f, 0.45f, 0.15f);

        [Header("총구 화염과 탄피")]
        [Tooltip("총구 화염 크기. 차지형은 충전량에 따라 1배에서 2배.")]
        [Range(0f, 3f)] public float muzzleFlashScale = 1f;
        [Tooltip("총구 화염이 보이는 시간.")]
        [Range(0.01f, 0.2f)] public float muzzleFlashDuration = 0.04f;
        [Tooltip("한 번 쏠 때 튀는 탄피 수.")]
        [Range(0, 5)] public int shellCount = 1;

        [Header("사운드")]
        [Tooltip("사격음 크기. 자동 사격 아군은 FeelSettings 의 배율을 더 곱한다.")]
        [Range(0f, 1f)] public float fireVolume = 0.5f;
        [Tooltip("사격음 높이. 낮을수록 무겁게 들린다.")]
        [Range(0.5f, 2f)] public float firePitch = 1f;
        [Tooltip("쏠 때마다 높이를 흔드는 폭. 연사가 같은 소리 반복으로 들리지 않게 한다.")]
        [Range(0f, 0.3f)] public float pitchJitter = 0.05f;
        [Tooltip("명중음 크기.")]
        [Range(0f, 1f)] public float hitVolume = 0.3f;
        [Tooltip("사격음. 비어 있으면 무기 종류별 임시 합성음을 쓴다.")]
        public AudioClip fireClip;
    }
}
