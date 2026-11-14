using UnityEngine;

namespace AimCombat.Data
{
    /// <summary>무기와 상관없는 타격감 설정(SPEC 4절). 카메라 흔들림의 크기와 회복, 전투 사건(버스트, 강타, 피격)의 흔들림, 음량.</summary>
    [CreateAssetMenu(menuName = "Aim Combat/Feel Settings", fileName = "FeelSettings")]
    public sealed class FeelSettings : ScriptableObject
    {
        [Header("카메라 흔들림")]
        [Tooltip("충격량 1일 때 카메라가 밀리는 최대 거리(유닛).")]
        [Range(0f, 0.5f)] public float maxShakeOffset = 0.18f;
        [Tooltip("충격량 1일 때 카메라가 기우는 최대 각도(도).")]
        [Range(0f, 5f)] public float maxShakeRoll = 1.2f;
        [Tooltip("초당 줄어드는 충격량. 클수록 흔들림이 빨리 멎는다.")]
        [Range(0.1f, 5f)] public float shakeRecovery = 2.2f;
        [Tooltip("흔들림의 빠르기. 클수록 잘게 떨린다.")]
        [Range(1f, 40f)] public float shakeFrequency = 22f;

        [Header("전투 사건")]
        [Tooltip("버스트를 쓸 때 더하는 충격.")]
        [Range(0f, 1f)] public float burstShake = 0.3f;
        [Tooltip("풀 버스트로 넘어가는 3단계 버스트에 멈추는 시간(실제 초).")]
        [Range(0f, 0.5f)] public float fullBurstHitStop = 0.12f;
        [Tooltip("보스 강타가 떨어질 때 더하는 충격.")]
        [Range(0f, 1f)] public float slamShake = 0.6f;
        [Tooltip("아군이 본인 몸으로 피해를 받을 때 더하는 충격.")]
        [Range(0f, 1f)] public float allyHurtShake = 0.12f;

        [Header("사운드")]
        [Range(0f, 1f)] public float masterVolume = 0.8f;
        [Tooltip("자동 사격 아군의 사격음 배율. 조작 중인 아군의 소리가 먼저 들리게 한다.")]
        [Range(0f, 1f)] public float autoAllyVolume = 0.35f;
    }
}
