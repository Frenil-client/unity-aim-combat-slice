using System;
using UnityEngine;

namespace AimCombat.UI
{
    /// <summary>RectTransform 의 배치(앵커, 기준점, 위치, 크기) 한 벌.</summary>
    [Serializable]
    public struct RectPlacement
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;

        /// <summary>한 점(앵커와 기준점이 같은 자리)에 붙인 배치. HUD 묶음은 대부분 화면 모서리나 변의 가운데 한 점에 붙는다.</summary>
        public static RectPlacement At(Vector2 anchor, Vector2 position, Vector2 size = default) => new RectPlacement
        {
            anchorMin = anchor,
            anchorMax = anchor,
            pivot = anchor,
            anchoredPosition = position,
            sizeDelta = size,
        };

        public static RectPlacement Capture(RectTransform rect) => new RectPlacement
        {
            anchorMin = rect.anchorMin,
            anchorMax = rect.anchorMax,
            pivot = rect.pivot,
            anchoredPosition = rect.anchoredPosition,
            sizeDelta = rect.sizeDelta,
        };

        public RectPlacement WithSize(Vector2 size)
        {
            RectPlacement copy = this;
            copy.sizeDelta = size;
            return copy;
        }

        public void ApplyTo(RectTransform rect)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
        }
    }

    /// <summary>
    /// 가로형과 세로형에서 다른 자리에 놓이는 UI 묶음. 갈래가 바뀌면 저장해 둔 배치 한 벌을 통째로 입힌다.
    /// 크기가 콘텐츠를 따르는 묶음(ContentSizeFitter)이면 크기는 그쪽이 다시 정하므로 위치와 앵커만 의미가 있다.
    /// 레이아웃 그룹의 자식에는 쓰지 않는다(자리를 부모가 정한다). 그때는 ResponsiveLayoutSize 로 선호 크기만 바꾼다.
    /// 편집 화면에서 배치를 고치려면 RectTransform 을 옮긴 뒤 컴포넌트 메뉴의 "지금 배치를 ...로 저장"을 누른다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResponsiveRect : ScreenShapeElement
    {
        [SerializeField] private RectPlacement landscape;
        [SerializeField] private RectPlacement portrait;

        public void Configure(RectPlacement landscapePlacement, RectPlacement portraitPlacement)
        {
            landscape = landscapePlacement;
            portrait = portraitPlacement;
        }

        protected override void Apply(ScreenShape shape)
        {
            (shape == ScreenShape.Portrait ? portrait : landscape).ApplyTo((RectTransform)transform);
        }

#if UNITY_EDITOR
        [ContextMenu("지금 배치를 가로형으로 저장")]
        private void SaveLandscape() => Save(ref landscape);

        [ContextMenu("지금 배치를 세로형으로 저장")]
        private void SavePortrait() => Save(ref portrait);

        [ContextMenu("가로형 배치 보기")]
        private void PreviewLandscape() => Preview(landscape);

        [ContextMenu("세로형 배치 보기")]
        private void PreviewPortrait() => Preview(portrait);

        private void Save(ref RectPlacement placement)
        {
            UnityEditor.Undo.RecordObject(this, "배치 저장");
            placement = RectPlacement.Capture((RectTransform)transform);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void Preview(RectPlacement placement)
        {
            UnityEditor.Undo.RecordObject(transform, "배치 보기");
            placement.ApplyTo((RectTransform)transform);
        }
#endif
    }
}
