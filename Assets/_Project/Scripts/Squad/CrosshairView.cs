using AimCombat.Battle;
using AimCombat.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Squad
{
    /// <summary>조준점이 한 프레임에 보여 줄 내용. SquadController 가 조작 중인 아군의 무기에서 읽어 넘긴다.</summary>
    public struct CrosshairState
    {
        public WeaponKind Kind;
        /// <summary>퍼짐을 조준점 크기로 바꾼 반지름(픽셀, 1080 기준).</summary>
        public float SpreadRadius;
        public bool Charging;
        public float Charge01;
        public int Ammo;
        public int MagazineSize;
        public bool Reloading;
        /// <summary>재장전 진행 0~1. 산탄형은 지금 넣는 단계의 진행이다.</summary>
        public float Reload01;
    }

    /// <summary>
    /// 조준점과 그 옆의 잔탄 표시(SPEC 2절 조준점 표시). 무기 종류마다 모양이 다르다.
    /// - 연사형: 가운데 점과 팔 4개. 쏠수록 퍼짐만큼 팔이 벌어진다.
    /// - 차지형: 가는 원과 바깥 눈금 4개, 가운데 점. 충전하면 원 둘레에 충전 호가 차오르고 다 차면 색이 바뀐다. 아래에 충전 %.
    /// - 산탄형: 탄이 퍼지는 범위만큼 큰 원과 가운데 점.
    /// 잔탄은 조준점 왼쪽 아래에 세 자리 숫자로 띄운다. 탄이 적으면 붉어지고, 재장전 중에는 진행 막대를 함께 보인다.
    /// 그림은 리소스가 준비되기 전이라 실행할 때 만든다(임시). 붙어 있는 오브젝트의 자식을 비우고 새로 만든다.
    /// </summary>
    public sealed class CrosshairView : MonoBehaviour
    {
        [Header("모양")]
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private Color outlineColor = new Color(0f, 0f, 0f, 0.7f);
        [Tooltip("차지형 원의 반지름(픽셀, 1080 기준).")]
        [SerializeField] private float chargeRadius = 34f;
        [SerializeField] private Color chargeColor = new Color(0.75f, 0.45f, 1f);
        [SerializeField] private Color chargeFullColor = new Color(1f, 0.92f, 0.45f);

        [Header("잔탄")]
        [Tooltip("이 비율 이하로 탄이 남으면 숫자가 붉어진다.")]
        [SerializeField] private float lowAmmoRatio = 0.25f;
        [SerializeField] private Color ammoColor = Color.white;
        [SerializeField] private Color lowAmmoColor = new Color(1f, 0.3f, 0.25f);
        [SerializeField] private Color labelColor = new Color(0.8f, 0.85f, 0.9f, 0.9f);
        [SerializeField] private Color reloadColor = new Color(1f, 0.8f, 0.3f);

        private static readonly Vector2[] Directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        private GameObject autoRoot;
        private readonly RectTransform[] autoArms = new RectTransform[4];

        private GameObject chargeRoot;
        private Image chargeFill;
        private Text chargeText;

        private GameObject shotgunRoot;
        private RectTransform shotgunRing;

        private RectTransform ammoRoot;
        private Text ammoLabel;
        private Text ammoNumber;
        private GameObject reloadBar;
        private RectTransform reloadFill;

        private int shownAmmo = -1;
        private bool built;

        private void Awake()
        {
            if (!built) Build();
        }

        public void Show(in CrosshairState state)
        {
            if (!built) Build();

            SetActive(autoRoot, state.Kind == WeaponKind.Auto);
            SetActive(chargeRoot, state.Kind == WeaponKind.Charge);
            SetActive(shotgunRoot, state.Kind == WeaponKind.Shotgun);

            float radius;
            switch (state.Kind)
            {
                case WeaponKind.Charge:
                    radius = chargeRadius;
                    bool full = state.Charging && state.Charge01 >= 1f;
                    SetActive(chargeFill.gameObject, state.Charging);
                    SetActive(chargeText.gameObject, state.Charging);
                    if (state.Charging)
                    {
                        chargeFill.fillAmount = state.Charge01;
                        chargeFill.color = full ? chargeFullColor : chargeColor;
                        chargeText.text = Mathf.FloorToInt(state.Charge01 * 100f) + "%";
                        chargeText.color = full ? chargeFullColor : chargeColor;
                        // 다 차면 살짝 맥동해 지금 쏘면 된다는 것을 알린다.
                        float pulse = full ? 1f + 0.06f * Mathf.PingPong(Time.unscaledTime * 6f, 1f) : 1f;
                        chargeFill.rectTransform.localScale = new Vector3(pulse, pulse, 1f);
                    }
                    break;

                case WeaponKind.Shotgun:
                    radius = Mathf.Max(18f, state.SpreadRadius);
                    shotgunRing.sizeDelta = new Vector2(radius * 2f, radius * 2f);
                    break;

                default:
                    radius = state.SpreadRadius;
                    for (int i = 0; i < 4; i++) autoArms[i].anchoredPosition = Directions[i] * radius;
                    break;
            }

            ShowAmmo(state, radius);
        }

        // 잔탄은 조준점 왼쪽 아래에 붙인다. 조준점이 커지면(퍼짐, 산탄 원) 그만큼 밀려나 겹치지 않는다.
        private void ShowAmmo(in CrosshairState state, float radius)
        {
            ammoRoot.anchoredPosition = new Vector2(-(radius + 30f), -(radius * 0.6f + 22f));

            if (state.Ammo != shownAmmo)
            {
                shownAmmo = state.Ammo;
                ammoNumber.text = state.Ammo.ToString("000");
            }
            bool low = state.MagazineSize > 0 && state.Ammo <= state.MagazineSize * lowAmmoRatio;
            ammoNumber.color = low ? lowAmmoColor : ammoColor;

            ammoLabel.text = state.Reloading ? "RELOAD" : "AMMO";
            ammoLabel.color = state.Reloading ? reloadColor : labelColor;
            SetActive(reloadBar, state.Reloading);
            if (state.Reloading) reloadFill.localScale = new Vector3(Mathf.Clamp01(state.Reload01), 1f, 1f);
        }

        private void Build()
        {
            built = true;
            // 씬에 남아 있는 예전 조준점 조각(팔, 충전 막대)을 치운다.
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);

            Sprite dot = RingSprite.Get(32, 0f);
            Sprite thinRing = RingSprite.Get(256, 0.93f);
            Sprite thickRing = RingSprite.Get(256, 0.86f);

            // 연사형: 점과 팔 4개.
            autoRoot = NewRect("Auto", transform, Vector2.zero).gameObject;
            AddImage(NewRect("Dot", autoRoot.transform, new Vector2(7f, 7f)), dot, lineColor, true);
            string[] names = { "Top", "Bottom", "Left", "Right" };
            for (int i = 0; i < 4; i++)
            {
                Vector2 size = i < 2 ? new Vector2(4f, 16f) : new Vector2(16f, 4f);
                autoArms[i] = NewRect(names[i], autoRoot.transform, size);
                AddImage(autoArms[i], null, lineColor, true);
            }

            // 차지형: 가는 원, 바깥 눈금 4개, 가운데 점, 충전 호와 충전 %.
            chargeRoot = NewRect("Charge", transform, Vector2.zero).gameObject;
            float diameter = chargeRadius * 2f;
            RectTransform fill = NewRect("ChargeFill", chargeRoot.transform, new Vector2(diameter + 22f, diameter + 22f));
            chargeFill = AddImage(fill, thickRing, chargeColor, false);
            chargeFill.type = Image.Type.Filled;
            chargeFill.fillMethod = Image.FillMethod.Radial360;
            chargeFill.fillOrigin = (int)Image.Origin360.Top;
            chargeFill.fillClockwise = true;
            AddImage(NewRect("Ring", chargeRoot.transform, new Vector2(diameter, diameter)), thinRing, lineColor, true);
            AddImage(NewRect("Dot", chargeRoot.transform, new Vector2(5f, 5f)), dot, lineColor, true);
            for (int i = 0; i < 4; i++)
            {
                Vector2 size = i < 2 ? new Vector2(3f, 12f) : new Vector2(12f, 3f);
                RectTransform tick = NewRect("Tick" + names[i], chargeRoot.transform, size);
                tick.anchoredPosition = Directions[i] * (chargeRadius - 8f);
                AddImage(tick, null, lineColor, true);
            }
            chargeText = AddText("ChargeText", chargeRoot.transform, 24, TextAnchor.MiddleCenter);
            chargeText.rectTransform.anchoredPosition = new Vector2(0f, -(chargeRadius + 34f));

            // 산탄형: 탄이 퍼지는 범위의 원과 가운데 점.
            shotgunRoot = NewRect("Shotgun", transform, Vector2.zero).gameObject;
            shotgunRing = NewRect("Ring", shotgunRoot.transform, new Vector2(80f, 80f));
            AddImage(shotgunRing, thinRing, lineColor, true);
            AddImage(NewRect("Dot", shotgunRoot.transform, new Vector2(6f, 6f)), dot, lineColor, true);

            // 잔탄: 작은 이름표, 세 자리 숫자, 재장전 진행 막대. 오른쪽 끝을 조준점 쪽에 맞춘다.
            ammoRoot = NewRect("Ammo", transform, Vector2.zero);
            ammoLabel = AddText("Label", ammoRoot, 16, TextAnchor.MiddleRight);
            ammoLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
            ammoLabel.rectTransform.anchoredPosition = new Vector2(0f, 20f);
            ammoNumber = AddText("Number", ammoRoot, 34, TextAnchor.MiddleRight);
            ammoNumber.rectTransform.pivot = new Vector2(1f, 0.5f);
            ammoNumber.rectTransform.anchoredPosition = new Vector2(0f, -6f);

            RectTransform bar = NewRect("ReloadBar", ammoRoot, new Vector2(72f, 6f));
            bar.pivot = new Vector2(1f, 0.5f);
            bar.anchoredPosition = new Vector2(0f, -30f);
            AddImage(bar, null, new Color(0f, 0f, 0f, 0.7f), false);
            reloadFill = NewRect("Fill", bar, Vector2.zero);
            reloadFill.anchorMin = Vector2.zero;
            reloadFill.anchorMax = Vector2.one;
            reloadFill.offsetMin = reloadFill.offsetMax = Vector2.zero;
            reloadFill.pivot = new Vector2(0f, 0.5f);
            AddImage(reloadFill, null, reloadColor, false);
            reloadBar = bar.gameObject;

            autoRoot.SetActive(false);
            chargeRoot.SetActive(false);
            shotgunRoot.SetActive(false);
            reloadBar.SetActive(false);
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go.activeSelf != active) go.SetActive(active);
        }

        private static RectTransform NewRect(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = size;
            return rect;
        }

        // 밝은 배경에서도 읽히게 선에는 어두운 외곽선을 두른다.
        private Image AddImage(RectTransform rect, Sprite sprite, Color color, bool outlined)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            if (outlined) AddOutline(rect.gameObject, 1.5f);
            return image;
        }

        private Text AddText(string name, Transform parent, int size, TextAnchor anchor)
        {
            RectTransform rect = NewRect(name, parent, new Vector2(160f, 40f));
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            AddOutline(rect.gameObject, 2f);
            return text;
        }

        private void AddOutline(GameObject go, float distance)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(distance, -distance);
        }
    }
}
