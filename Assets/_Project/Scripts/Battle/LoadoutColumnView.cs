using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 스킬 세트 선택 화면의 캐릭터 한 열(SPEC 3.2 스킬 세트 선택). 씬에 원본 하나를 두고 캐릭터 수만큼 복제한다.
    /// 구성: 머리(얼굴, 이름과 버스트 단계. 가로 레이아웃)와 카드 목록(세로 레이아웃). 카드도 원본을 세트 수만큼 복제한다.
    /// </summary>
    public sealed class LoadoutColumnView : MonoBehaviour
    {
        [SerializeField] private Image face;
        [SerializeField] private TMP_Text title;
        [SerializeField] private Transform cardParent;
        [SerializeField] private LoadoutCardView cardTemplate;

        public Image Face => face;
        public TMP_Text Title => title;
        public Transform CardParent => cardParent;
        public LoadoutCardView CardTemplate => cardTemplate;
    }
}
