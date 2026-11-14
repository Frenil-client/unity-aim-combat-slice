using AimCombat.Core;
using AimCombat.Core.Combat;
using AimCombat.Squad;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 임시 전투 HUD. 웨이브와 남은 시간, 아군별 패널(이름, 체력, 엄폐물, 탄약), 궁극기 버튼, 결과 표시.
    /// 궁극기 버튼은 게이지와 연계 순번, 남은 시간을 보여주고 누르면 다음 순번을 발동한다(Q 키와 같다).
    /// 아군 패널은 버튼이라 누르면 그 아군을 조작한다(초상 터치 대신). UI 시스템 패키지로 옮기는 것은 리소스가 준비된 뒤에 한다.
    /// </summary>
    public sealed class BattleHud : MonoBehaviour
    {
        [SerializeField] private SquadController squad;
        [SerializeField] private BattleDirector director;
        [SerializeField] private Text waveText;
        [SerializeField] private Text resultText;
        [SerializeField] private Button[] allyButtons;
        [SerializeField] private Text[] allyTexts;
        [SerializeField] private Image[] allyHealthFills;
        [SerializeField] private Image[] allyCoverFills;
        [SerializeField] private Image[] allyFrames;
        [SerializeField] private Button ultimateButton;
        [SerializeField] private Text ultimateText;
        [SerializeField] private Image ultimateFrame;
        [SerializeField] private Color ultimateIdleColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color ultimateReadyColor = new Color(0.9f, 0.45f, 0.1f, 0.85f);
        [SerializeField] private Color enhancedColor = new Color(0.85f, 0.2f, 0.25f, 0.85f);
        [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color downColor = new Color(0.3f, 0.05f, 0.05f, 0.7f);

        private void Awake()
        {
            for (int i = 0; i < allyButtons.Length; i++)
            {
                int index = i;
                allyButtons[i].onClick.AddListener(() => squad.Select(index));
            }
            if (ultimateButton != null) ultimateButton.onClick.AddListener(() => squad.TryUltimate());
            resultText.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            float timeLeft = director.TimeLeft;
            string time = Mathf.FloorToInt(timeLeft / 60f) + ":" + Mathf.FloorToInt(timeLeft % 60f).ToString("00");
            string wave = director.InBreak ? "다음 웨이브 준비" : "남은 적 " + director.EnemiesLeft;
            waveText.text = "WAVE " + director.WaveNumber + " / " + director.WaveCount + "    " + time + "    " + wave;

            for (int i = 0; i < allyTexts.Length && i < squad.Count; i++)
            {
                AllyUnit unit = squad.UnitOf(i);
                var weapon = squad.WeaponOf(i);
                string state = unit.IsDown ? "전투 불능" : squad.IsReloading(i) ? "재장전" : weapon.Ammo + " / " + weapon.MagazineSize;
                allyTexts[i].text = (i + 1) + "  " + squad.NameOf(i) + "\n" + state;

                allyHealthFills[i].rectTransform.localScale = new Vector3(unit.Health / unit.MaxHealth, 1f, 1f);
                CoverObject cover = unit.Cover;
                float coverRatio = cover != null ? cover.Durability / cover.MaxDurability : 0f;
                allyCoverFills[i].rectTransform.localScale = new Vector3(coverRatio, 1f, 1f);

                allyFrames[i].color = unit.IsDown ? downColor : i == squad.SelectedIndex ? selectedColor : normalColor;
            }

            RefreshUltimate();

            if (director.State != BattleState.Playing && !resultText.gameObject.activeSelf)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = (director.State == BattleState.Victory ? "승리" : "패배") + "\n<size=36>누르거나 R 키로 다시 시작</size>";
            }
        }

        // 충전 중: 게이지 %, 다 차면 1번 순번. 연계 중: 다음 순번과 남은 시간. 강화 상태: 남은 시간.
        private void RefreshUltimate()
        {
            if (ultimateText == null) return;

            UltimateChain ultimate = squad.Ultimate;
            string next = (ultimate.ActivatingOrder + 1) + "번 " + squad.NameOf(ultimate.ActivatingOrder);
            string text;
            Color color;
            switch (ultimate.Phase)
            {
                case UltimatePhase.Chain:
                    text = "연계 " + next + "\n" + Seconds(ultimate.TimeLeft) + "초  (Q)";
                    color = ultimateReadyColor;
                    break;
                case UltimatePhase.Enhanced:
                    text = "강화 상태\n" + Seconds(ultimate.TimeLeft) + "초";
                    color = enhancedColor;
                    break;
                default:
                    bool ready = ultimate.CanActivate;
                    long percent = ultimate.GaugeMax.raw > 0 ? ultimate.Gauge.raw * 100 / ultimate.GaugeMax.raw : 0;
                    text = ready ? "궁극기 " + next + "\n(Q)" : "궁극기\n" + percent + "%";
                    color = ready ? ultimateReadyColor : ultimateIdleColor;
                    break;
            }

            ultimateText.text = text;
            if (ultimateFrame != null) ultimateFrame.color = color;
            if (ultimateButton != null) ultimateButton.interactable = ultimate.CanActivate && !squad.UnitOf(ultimate.ActivatingOrder).IsDown;
        }

        private static string Seconds(Fixed value) => (value.raw / (float)Fixed.Scale).ToString("0.0");
    }
}
