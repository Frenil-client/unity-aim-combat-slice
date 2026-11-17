using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.UI
{
    /// <summary>
    /// 레이아웃 그룹 안의 칸이 가로형과 세로형에서 다른 선호 크기를 갖게 한다. 같은 오브젝트의 LayoutElement 값을 바꾼다.
    /// 예: 스킬 세트 선택 팝업의 캐릭터 열은 가로형에서 세 열이 나란히 서는 폭(560), 세로형에서 화면 폭을 다 쓰는 폭(1000)이다.
    /// 음수인 축은 건드리지 않는다.
    /// </summary>
    [RequireComponent(typeof(LayoutElement))]
    [DisallowMultipleComponent]
    public sealed class ResponsiveLayoutSize : ScreenShapeElement
    {
        [Tooltip("가로형의 선호 폭과 높이. 음수인 축은 그대로 둔다.")]
        [SerializeField] private Vector2 landscape = new Vector2(-1f, -1f);
        [Tooltip("세로형의 선호 폭과 높이. 음수인 축은 그대로 둔다.")]
        [SerializeField] private Vector2 portrait = new Vector2(-1f, -1f);

        public void Configure(Vector2 landscapeSize, Vector2 portraitSize)
        {
            landscape = landscapeSize;
            portrait = portraitSize;
        }

        protected override void Apply(ScreenShape shape)
        {
            var element = GetComponent<LayoutElement>();
            Vector2 size = shape == ScreenShape.Portrait ? portrait : landscape;
            if (size.x >= 0f) element.preferredWidth = size.x;
            if (size.y >= 0f) element.preferredHeight = size.y;
        }
    }
}
