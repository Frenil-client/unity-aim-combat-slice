using UnityEngine;

namespace AimCombat.UI
{
    /// <summary>
    /// 화면 비에 따라 카메라 화각을 맞춘다. Unity 카메라는 세로 화각을 고정하므로, 세로 휴대폰에서는 가로로 보이는 폭이 크게 줄어
    /// 전장 양옆의 적이 화면 밖으로 나간다(16:9 에서 가로 66도가 9:20 에서 19도).
    /// 가로 화각이 minHorizontalFov 아래로 내려가지 않도록 세로 화각을 넓히고, 너무 넓어져 원근이 일그러지지 않게 maxVerticalFov 에서 멈춘다.
    /// 가로 화면(PC, 4:3 포함)에서는 기준 화각 그대로다. 카메라는 지금처럼 고른 아군을 따라 옆으로 움직인다(SquadController).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public sealed class CameraAspectFit : MonoBehaviour
    {
        [Tooltip("가로 화면에서 쓰는 세로 화각(도).")]
        [SerializeField] private float verticalFov = 40f;
        [Tooltip("이보다 좁아지지 않게 지키는 가로 화각(도). 34도면 세로 휴대폰에서도 적이 나오는 거리(약 20m)에서 좌우 6m 남짓이 보인다.")]
        [SerializeField] private float minHorizontalFov = 34f;
        [Tooltip("세로 화각을 넓히는 한계(도).")]
        [SerializeField] private float maxVerticalFov = 68f;

        private Camera viewCamera;
        private float appliedAspect = -1f;

        public void Configure(float baseVerticalFov)
        {
            verticalFov = baseVerticalFov;
            appliedAspect = -1f;
        }

        private void OnEnable()
        {
            viewCamera = GetComponent<Camera>();
            Apply();
        }

        private void LateUpdate()
        {
            if (!Mathf.Approximately(viewCamera.aspect, appliedAspect)) Apply();
        }

        private void Apply()
        {
            appliedAspect = viewCamera.aspect;
            float keepWidth = Camera.HorizontalToVerticalFieldOfView(minHorizontalFov, appliedAspect);
            viewCamera.fieldOfView = Mathf.Clamp(keepWidth, verticalFov, Mathf.Max(verticalFov, maxVerticalFov));
        }
    }
}
