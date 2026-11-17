using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.UI
{
    /// <summary>
    /// 가로형과 세로형에서 쌓는 방향이 다른 레이아웃 그룹. 나머지 설정(여백, 간격, 정렬, 자식 크기 제어)은 가로, 세로 레이아웃 그룹과 같다.
    /// 예: 스킬 세트 선택 팝업의 캐릭터 열은 가로형에서 옆으로, 세로형에서 아래로 쌓인다.
    /// Unity 에는 방향을 바꾸는 기본 그룹이 없고, 한 오브젝트에 레이아웃 그룹을 둘 둘 수도 없어 둔다.
    /// </summary>
    [AddComponentMenu("Layout/Responsive Stack Layout Group")]
    public sealed class ResponsiveStack : HorizontalOrVerticalLayoutGroup
    {
        [Tooltip("가로형에서 위에서 아래로 쌓는지(끄면 왼쪽에서 오른쪽).")]
        [SerializeField] private bool verticalInLandscape;
        [Tooltip("세로형에서 위에서 아래로 쌓는지(끄면 왼쪽에서 오른쪽).")]
        [SerializeField] private bool verticalInPortrait = true;

        private ScreenShape shape;

        public void Configure(bool vertical, bool verticalPortrait)
        {
            verticalInLandscape = vertical;
            verticalInPortrait = verticalPortrait;
        }

        private bool IsVertical => shape == ScreenShape.Portrait ? verticalInPortrait : verticalInLandscape;

        protected override void OnEnable()
        {
            shape = ScreenLayout.Current;
            base.OnEnable();
        }

        // 기반 레이아웃 그룹의 Update 는 에디터에서만 있다(편집 중 자식 크기 감시). 플레이어 빌드에는 없으므로 갈라 둔다.
#if UNITY_EDITOR
        protected override void Update()
        {
            base.Update();
            RefreshShape();
        }
#else
        private void Update() => RefreshShape();
#endif

        private void RefreshShape()
        {
            ScreenShape current = ScreenLayout.Current;
            if (current == shape) return;
            shape = current;
            SetDirty();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, IsVertical);
        }

        public override void CalculateLayoutInputVertical() => CalcAlongAxis(1, IsVertical);

        public override void SetLayoutHorizontal() => SetChildrenAlongAxis(0, IsVertical);

        public override void SetLayoutVertical() => SetChildrenAlongAxis(1, IsVertical);
    }
}
