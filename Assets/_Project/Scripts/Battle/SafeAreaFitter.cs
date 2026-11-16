using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 붙은 RectTransform 을 기기의 안전 영역(노치, 둥근 모서리, 홈 표시줄을 피한 영역)에 맞춘다.
    /// 화면 가장자리에 앵커로 붙은 UI 를 이 아래에 두면 모바일에서 가려지지 않는다. 안전 영역이 화면 전체면 아무 일도 없다.
    /// Unity 에 같은 기능의 기본 컴포넌트가 없어 둔다. 화면 크기나 방향이 바뀔 때만 다시 맞춘다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect applied;
        private Vector2Int screen;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != applied || screen.x != Screen.width || screen.y != Screen.height) Apply();
        }

        private void Apply()
        {
            applied = Screen.safeArea;
            screen = new Vector2Int(Screen.width, Screen.height);
            if (screen.x <= 0 || screen.y <= 0) return;

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(applied.xMin / screen.x, applied.yMin / screen.y);
            rect.anchorMax = new Vector2(applied.xMax / screen.x, applied.yMax / screen.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
