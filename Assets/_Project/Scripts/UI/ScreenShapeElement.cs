using UISystem;

namespace AimCombat.UI
{
    /// <summary>
    /// 화면 배치 갈래(가로형, 세로형)가 바뀔 때마다 자기 모습을 바꾸는 부품의 바탕.
    /// 켜질 때 한 번, 그 뒤로는 갈래가 바뀐 프레임에만 Apply 를 부른다(휴대폰 회전, 창 크기 변경).
    /// 실행 중에만 움직인다. 편집 화면에서는 저작한 모습(가로형)을 그대로 둬 씬과 프리팹을 더럽히지 않는다.
    /// </summary>
    public abstract class ScreenShapeElement : UIElement
    {
        private ScreenShape applied;
        private bool hasApplied;

        protected virtual void OnEnable() => Refresh(true);

        protected virtual void Update() => Refresh(false);

        private void Refresh(bool force)
        {
            ScreenShape shape = ScreenLayout.Current;
            if (!force && hasApplied && applied == shape) return;
            applied = shape;
            hasApplied = true;
            Apply(shape);
        }

        protected abstract void Apply(ScreenShape shape);
    }
}
