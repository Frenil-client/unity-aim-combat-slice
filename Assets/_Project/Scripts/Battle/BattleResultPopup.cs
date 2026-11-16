using TMPro;
using UISystem;
using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 전투 결과(승리, 패배) 팝업. 전투 화면(BattleHud)이 전투가 끝날 때 UIManager 로 연다.
    /// 다시 시작은 전투 감독이 방아쇠나 R 키로 받는다(씬을 다시 불러오면 이 팝업도 씬과 함께 정리된다).
    /// 공유 Dim 을 눌러도, 뒤로가기로도 닫히지 않는다.
    /// </summary>
    public sealed class BattleResultPopup : UIPopup
    {
        [SerializeField] private TMP_Text label;

        protected override UIViewOptions DefaultOptions => new() { BlockClose = true };

        public void Show(bool victory)
        {
            label.text = (victory ? "승리" : "패배") + "\n<size=36>누르거나 R 키로 다시 시작</size>";
        }
    }
}
