using UnityEngine;

namespace AimCombat.UI
{
    /// <summary>화면 배치 갈래. 가로형은 PC와 가로로 든 휴대폰, 세로형은 세로로 든 휴대폰과 거의 정사각형인 화면이다.</summary>
    public enum ScreenShape
    {
        Landscape,
        Portrait,
    }

    /// <summary>
    /// 지금 화면이 어느 배치 갈래인지 정한다. 안전 영역(노치와 둥근 모서리를 뺀 영역)의 가로세로 비로 가른다.
    /// 경계를 1이 아니라 1.3 으로 둔 것은, 가로형 HUD(왼쪽 아래 아군 열과 오른쪽 아래 버스트 열)가 캔버스 폭 1400 정도부터
    /// 겹치지 않기 때문이다. 펼친 폴더블처럼 정사각형에 가까운 화면은 세로형 배치를 쓴다.
    /// </summary>
    public static class ScreenLayout
    {
        public const float PortraitBelowAspect = 1.3f;

        /// <summary>지금 갈래. 편집 화면(실행 전)에서는 저작 기준인 가로형이다.</summary>
        public static ScreenShape Current
        {
            get
            {
                if (!Application.isPlaying) return ScreenShape.Landscape;
                Rect safe = Screen.safeArea;
                if (safe.width <= 0f || safe.height <= 0f) return ScreenShape.Landscape;
                return safe.width / safe.height < PortraitBelowAspect ? ScreenShape.Portrait : ScreenShape.Landscape;
            }
        }
    }
}
