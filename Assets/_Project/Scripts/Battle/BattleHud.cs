using System.Collections.Generic;
using System.Text;
using AimCombat.Core;
using AimCombat.Core.Combat;
using AimCombat.Data;
using AimCombat.Squad;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 임시 전투 HUD. 웨이브와 남은 시간, 아군별 패널(이름, 버스트 단계, 체력, 엄폐물, 탄약, 버프), 스킬 줄(일반 스킬과 버스트 쿨다운),
    /// 버스트 패널과 후보 버튼, 자동 버스트와 자동 전투 전환 버튼, 보스 패널(체력, 저항 게이지, 상태효과, 강타 예고와 저지 진행), 스킬과 부위 파괴 알림, 결과 표시.
    /// 보스의 부위 체력과 저지 표적은 BossPartHud 가 보스 위치에 띄운다.
    /// - 아군 패널은 버튼이라 누르면 그 아군을 조작한다(Z, X, C, V, B 키와 같다).
    /// - 스킬 줄은 보여주기만 한다. 일반 스킬은 각자 쿨다운이 차는 대로 쓴다.
    /// - 버스트 패널은 게이지, 지금 단계, 창의 남은 시간, 풀 버스트를 보여준다. 단계가 열리면 그 단계에서 쓸 수 있는 아군이
    ///   배치 순서대로 후보 버튼에 뜨고, n번째 버튼은 숫자 n 키와 같다. 쓸 수 있는 아군이 없으면 그렇다고 알린다.
    /// UI 시스템 패키지로 옮기는 것은 리소스가 준비된 뒤에 한다.
    /// </summary>
    public sealed class BattleHud : MonoBehaviour
    {
        private static readonly string[] SelectKeyNames = { "Z", "X", "C", "V", "B" };
        private static readonly string[] StageNames = { "-", "I", "II", "III" };

        [SerializeField] private SquadController squad;
        [SerializeField] private BattleDirector director;
        [SerializeField] private Text waveText;
        [SerializeField] private Text resultText;

        [Header("아군 패널")]
        [SerializeField] private Button[] allyButtons;
        [SerializeField] private Text[] allyTexts;
        [SerializeField] private Text[] allyStatusTexts;
        [SerializeField] private Image[] allyHealthFills;
        [SerializeField] private Image[] allyCoverFills;
        [SerializeField] private Image[] allyFrames;
        [Tooltip("아군 패널의 얼굴 초상. 전투 불능이면 어둡게 한다.")]
        [SerializeField] private Image[] allyFaces;

        [Header("스킬 줄")]
        [SerializeField] private GameObject[] skillRows;
        [SerializeField] private Text[] skillTexts;
        [SerializeField] private Image[] skillFills;

        [Header("버스트")]
        [SerializeField] private Text burstText;
        [SerializeField] private Image burstFrame;
        [SerializeField] private Image burstGaugeFill;
        [Tooltip("후보 버튼. 위에서부터 1, 2, 3 ... 번째 후보(숫자 키와 같다).")]
        [SerializeField] private Button[] candidateButtons;
        [SerializeField] private Text[] candidateTexts;
        [SerializeField] private Image[] candidateFaces;

        [Header("자동 전환")]
        [SerializeField] private Button autoBurstButton;
        [SerializeField] private Text autoBurstText;
        [SerializeField] private Button autoBattleButton;
        [SerializeField] private Text autoBattleText;

        [Header("보스")]
        [SerializeField] private GameObject bossPanel;
        [SerializeField] private Text bossText;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private Image bossGaugeFill;
        [SerializeField] private Text bossWarningText;
        [SerializeField] private Image bossCancelFill;

        [Header("알림")]
        [SerializeField] private Text feedText;
        [SerializeField] private float feedDuration = 1.6f;

        [Header("색")]
        [SerializeField] private Color burstIdleColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color burstStageColor = new Color(0.9f, 0.45f, 0.1f, 0.85f);
        [SerializeField] private Color burstBlockedColor = new Color(0.35f, 0.3f, 0.3f, 0.85f);
        [SerializeField] private Color fullBurstColor = new Color(0.85f, 0.2f, 0.25f, 0.85f);
        [Tooltip("버스트를 바로 쓸 수 있는 후보 버튼 바탕.")]
        [SerializeField] private Color candidateReadyColor = new Color(0.9f, 0.45f, 0.1f, 0.9f);
        [Tooltip("단계는 맞지만 쿨다운 중이라 누를 수 없는 아군 칸 바탕.")]
        [SerializeField] private Color candidateWaitingColor = new Color(0.25f, 0.23f, 0.27f, 0.85f);
        [SerializeField] private Color skillReadyColor = new Color(0.9f, 0.45f, 0.1f, 0.85f);
        [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0.55f);
        [SerializeField] private Color downColor = new Color(0.3f, 0.05f, 0.05f, 0.7f);
        [SerializeField] private Color toggleOnColor = new Color(0.2f, 0.6f, 0.35f, 0.85f);

        private readonly StringBuilder builder = new StringBuilder();
        private float feedEndTime;
        private AimCombat.Feel.BurstCutIn cutIn;

        private void Awake()
        {
            for (int i = 0; i < allyButtons.Length; i++)
            {
                int index = i;
                allyButtons[i].onClick.AddListener(() => squad.Select(index));
            }
            for (int i = 0; i < candidateButtons.Length; i++)
            {
                int candidate = i;
                candidateButtons[i].onClick.AddListener(() => squad.TryBurstCandidate(candidate));
            }
            if (autoBurstButton != null) autoBurstButton.onClick.AddListener(() => squad.AutoBurst = !squad.AutoBurst);
            if (autoBattleButton != null) autoBattleButton.onClick.AddListener(() => squad.AutoControlSelected = !squad.AutoControlSelected);
            for (int i = 0; i < allyFaces.Length && i < squad.Count; i++) SetFace(allyFaces[i], squad.FaceOf(i));
            cutIn = FindFirstObjectByType<AimCombat.Feel.BurstCutIn>(FindObjectsInactive.Include);
            squad.SkillUsed += OnSkillUsed;
            resultText.gameObject.SetActive(false);
            if (feedText != null) feedText.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            float timeLeft = director.TimeLeft;
            string time = Mathf.FloorToInt(timeLeft / 60f) + ":" + Mathf.FloorToInt(timeLeft % 60f).ToString("00");
            string wave = director.InBreak ? "다음 웨이브 준비" : director.BossWave ? "보스" : "남은 적 " + director.EnemiesLeft;
            string waveName = director.BossWave ? "BOSS" : "WAVE " + director.WaveNumber + " / " + director.WaveCount;
            waveText.text = waveName + "    " + time + "    " + wave;

            for (int i = 0; i < allyTexts.Length && i < squad.Count; i++) RefreshAlly(i);

            RefreshBurst();
            RefreshToggles();
            RefreshBoss();

            if (feedText != null && feedText.gameObject.activeSelf && Time.time >= feedEndTime) feedText.gameObject.SetActive(false);

            if (director.State != BattleState.Playing && !resultText.gameObject.activeSelf)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = (director.State == BattleState.Victory ? "승리" : "패배") + "\n<size=36>누르거나 R 키로 다시 시작</size>";
            }
        }

        private void RefreshAlly(int i)
        {
            AllyUnit unit = squad.UnitOf(i);
            var weapon = squad.WeaponOf(i);
            string key = i < SelectKeyNames.Length ? SelectKeyNames[i] : (i + 1).ToString();
            SkillData burstSkill = squad.BurstSkillOf(i);
            string stage = burstSkill != null ? "  <size=24>[B" + StageNames[burstSkill.BurstStage] + "]</size>" : "";
            string state = unit.IsDown ? "전투 불능" : squad.IsReloading(i) ? "재장전" : weapon.Ammo + " / " + weapon.MagazineSize;
            allyTexts[i].text = key + "  " + squad.NameOf(i) + stage + "\n" + state;

            allyHealthFills[i].rectTransform.localScale = new Vector3(unit.Health / unit.MaxHealth, 1f, 1f);
            CoverObject cover = unit.Cover;
            float coverRatio = cover != null ? cover.Durability / cover.MaxDurability : 0f;
            allyCoverFills[i].rectTransform.localScale = new Vector3(coverRatio, 1f, 1f);
            allyFrames[i].color = unit.IsDown ? downColor : i == squad.SelectedIndex ? selectedColor : normalColor;
            if (i < allyFaces.Length && allyFaces[i].enabled) allyFaces[i].color = unit.IsDown ? new Color(0.35f, 0.35f, 0.4f, 1f) : Color.white;

            if (i < allyStatusTexts.Length) allyStatusTexts[i].text = Describe(squad.CombatantOf(i).Statuses);
            if (i < skillTexts.Length) RefreshSkill(i, unit.IsDown);
        }

        // 스킬 줄: 일반 스킬 이름과 쿨다운(막대), 오른쪽에 버스트 쿨다운.
        private void RefreshSkill(int i, bool down)
        {
            SkillData skill = squad.ActiveSkillOf(i);
            SkillCooldown cooldown = squad.CooldownOf(i);
            SkillCooldown burstCooldown = squad.BurstCooldownOf(i);
            if (skill == null && burstCooldown == null)
            {
                skillRows[i].SetActive(false);
                return;
            }

            string text = "";
            float progress = 0f;
            bool ready = false;
            if (skill != null && cooldown != null)
            {
                ready = cooldown.IsReady && !down;
                text = skill.DisplayName + " " + (down ? "-" : ready ? "준비" : Seconds(cooldown.Remaining) + "초");
                progress = down ? 0f : FixedConvert.ToFloat(cooldown.Progress01);
            }
            if (burstCooldown != null)
            {
                string burstState = down ? "-" : burstCooldown.IsReady ? "준비" : Seconds(burstCooldown.Remaining) + "초";
                text += "  <color=#FFB060>B " + burstState + "</color>";
            }

            skillTexts[i].text = text;
            skillFills[i].rectTransform.localScale = new Vector3(progress, 1f, 1f);
            skillFills[i].color = ready ? skillReadyColor : new Color(0.3f, 0.55f, 0.9f, 0.6f);
        }

        // 충전 중: 게이지 %. 연계 중: 지금 단계와 창의 남은 시간(1단계는 제한 없음), 후보가 없으면 그렇다고 알린다. 풀 버스트: 남은 시간.
        private void RefreshBurst()
        {
            if (burstText == null) return;

            BurstChain burst = squad.Burst;
            IReadOnlyList<int> candidates = squad.BurstCandidates;
            IReadOnlyList<int> waiting = squad.BurstWaiting;
            string text;
            Color color;
            float gauge = burst.GaugeMax.raw > 0 ? burst.Gauge.raw / (float)burst.GaugeMax.raw : 0f;
            switch (burst.Phase)
            {
                case BurstPhase.Stage:
                    string window = burst.StageTimed ? "  " + Seconds(burst.TimeLeft) + "초" : "";
                    string hint = candidates.Count > 0 ? (candidates.Count == 1 ? "1" : "1-" + candidates.Count) + " 키로 선택"
                        : waiting.Count > 0 ? StageNames[burst.Stage] + "단계 쿨다운 중"
                        : "쓸 수 있는 " + StageNames[burst.Stage] + "단계 없음";
                    text = "버스트 " + StageNames[burst.Stage] + " 단계" + window + "\n<size=24>" + hint + "</size>";
                    color = candidates.Count == 0 ? burstBlockedColor : burstStageColor;
                    gauge = 1f;
                    break;
                case BurstPhase.FullBurst:
                    text = "풀 버스트\n" + Seconds(burst.TimeLeft) + "초";
                    color = fullBurstColor;
                    gauge = burst.FullBurstDuration.raw > 0 ? burst.TimeLeft.raw / (float)burst.FullBurstDuration.raw : 0f;
                    break;
                default:
                    text = "버스트\n" + Mathf.FloorToInt(gauge * 100f) + "%";
                    color = burstIdleColor;
                    break;
            }

            burstText.text = text;
            if (burstFrame != null) burstFrame.color = color;
            if (burstGaugeFill != null) burstGaugeFill.rectTransform.localScale = new Vector3(Mathf.Clamp01(gauge), 1f, 1f);

            // 후보(번호, 누를 수 있음) 뒤에 쿨다운 중인 아군(번호 없음, 어둡게, 남은 시간)을 이어 놓는다(SPEC 3.2).
            for (int c = 0; c < candidateButtons.Length; c++)
            {
                bool ready = c < candidates.Count;
                int w = c - candidates.Count;
                bool show = ready || w < waiting.Count;
                Button button = candidateButtons[c];
                if (button.gameObject.activeSelf != show) button.gameObject.SetActive(show);
                if (!show) continue;

                int index = ready ? candidates[c] : waiting[w];
                if (ready)
                {
                    candidateTexts[c].text = (c + 1) + "  " + squad.NameOf(index) + "  " + squad.BurstSkillOf(index).DisplayName;
                }
                else
                {
                    string left = Seconds(squad.BurstCooldownOf(index).Remaining);
                    candidateTexts[c].text = "<color=#B0B0B8>" + squad.NameOf(index) + "  쿨다운</color>  <color=#FFB060>" + left + "초</color>";
                }
                button.interactable = ready;
                ((Image)button.targetGraphic).color = ready ? candidateReadyColor : candidateWaitingColor;
                if (c < candidateFaces.Length)
                {
                    SetFace(candidateFaces[c], squad.FaceOf(index));
                    candidateFaces[c].color = ready ? Color.white : new Color(0.45f, 0.45f, 0.5f, 1f);
                }
            }
        }

        private void RefreshToggles()
        {
            SetToggle(autoBurstButton, autoBurstText, squad.AutoBurst, "자동 버스트", "Tab");
            SetToggle(autoBattleButton, autoBattleText, squad.AutoControlSelected, "자동 전투", "Shift");
        }


        private void SetToggle(Button button, Text label, bool on, string name, string key)
        {
            if (button == null) return;
            label.text = name + " " + (on ? "ON" : "OFF") + "\n<size=22>(" + key + ")</size>";
            ((Image)button.targetGraphic).color = on ? toggleOnColor : normalColor;
        }

        // 보스가 나와 있는 동안만 보인다. 강타 예고 중에는 경고와 저지 진행 막대(표적들이 깎인 비율)를 띄운다.
        private void RefreshBoss()
        {
            if (bossPanel == null) return;
            EnemyUnit boss = director.Boss;
            bool show = boss != null && !boss.IsDead;
            if (bossPanel.activeSelf != show) bossPanel.SetActive(show);
            if (!show) return;

            Combatant c = boss.Combatant;
            float health = FixedConvert.ToFloat(c.Health);
            bossText.text = boss.Data.DisplayName + "  " + Mathf.CeilToInt(health) + "    " + Describe(c.Statuses);
            bossHealthFill.rectTransform.localScale = new Vector3(health / FixedConvert.ToFloat(c.MaxHealth), 1f, 1f);
            float gauge = c.ControlGaugeMax.raw > 0 ? c.ControlGauge.raw / (float)c.ControlGaugeMax.raw : 0f;
            bossGaugeFill.rectTransform.localScale = new Vector3(c.IsControlled ? 0f : gauge, 1f, 1f);

            BossBrain brain = boss.Boss;
            string warning = "";
            bool telegraph = brain != null && brain.Phase == BossPhase.SlamTelegraph;
            if (brain != null)
            {
                switch (brain.Phase)
                {
                    case BossPhase.SlamTelegraph:
                        string target = brain.SlamTarget != null ? brain.SlamTarget.name : "";
                        warning = "강타 예고: " + target + "  표적 " + brain.MarksLeft + "개를 부숴라";
                        break;
                    case BossPhase.Stagger:
                        warning = "강타 저지";
                        break;
                    case BossPhase.Broken:
                        warning = "그로기";
                        break;
                    case BossPhase.Barrage:
                        warning = "탄막";
                        break;
                }
            }
            bossWarningText.text = warning;
            bossCancelFill.transform.parent.gameObject.SetActive(telegraph);
            if (telegraph) bossCancelFill.rectTransform.localScale = new Vector3(brain.CancelProgress, 1f, 1f);
        }

        private void OnEnable() => BossBrain.PartDestroyed += OnBossPartDestroyed;

        private void OnDisable() => BossBrain.PartDestroyed -= OnBossPartDestroyed;

        // 보스의 팔이 부서지면 알림으로 띄운다(SPEC 3.4).
        private void OnBossPartDestroyed(BossBrain boss, BossPart part)
        {
            if (feedText == null) return;
            feedText.text = "<color=#FF7060>" + part.DisplayName + " 파괴</color>";
            feedText.gameObject.SetActive(true);
            feedEndTime = Time.time + feedDuration;
        }

        private void OnSkillUsed(int index, SkillData skill)
        {
            if (feedText == null) return;
            // 버스트는 컷인이 같은 내용을 크게 보여주므로 알림을 겹쳐 띄우지 않는다. 컷인이 꺼져 있으면(F2) 알림으로 보여준다.
            if (skill.IsBurst && cutIn != null && cutIn.WillShow) return;
            string kind = skill.IsBurst ? "버스트 " + StageNames[skill.BurstStage] + "  " : "";
            feedText.text = squad.NameOf(index) + "  " + kind + skill.DisplayName;
            feedText.gameObject.SetActive(true);
            feedEndTime = Time.time + feedDuration;
        }

        // 걸린 상태효과를 짧게 나열한다. 중첩형은 중첩 수를 붙인다. 색은 StatusData 의 색을 쓴다.
        private string Describe(StatusContainer statuses)
        {
            builder.Clear();
            foreach (StatusInstance s in statuses.Active)
            {
                if (builder.Length > 0) builder.Append(' ');
                StatusData data = StatusData.Find(s.Def);
                Color color = data != null ? data.Color : Color.white;
                builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append('>').Append(s.Def.Name);
                if (s.Stacks > 1) builder.Append(" x").Append(s.Stacks);
                builder.Append("</color>");
            }
            return builder.ToString();
        }

        // 얼굴 그림을 넣는다. 그림이 없으면 숨긴다(빈 흰 사각형이 보이지 않게).
        private static void SetFace(Image image, Sprite sprite)
        {
            if (image == null) return;
            image.sprite = sprite;
            image.enabled = sprite != null;
        }

        private static string Seconds(Fixed value) => (value.raw / (float)Fixed.Scale).ToString("0.0");
    }
}
