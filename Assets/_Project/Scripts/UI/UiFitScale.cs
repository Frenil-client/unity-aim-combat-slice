using UISystem;
using UnityEngine;

namespace AimCombat.UI
{
    /// <summary>
    /// 묶음을 기준 크기(가로 화면 1920x1080)로 그리고, 부모 안에 다 들어가도록 같은 비율로 줄인다. 키우지는 않는다.
    /// 화면 전체에 걸쳐 짜인 연출(버스트 컷인 띠와 그림)처럼 자리를 다시 짜기보다 통째로 줄이는 편이 나은 것에 쓴다.
    /// 세로 휴대폰(폭 1080)에서는 0.56배가 되어 띠와 그림이 화면 안에 다 들어온다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UiFitScale : UIElement
    {
        [SerializeField] private Vector2 designSize = new Vector2(1920f, 1080f);

        private Vector2 parentSize;

        private void OnEnable() => Apply();

        private void LateUpdate()
        {
            if (transform.parent is RectTransform parent && parent.rect.size != parentSize) Apply();
        }

        private void Apply()
        {
            var rect = (RectTransform)transform;
            if (!(transform.parent is RectTransform parent)) return;
            parentSize = parent.rect.size;
            if (parentSize.x <= 0f || parentSize.y <= 0f) return;

            float scale = Mathf.Min(1f, parentSize.x / designSize.x, parentSize.y / designSize.y);
            rect.sizeDelta = designSize;
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
