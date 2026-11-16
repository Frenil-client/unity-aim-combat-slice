using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 켜고 끄는 전환 버튼(프리팹 ToggleButton). 자동 전투, 자동 버스트가 같은 프리팹을 쓴다.
    /// 글(이름, ON/OFF, 단축키)과 바탕 색은 BattleHud 가 넣는다.
    /// </summary>
    public sealed class ToggleButtonView : UIElement
    {
        [SerializeField] private Button button;
        [SerializeField] private Image back;
        [SerializeField] private TMP_Text label;

        public Button Button => button;
        public Image Back => back;
        public TMP_Text Label => label;
    }
}
