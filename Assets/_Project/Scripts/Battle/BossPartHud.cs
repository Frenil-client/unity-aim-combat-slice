using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 보스 부위와 저지 표적 표시(SPEC 3.4). 화면 좌표 UI 라서 부위 위치를 매 프레임 화면에 투영해 따라간다.
    /// - 살아 있는 팔 위에 이름과 체력 막대를 띄운다. 손상 단계면 막대 색이 바뀐다.
    /// - 저지 표적이 뜬 부위에는 원을 띄운다. 밝은 호가 남은 저지 게이지이고, 깎인 만큼 붉은 바탕이 드러난다. 예고가 끝나갈수록 빠르게 맥동한다.
    ///   예고 중에는 보스 몸이 붉게 맥동하므로 남은 게이지는 붉은색이 아닌 밝은 노란색으로 둔다.
    /// - 강타 예고 중에는 보스 머리 위에 경고와 남은 시간을 띄운다.
    /// 원 그림은 리소스가 준비되기 전이라 실행할 때 만든 것을 쓴다(RingSprite, 임시). 표시 오브젝트는 보스가 처음 나올 때 부위 수만큼 만든다.
    /// </summary>
    public sealed class BossPartHud : MonoBehaviour
    {
        private sealed class View
        {
            public RectTransform root;
            public RectTransform ring;
            public Image ringBack;
            public Image ringFill;
            public Text label;
            public GameObject bar;
            public Image barFill;
        }

        [SerializeField] private BattleDirector director;
        [SerializeField] private Camera viewCamera;
        [Tooltip("글자 원본. 부위 이름과 경고 글자를 같은 설정으로 만들어 쓴다.")]
        [SerializeField] private Text template;

        [Header("모양")]
        [Tooltip("부위 이름과 체력 막대를 부위 지점에서 띄우는 높이(픽셀, 1080 기준).")]
        [SerializeField] private float labelOffset = 70f;
        [SerializeField] private Vector2 barSize = new Vector2(130f, 10f);
        [Tooltip("저지 표적 원의 가장 작은 지름(픽셀, 1080 기준). 보스가 멀어도 이보다 작아지지 않는다.")]
        [SerializeField] private float minRingSize = 90f;
        [Tooltip("경고 글자를 보스 머리 위 지점에서 더 띄우는 높이(픽셀, 1080 기준).")]
        [SerializeField] private float warningOffset = 80f;
        [SerializeField] private Color ringBackColor = new Color(0.75f, 0.05f, 0.08f, 0.9f);
        [SerializeField] private Color ringFillColor = new Color(1f, 0.85f, 0.25f, 1f);
        [SerializeField] private Color healthColor = new Color(0.95f, 0.95f, 0.98f, 1f);
        [SerializeField] private Color damagedColor = new Color(1f, 0.55f, 0.2f, 1f);
        [SerializeField] private Color markedLabelColor = new Color(1f, 0.4f, 0.35f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, 0.25f, 0.25f, 1f);

        private readonly List<View> views = new List<View>();
        private BossBrain shownBoss;
        private Text warning;
        private Sprite ringSprite;
        private float canvasScale = 1f;

        private void Awake()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            template.gameObject.SetActive(false);
            ringSprite = RingSprite.Get(128, 0.8f);

            warning = Instantiate(template, transform);
            warning.name = "Warning";
            warning.fontSize = 44;
            warning.color = warningColor;
            warning.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            EnemyUnit unit = director != null ? director.Boss : null;
            BossBrain boss = unit != null && !unit.IsDead ? unit.Boss : null;
            if (boss == null)
            {
                if (shownBoss != null) HideAll();
                shownBoss = null;
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            canvasScale = canvas != null ? canvas.scaleFactor : 1f;

            IReadOnlyList<BossPart> parts = boss.Parts;
            if (boss != shownBoss)
            {
                shownBoss = boss;
                HideAll();
                while (views.Count < parts.Count) views.Add(CreateView(views.Count));
            }

            bool telegraph = boss.Phase == BossPhase.SlamTelegraph;
            float pulse = telegraph ? Mathf.PingPong(Time.unscaledTime * (2f + 6f * boss.TelegraphProgress), 1f) : 0f;
            for (int i = 0; i < views.Count; i++)
            {
                if (i < parts.Count) Refresh(views[i], parts[i], pulse);
                else views[i].root.gameObject.SetActive(false);
            }

            RefreshWarning(boss, telegraph, pulse);
        }

        private void Refresh(View view, BossPart part, float pulse)
        {
            Vector3 screen = viewCamera.WorldToScreenPoint(part.MarkPoint);
            bool showBar = part.Destructible && !part.IsDestroyed;
            bool show = screen.z > 0f && (showBar || part.Marked);
            if (view.root.gameObject.activeSelf != show) view.root.gameObject.SetActive(show);
            if (!show) return;

            view.root.position = (Vector2)screen;

            // 저지 표적: 부위 크기에 맞춘 원. 남은 저지 게이지만큼 밝은 호가 남는다.
            if (view.ring.gameObject.activeSelf != part.Marked) view.ring.gameObject.SetActive(part.Marked);
            if (part.Marked)
            {
                Vector3 edge = viewCamera.WorldToScreenPoint(part.MarkPoint + viewCamera.transform.right * part.MarkRadius);
                float diameter = Mathf.Max(minRingSize, 2f * Mathf.Abs(edge.x - screen.x) / canvasScale);
                view.ring.sizeDelta = new Vector2(diameter, diameter);
                float scale = 1f + 0.08f * pulse;
                view.ring.localScale = new Vector3(scale, scale, 1f);
                view.ringFill.fillAmount = part.MarkRatio;
            }

            // 이름은 팔에만 띄운다. 본체의 저지 지점은 서로 가까워 이름이 겹치므로 원만 보인다.
            if (view.label.gameObject.activeSelf != part.Destructible) view.label.gameObject.SetActive(part.Destructible);
            view.label.text = part.DisplayName;
            view.label.color = part.Marked ? markedLabelColor : Color.white;
            if (view.bar.activeSelf != showBar) view.bar.SetActive(showBar);
            if (showBar)
            {
                view.barFill.rectTransform.localScale = new Vector3(part.HealthRatio, 1f, 1f);
                view.barFill.color = part.Stage == BossPartStage.Damaged ? damagedColor : healthColor;
            }
        }

        private void RefreshWarning(BossBrain boss, bool telegraph, float pulse)
        {
            Vector3 screen = viewCamera.WorldToScreenPoint(boss.WarningPoint);
            bool show = telegraph && screen.z > 0f;
            if (warning.gameObject.activeSelf != show) warning.gameObject.SetActive(show);
            if (!show) return;

            // 부위 이름과 겹치지 않게 머리 위에서 더 띄운다.
            warning.rectTransform.position = (Vector2)screen + Vector2.up * (warningOffset * canvasScale);
            warning.text = "! WARNING !\n<size=58>" + boss.TelegraphTimeLeft.ToString("0.0") + "</size>";
            Color color = warningColor;
            color.a = 0.6f + 0.4f * pulse;
            warning.color = color;
        }

        private void HideAll()
        {
            foreach (View view in views) view.root.gameObject.SetActive(false);
            if (warning != null) warning.gameObject.SetActive(false);
        }

        private View CreateView(int index)
        {
            var view = new View();
            view.root = NewRect("Part_" + index, transform, Vector2.zero);

            view.ring = NewRect("Ring", view.root, new Vector2(minRingSize, minRingSize));
            view.ringBack = AddImage(view.ring, ringBackColor);
            view.ringBack.sprite = ringSprite;
            RectTransform fill = NewRect("Fill", view.ring, Vector2.zero);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            view.ringFill = AddImage(fill, ringFillColor);
            view.ringFill.sprite = ringSprite;
            view.ringFill.type = Image.Type.Filled;
            view.ringFill.fillMethod = Image.FillMethod.Radial360;
            view.ringFill.fillOrigin = (int)Image.Origin360.Top;
            view.ringFill.fillClockwise = false;

            view.label = Instantiate(template, view.root);
            view.label.name = "Label";
            view.label.gameObject.SetActive(true);
            view.label.rectTransform.anchoredPosition = new Vector2(0f, labelOffset + barSize.y + 16f);

            RectTransform bar = NewRect("HealthBar", view.root, barSize);
            bar.anchoredPosition = new Vector2(0f, labelOffset);
            AddImage(bar, new Color(0f, 0f, 0f, 0.7f));
            RectTransform barFill = NewRect("Fill", bar, Vector2.zero);
            barFill.anchorMin = Vector2.zero;
            barFill.anchorMax = Vector2.one;
            barFill.offsetMin = barFill.offsetMax = Vector2.zero;
            barFill.pivot = new Vector2(0f, 0.5f);
            view.barFill = AddImage(barFill, healthColor);
            view.bar = bar.gameObject;

            view.root.gameObject.SetActive(false);
            return view;
        }

        private static RectTransform NewRect(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = size;
            return rect;
        }

        private static Image AddImage(RectTransform rect, Color color)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
    }
}
