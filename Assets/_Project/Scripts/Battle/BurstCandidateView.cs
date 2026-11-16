using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 버스트 후보 버튼 한 칸(프리팹 BurstCandidate). 왼쪽에 얼굴, 오른쪽에 번호와 이름, 버스트 스킬.
    /// 후보 수만큼 같은 프리팹을 쌓아 두고, 보일지와 글, 바탕 색(쓸 수 있음, 쿨다운 중)은 BattleHud 가 정한다.
    /// </summary>
    public sealed class BurstCandidateView : UIElement
    {
        [SerializeField] private Button button;
        [SerializeField] private Image back;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image face;

        public Button Button => button;
        public Image Back => back;
        public TMP_Text Label => label;
        public Image Face => face;
    }
}
