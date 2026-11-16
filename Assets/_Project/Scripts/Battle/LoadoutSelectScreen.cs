using System;
using System.Text;
using AimCombat.Core.Combat;
using AimCombat.Data;
using AimCombat.Squad;
using AimCombat.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 스킬 세트 선택 화면(SPEC 3.2 스킬 세트 선택). 전투 씬이 열리면 전투 화면 위에 덮어 띄우고, 캐릭터마다 세트 하나를 고르게 한다.
    /// - 캐릭터 3명을 배치 순서대로 세 열로 놓는다. 열마다 얼굴, 이름, 버스트 단계와 세트 카드(세트 이름, 한 줄 설명, 세 스킬의 이름).
    /// - 카드를 누르면 그 세트를 고르고 아래 상세 칸에 세 스킬의 설명을 스킬 1, 스킬 2, 버스트 순으로 나란히 띄운다.
    /// - Z, X, C, V, B 키로 그 열의 세트를 바꾸고, 전투 시작 버튼이나 Enter 로 시작한다.
    /// 떠 있는 동안은 분대와 전투 감독을 꺼 두고 게임 시간을 멈추며 전투 HUD 를 숨긴다. 시작하면 모두 되돌리고 스스로 사라진다.
    /// 고를 세트가 없으면(모든 캐릭터의 세트가 하나뿐이면) 띄우지 않는다.
    /// 화면 구성은 씬에 있다. 열과 카드는 원본을 캐릭터 수, 세트 수만큼 복제하고, 배치와 크기는 레이아웃이 정한다(카드는 글 양만큼 자란다).
    /// 여기서는 글과 색만 넣는다.
    /// </summary>
    public sealed class LoadoutSelectScreen : MonoBehaviour
    {
        [SerializeField] private SquadController squad;
        [SerializeField] private BattleDirector director;
        [Tooltip("숨길 전투 HUD 묶음.")]
        [SerializeField] private CanvasGroup hud;

        [Header("구성")]
        [SerializeField] private LoadoutColumnView columnTemplate;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text detailHeader;
        [Tooltip("상세 칸. 스킬 1, 스킬 2, 버스트 순.")]
        [SerializeField] private TMP_Text[] detailTexts = new TMP_Text[3];

        [Header("색")]
        [SerializeField] private Color frameColor = new Color(0.25f, 0.26f, 0.32f, 1f);
        [SerializeField] private Color chargeColor = new Color(0.75f, 0.45f, 1f);
        [SerializeField] private Color autoColor = new Color(0.25f, 0.9f, 0.8f);
        [SerializeField] private Color shotgunColor = new Color(1f, 0.6f, 0.2f);

        private static readonly string[] StageNames = { "-", "I", "II", "III" };
        private static readonly SkillSlot[] Slots = { SkillSlot.Skill1, SkillSlot.Skill2, SkillSlot.Burst };
        private static readonly string[] SlotNames = { "스킬 1", "스킬 2", "버스트" };

        private readonly StringBuilder builder = new StringBuilder();
        private LoadoutCardView[][] cards;
        private int[] chosen;
        private int focus;
        private float pausedTimeScale = 1f;
        private float hudAlpha = 1f;

        /// <summary>화면이 떠 있어 전투가 아직 시작되지 않았다.</summary>
        public bool IsOpen { get; private set; }

        /// <summary>전투 시작을 눌렀다.</summary>
        public event Action Confirmed;

        private void Awake()
        {
            columnTemplate.gameObject.SetActive(false);

            bool selectable = false;
            for (int i = 0; i < squad.Count; i++)
            {
                CharacterData character = squad.CharacterOf(i);
                if (character != null && character.SetCount > 1) selectable = true;
            }
            if (!selectable)
            {
                gameObject.SetActive(false);
                return;
            }

            // Start 가 불리기 전에 꺼 두면 선택이 끝날 때까지 전투가 시작되지 않는다.
            // 게임 시간도 멈춘다. 꺼 둔 두 컴포넌트 밖의 것(상태효과 시간, 연출)까지 흐르지 않게 하기 위해서다. UI 와 키 입력은 시간과 상관없이 받는다.
            squad.enabled = false;
            director.enabled = false;
            pausedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            IsOpen = true;
            SetHudVisible(false);

            chosen = new int[squad.Count];
            for (int i = 0; i < squad.Count; i++) chosen[i] = SquadLoadout.Get(squad.CharacterOf(i));
            focus = Mathf.Clamp(squad.SelectedIndex, 0, squad.Count - 1);
            startButton.onClick.AddListener(Confirm);

            BuildColumns();
            Refresh();
        }

        // 선택 중에 씬을 떠나면(다시 시작, 플레이 종료) 멈춘 시간과 숨긴 HUD 를 되돌린다.
        private void OnDestroy()
        {
            if (!IsOpen) return;
            Time.timeScale = pausedTimeScale > 0f ? pausedTimeScale : 1f;
            SetHudVisible(true);
        }

        private void Update()
        {
            if (!IsOpen) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.zKey.wasPressedThisFrame) Cycle(0);
            if (keyboard.xKey.wasPressedThisFrame) Cycle(1);
            if (keyboard.cKey.wasPressedThisFrame) Cycle(2);
            if (keyboard.vKey.wasPressedThisFrame) Cycle(3);
            if (keyboard.bKey.wasPressedThisFrame) Cycle(4);
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) Confirm();
        }

        /// <summary>member 번 캐릭터의 세트를 고른다. 상세 칸도 그 세트로 바꾼다.</summary>
        public void Choose(int member, int set)
        {
            if (!IsOpen || member < 0 || member >= chosen.Length) return;
            CharacterData character = squad.CharacterOf(member);
            if (character == null) return;
            chosen[member] = Mathf.Clamp(set, 0, character.SetCount - 1);
            focus = member;
            Refresh();
        }

        /// <summary>고른 세트로 전투를 시작한다.</summary>
        public void Confirm()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Time.timeScale = pausedTimeScale > 0f ? pausedTimeScale : 1f;
            for (int i = 0; i < chosen.Length; i++) squad.SetSkillSet(i, chosen[i]);
            squad.enabled = true;
            director.enabled = true;
            SetHudVisible(true);
            Confirmed?.Invoke();
            gameObject.SetActive(false);
        }

        // 전투 HUD 묶음 하나를 투명하게 한다. 켜고 끄지 않아 HUD 가 스스로 켜고 끄는 것과 초기화 순서를 건드리지 않는다.
        private void SetHudVisible(bool visible)
        {
            if (hud == null) return;
            if (!visible) hudAlpha = hud.alpha;
            hud.alpha = visible ? hudAlpha : 0f;
            hud.blocksRaycasts = visible;
            hud.interactable = visible;
        }

        private void Cycle(int member)
        {
            if (member >= chosen.Length) return;
            CharacterData character = squad.CharacterOf(member);
            if (character == null) return;
            Choose(member, (chosen[member] + 1) % character.SetCount);
        }

        private Color AccentOf(int member)
        {
            switch (squad.WeaponOf(member).Kind)
            {
                case WeaponKind.Charge: return chargeColor;
                case WeaponKind.Shotgun: return shotgunColor;
                default: return autoColor;
            }
        }

        // 열과 카드를 원본에서 복제한다. 놓는 자리는 부모의 레이아웃이 정한다.
        private void BuildColumns()
        {
            cards = new LoadoutCardView[squad.Count][];
            for (int i = 0; i < squad.Count; i++)
            {
                LoadoutColumnView column = Instantiate(columnTemplate, columnTemplate.transform.parent);
                column.name = "Column_" + squad.NameOf(i);
                column.gameObject.SetActive(true);

                CharacterData character = squad.CharacterOf(i);
                Sprite face = squad.FaceOf(i);
                column.Face.sprite = face;
                column.Face.enabled = face != null;
                SkillData burst = character != null ? character.BurstSkill : null;
                column.Title.text = "<color=#" + ColorUtility.ToHtmlStringRGB(AccentOf(i)) + ">" + squad.NameOf(i) + "</color>  <size=28>"
                    + (character != null ? character.DisplayName : "") + "</size>\n<size=24><color=#B8C0CC>"
                    + (burst != null ? "버스트 " + StageNames[burst.BurstStage] + " 단계" : "") + "</color></size>";

                column.CardTemplate.gameObject.SetActive(false);
                int sets = character != null ? character.SetCount : 0;
                cards[i] = new LoadoutCardView[sets];
                for (int set = 0; set < sets; set++)
                {
                    LoadoutCardView card = Instantiate(column.CardTemplate, column.CardParent);
                    card.name = "Card_" + (char)('A' + set);
                    card.gameObject.SetActive(true);
                    int member = i;
                    int chosenSet = set;
                    card.Button.onClick.AddListener(() => Choose(member, chosenSet));
                    cards[i][set] = card;
                }
            }
        }

        private void Refresh()
        {
            for (int i = 0; i < cards.Length; i++)
            {
                CharacterData character = squad.CharacterOf(i);
                if (character == null) continue;
                Color accent = AccentOf(i);
                for (int set = 0; set < cards[i].Length; set++)
                {
                    bool on = chosen[i] == set;
                    cards[i][set].Frame.color = on ? accent : frameColor;
                    cards[i][set].Label.text = CardText(character, set, on ? accent : new Color(0.7f, 0.72f, 0.78f));
                    cards[i][set].Label.color = on ? Color.white : new Color(0.72f, 0.74f, 0.8f);
                }
            }

            CharacterData focused = squad.CharacterOf(focus);
            if (focused == null) return;
            int focusedSet = chosen[focus];
            string hex = ColorUtility.ToHtmlStringRGB(AccentOf(focus));
            detailHeader.text = "<color=#" + hex + ">" + squad.NameOf(focus) + "</color>  " + SetLetter(focusedSet) + "  " + focused.SetNameOf(focusedSet)
                + "  <size=24><color=#B8C0CC>" + focused.SetSummaryOf(focusedSet) + "</color></size>";
            for (int s = 0; s < Slots.Length && s < detailTexts.Length; s++)
            {
                SkillData skill = focused.SkillOf(Slots[s], focusedSet);
                string label = Slots[s] == SkillSlot.Burst && skill != null
                    ? "버스트 " + StageNames[skill.BurstStage] + "  쿨다운 " + FixedConvert.ToFloat(skill.ToCore().Cooldown).ToString("0") + "초"
                    : SlotNames[s];
                detailTexts[s].text = skill == null
                    ? "<color=#808590>" + label + "\n비어 있음</color>"
                    : "<size=22><color=#" + hex + ">" + label + "</color></size>\n<b>" + skill.DisplayName + "</b>\n<size=22><color=#D0D4DC>" + skill.Description + "</color></size>";
            }
        }

        private string CardText(CharacterData character, int set, Color accent)
        {
            builder.Clear();
            builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(accent)).Append("><b>").Append(SetLetter(set)).Append("</b></color>  <b>")
                .Append(character.SetNameOf(set)).Append("</b>\n<size=21><color=#B8C0CC>").Append(character.SetSummaryOf(set)).Append("</color></size>");
            for (int s = 0; s < Slots.Length; s++)
            {
                SkillData skill = character.SkillOf(Slots[s], set);
                builder.Append("\n<size=23><color=#8C93A0>").Append(SlotNames[s]).Append("</color>  ").Append(skill != null ? skill.DisplayName : "-").Append("</size>");
            }
            return builder.ToString();
        }

        private static string SetLetter(int set) => ((char)('A' + Mathf.Clamp(set, 0, 25))).ToString();
    }
}
