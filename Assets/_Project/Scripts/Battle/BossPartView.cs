using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 보스 부위 하나의 화면 표시(SPEC 3.4). 씬에 원본 하나를 두고 BossPartHud 가 부위 수만큼 복제해 쓴다. 값은 BossPartHud 가 넣는다.
    /// 구성: 저지 표적 원(바탕, 남은 저지 게이지의 밝은 호, 맥동 애니메이터)과 그 위의 정보(이름, 체력 막대. 세로 레이아웃).
    /// </summary>
    public sealed class BossPartView : MonoBehaviour
    {
        [SerializeField] private RectTransform ring;
        [SerializeField] private Image ringFill;
        [SerializeField] private Animator ringPulse;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject healthBar;
        [SerializeField] private Image healthFill;

        public RectTransform Ring => ring;
        public Image RingFill => ringFill;
        public Animator RingPulse => ringPulse;
        public TMP_Text Label => label;
        public GameObject HealthBar => healthBar;
        public Image HealthFill => healthFill;
        public RectTransform Rect => (RectTransform)transform;
    }
}
