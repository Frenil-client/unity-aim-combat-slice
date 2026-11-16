using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 스킬 세트 카드 하나(SPEC 3.2 스킬 세트 선택). 테두리 색으로 고른 상태를 보이고, 카드 전체가 버튼이다.
    /// 높이는 글 양에 맞춰 레이아웃이 정한다.
    /// </summary>
    public sealed class LoadoutCardView : UIElement
    {
        [SerializeField] private Image frame;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button button;

        public Image Frame => frame;
        public TMP_Text Label => text;
        public Button Button => button;
    }
}
