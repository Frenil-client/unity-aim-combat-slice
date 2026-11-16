using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 전투 HUD 의 아군 한 명 열(프리팹 AllyPanel). 아래부터 아군 패널(얼굴, 이름과 탄약, 체력과 엄폐물 막대), 스킬 줄, 버프 줄을 세로로 쌓는다.
    /// 아군 수만큼 같은 프리팹을 놓고, 값은 BattleHud 가 넣는다.
    /// - 패널은 버튼이라 누르면 그 아군을 조작한다. 바탕 색(선택, 전투 불능)은 BattleHud 가 바꾼다.
    /// - 스킬 줄은 액티브 스킬이 있는 세트에서만 켜진다. 꺼지면 레이아웃이 건너뛰어 버프 줄이 내려온다.
    /// </summary>
    public sealed class AllyPanelView : UIElement
    {
        [SerializeField] private Button button;
        [SerializeField] private Image frame;
        [SerializeField] private Image face;
        [Tooltip("얼굴 테두리. 아래 띠가 캐릭터 포인트 색이다.")]
        [SerializeField] private Image faceFrame;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text status;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image coverFill;
        [SerializeField] private GameObject skillRow;
        [SerializeField] private TMP_Text skillLabel;
        [SerializeField] private Image skillFill;

        public Button Button => button;
        public Image Frame => frame;
        public Image Face => face;
        public Image FaceFrame => faceFrame;
        public TMP_Text Label => label;
        public TMP_Text Status => status;
        public Image HealthFill => healthFill;
        public Image CoverFill => coverFill;
        public GameObject SkillRow => skillRow;
        public TMP_Text SkillLabel => skillLabel;
        public Image SkillFill => skillFill;
    }
}
