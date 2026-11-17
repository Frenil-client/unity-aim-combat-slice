using System;
using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 전투 결과(승리, 패배) 팝업. 전투 화면(BattleHud)이 전투가 끝날 때 UIManager 로 연다.
    /// 화면 아무 곳이나 누르면 다시 시작을 요청한다(RestartRequested). R 키는 전투 감독이 받는다.
    /// 누르는 칸은 팝업 화면 전체를 덮는 투명한 버튼이다. 팝업 뒤의 공유 Dim 이 화면을 덮어 사격 입력(CombatInput)이 UI 위의 누름을 빼기 때문에,
    /// 팝업이 직접 받아야 한다. 전투가 끝나는 순간 쏘던 손가락이 바로 다시 시작으로 이어지지 않게 뜬 뒤 잠깐은 받지 않는다.
    /// 공유 Dim 을 눌러도, 뒤로가기로도 닫히지 않는다(씬을 다시 불러오면 이 팝업도 씬과 함께 정리된다).
    /// </summary>
    public sealed class BattleResultPopup : UIPopup
    {
        [SerializeField] private TMP_Text label;
        [Tooltip("화면 전체를 덮는 투명한 버튼. 누르면 다시 시작을 요청한다.")]
        [SerializeField] private Button restartArea;
        [Tooltip("뜬 뒤 이 시간(초, 실제 시간) 동안은 눌러도 받지 않는다.")]
        [SerializeField] private float inputDelay = 0.8f;

        private float shownAt;

        /// <summary>다시 시작을 눌렀다.</summary>
        public event Action RestartRequested;

        protected override UIViewOptions DefaultOptions => new() { BlockClose = true };

        private void Awake()
        {
            if (restartArea != null) restartArea.onClick.AddListener(OnRestartClicked);
        }

        public void Show(bool victory)
        {
            shownAt = Time.unscaledTime;
            label.text = (victory ? "승리" : "패배") + "\n<size=36>누르거나 R 키로 다시 시작</size>";
        }

        private void OnRestartClicked()
        {
            if (Time.unscaledTime < shownAt + inputDelay) return;
            RestartRequested?.Invoke();
        }
    }
}
