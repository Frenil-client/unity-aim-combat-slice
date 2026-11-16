using AimCombat.Weapons;
using TMPro;
using UISystem;
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
    /// - 차지형: 가는 원과 눈금 4개, 가운데 점. 충전하면 원 둘레에 충전 호가 차오르고 다 차면 색이 바뀌며 맥동한다. 아래에 충전 %.
    /// - 산탄형: 탄이 퍼지는 범위만큼의 원과 가운데 점.
    /// 잔탄은 조준점 왼쪽 아래에 세 자리 숫자로 띄운다. 탄이 적으면 붉어지고, 재장전 중에는 진행 막대를 함께 보인다.
    /// 모양은 씬에 구성돼 있다. 팔, 원, 잔탄은 모두 조준 범위(reticle)의 가장자리에 앵커로 붙어 있어 여기서는 범위의 크기만 바꾼다.
    /// 잔탄의 이름표, 숫자, 재장전 막대는 세로 레이아웃이라 재장전 막대가 꺼지면 저절로 줄어든다.
    /// </summary>
    public sealed class CrosshairView : UIElement
    {
        private static readonly int PulseId = Animator.StringToHash("Pulse");

        [Header("구성")]
        [Tooltip("조준 범위. 지름이 퍼짐(차지형은 원의 크기)이 되고, 무기별 모양과 잔탄 표시가 이 가장자리에 붙는다.")]
        [SerializeField] private RectTransform reticle;
        [SerializeField] private GameObject autoRoot;
        [SerializeField] private GameObject chargeRoot;
        [SerializeField] private GameObject shotgunRoot;
        [SerializeField] private Image chargeFill;
        [Tooltip("충전 호의 맥동. 다 차면 켠다.")]
        [SerializeField] private Animator chargePulse;
        [SerializeField] private TMP_Text chargeText;
        [SerializeField] private TMP_Text ammoLabel;
        [SerializeField] private TMP_Text ammoNumber;
        [SerializeField] private GameObject reloadBar;
        [SerializeField] private Image reloadFill;

        [Header("모양")]
        [Tooltip("차지형 원의 반지름(픽셀, 1080 기준).")]
        [SerializeField] private float chargeRadius = 34f;
        [Tooltip("산탄형 원의 가장 작은 반지름.")]
        [SerializeField] private float minShotgunRadius = 18f;
        [SerializeField] private Color chargeColor = new Color(0.75f, 0.45f, 1f);
        [SerializeField] private Color chargeFullColor = new Color(1f, 0.92f, 0.45f);

        [Header("잔탄")]
        [Tooltip("이 비율 이하로 탄이 남으면 숫자가 붉어진다.")]
        [SerializeField] private float lowAmmoRatio = 0.25f;
        [SerializeField] private Color ammoColor = Color.white;
        [SerializeField] private Color lowAmmoColor = new Color(1f, 0.3f, 0.25f);
        [SerializeField] private Color labelColor = new Color(0.8f, 0.85f, 0.9f, 0.9f);
        [SerializeField] private Color reloadColor = new Color(1f, 0.8f, 0.3f);

        private int shownAmmo = -1;

        public void Show(in CrosshairState state)
        {
            SetActive(autoRoot, state.Kind == WeaponKind.Auto);
            SetActive(chargeRoot, state.Kind == WeaponKind.Charge);
            SetActive(shotgunRoot, state.Kind == WeaponKind.Shotgun);

            float radius = state.Kind == WeaponKind.Charge ? chargeRadius
                : state.Kind == WeaponKind.Shotgun ? Mathf.Max(minShotgunRadius, state.SpreadRadius)
                : state.SpreadRadius;
            Vector2 size = new Vector2(radius * 2f, radius * 2f);
            if (reticle.sizeDelta != size) reticle.sizeDelta = size;

            if (state.Kind == WeaponKind.Charge) ShowCharge(state);
            ShowAmmo(state);
        }

        private void ShowCharge(in CrosshairState state)
        {
            bool full = state.Charging && state.Charge01 >= 1f;
            SetActive(chargeFill.gameObject, state.Charging);
            SetActive(chargeText.gameObject, state.Charging);
            if (!state.Charging) return;

            Color color = full ? chargeFullColor : chargeColor;
            chargeFill.fillAmount = state.Charge01;
            chargeFill.color = color;
            chargeText.text = Mathf.FloorToInt(state.Charge01 * 100f) + "%";
            chargeText.color = color;
            // 다 차면 맥동해 지금 쏘면 된다는 것을 알린다(애니메이션 클립).
            if (chargePulse != null) chargePulse.SetBool(PulseId, full);
        }

        private void ShowAmmo(in CrosshairState state)
        {
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
            if (state.Reloading) reloadFill.fillAmount = Mathf.Clamp01(state.Reload01);
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go.activeSelf != active) go.SetActive(active);
        }
    }
}
