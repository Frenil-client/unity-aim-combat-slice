using UnityEngine;

namespace AimCombat.Feel
{
    /// <summary>
    /// 연출용 카메라 포커스. 지금 카메라 자리(전투 시점)와 대상을 비추는 자리 사이를 Weight(0~1)로 섞는다.
    /// - Begin 때의 카메라 자리를 기억해 두고 매 프레임 거기서부터 섞으므로, 끝나면(End) 정확히 원래 자리로 돌아온다.
    ///   그동안 카메라를 옮기는 쪽(분대의 옆 따라가기)은 꺼 둔다. 연출 쪽(BossIntro)이 그렇게 한다.
    /// - 대상 앞 distance 거리, 대상보다 낮은 곳에서 올려다본다(보스가 크게 보이게).
    /// - 화각은 지금 화각(화면 비 맞춤 포함)에 fovScale 을 곱한 쪽으로 섞는다. 흔들림(CameraShake)은 그리는 동안만 더해지므로 함께 쓸 수 있다.
    /// 다른 LateUpdate 가 카메라를 다 옮긴 뒤에 놓도록 늦게 돈다.
    /// </summary>
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFocus : MonoBehaviour
    {
        [Tooltip("대상에서 카메라까지의 거리(m).")]
        [SerializeField] private float distance = 14f;
        [Tooltip("대상 높이에서 카메라를 얼마나 내리는지(m). 음수면 올려다본다.")]
        [SerializeField] private float heightOffset = -1.5f;
        [Tooltip("포커스 때의 화각 배율. 1보다 작으면 조금 당겨 본다.")]
        [SerializeField] private float fovScale = 0.9f;

        private Camera viewCamera;
        private Vector3 basePosition;
        private Quaternion baseRotation;
        private float baseFov;
        private Vector3 target;

        /// <summary>포커스 비율. 0이면 원래 자리, 1이면 대상을 비추는 자리.</summary>
        public float Weight { get; set; }

        public bool Active { get; private set; }

        private void Awake() => viewCamera = GetComponent<Camera>();

        public void Begin(Vector3 point)
        {
            if (!Active)
            {
                basePosition = transform.position;
                baseRotation = transform.rotation;
                baseFov = viewCamera.fieldOfView;
            }
            target = point;
            Weight = 0f;
            Active = true;
        }

        public void End()
        {
            if (!Active) return;
            Active = false;
            Weight = 0f;
            transform.SetPositionAndRotation(basePosition, baseRotation);
            viewCamera.fieldOfView = baseFov;
        }

        private void LateUpdate()
        {
            if (!Active) return;

            Vector3 toTarget = target - basePosition;
            toTarget.y = 0f;
            Vector3 forward = toTarget.sqrMagnitude > 0.01f ? toTarget.normalized : Vector3.forward;
            Vector3 focusPosition = target - forward * distance;
            focusPosition.y = target.y + heightOffset;
            Quaternion focusRotation = Quaternion.LookRotation(target - focusPosition);

            float w = Mathf.Clamp01(Weight);
            w = w * w * (3f - 2f * w);   // 출발과 도착을 부드럽게
            transform.SetPositionAndRotation(Vector3.Lerp(basePosition, focusPosition, w), Quaternion.Slerp(baseRotation, focusRotation, w));
            viewCamera.fieldOfView = Mathf.Lerp(baseFov, baseFov * fovScale, w);
        }
    }
}
