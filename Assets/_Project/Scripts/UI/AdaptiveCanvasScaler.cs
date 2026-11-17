using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.UI
{
    /// <summary>
    /// 화면 방향을 따라 기준 해상도를 돌려 쓰는 CanvasScaler.
    /// 설정값(1920x1080, 높이 기준)은 그대로 두고, 세로 화면에서는 기준을 1080x1920 으로 돌려 짧은 변(폭)에 맞춘다.
    /// 그래서 가로든 세로든 캔버스의 짧은 변이 늘 1080 이고, 글자와 버튼의 손가락 크기가 방향에 따라 바뀌지 않는다.
    /// - 그냥 높이 기준이면 세로 휴대폰(1080x2400)에서 캔버스 폭이 486 이 되어 HUD 가 화면 밖으로 넘친다.
    /// - UI 시스템은 팝업을 열 때마다 공통 설정값을 CanvasScaler 에 다시 쓰고, 화면(UIScreen)은 값이 다르면 경고한다.
    ///   설정값을 바꾸지 않고 계산만 바꾸므로 둘 다 그대로 맞는다. 이 컴포넌트도 CanvasScaler 라 UI 시스템이 같은 것으로 본다.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Layout/Adaptive Canvas Scaler")]
    public sealed class AdaptiveCanvasScaler : CanvasScaler
    {
        private const float LogBase = 2f;
        private Canvas canvas;

        protected override void HandleScaleWithScreenSize()
        {
            if (screenMatchMode != ScreenMatchMode.MatchWidthOrHeight)
            {
                base.HandleScaleWithScreenSize();
                return;
            }

            if (canvas == null) canvas = GetComponent<Canvas>();
            Vector2 screen = canvas.renderingDisplaySize;
            int displayIndex = canvas.targetDisplay;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                Display display = Display.displays[displayIndex];
                screen = new Vector2(display.renderingWidth, display.renderingHeight);
            }
            if (screen.x <= 0f || screen.y <= 0f) return;

            // 기준과 화면의 방향이 다르면 기준을 돌린다. 축이 바뀌므로 맞출 축(match)도 뒤집는다.
            Vector2 reference = referenceResolution;
            float match = matchWidthOrHeight;
            if ((reference.x >= reference.y) != (screen.x >= screen.y))
            {
                reference = new Vector2(reference.y, reference.x);
                match = 1f - match;
            }

            float logWidth = Mathf.Log(screen.x / reference.x, LogBase);
            float logHeight = Mathf.Log(screen.y / reference.y, LogBase);
            SetScaleFactor(Mathf.Pow(LogBase, Mathf.Lerp(logWidth, logHeight, match)));
            SetReferencePixelsPerUnit(referencePixelsPerUnit);
        }
    }
}
