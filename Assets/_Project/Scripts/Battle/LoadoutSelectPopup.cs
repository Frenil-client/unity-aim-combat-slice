using System.Text;
using AimCombat.Core.Combat;
using AimCombat.Data;
using AimCombat.Squad;
using AimCombat.Weapons;
using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 스킬 세트 선택 팝업(SPEC 3.2 스킬 세트 선택). UI 스택(com.frenil.uisystem)의 UIPopup 이라 프리팹 표에서 태어나 전투 화면 위에 쌓인다.
    /// 전투 감독이 전투 시작 전에 열고(그동안 분대와 감독을 꺼 두고 게임 시간을 멈춘다), 닫힐 때까지 기다렸다가 전투를 시작한다.
    /// - 캐릭터 3명을 배치 순서대로 세 열로 놓는다. 열마다 얼굴, 이름, 버스트 단계와 세트 카드(세트 이름, 한 줄 설명, 세 스킬의 이름).
    /// - 카드를 누르면 그 세트를 고르고 아래 상세 칸에 세 스킬의 설명을 스킬 1, 스킬 2, 버스트 순으로 나란히 띄운다.
    /// - Z, X, C, V, B 키로 그 열의 세트를 바꾸고, 전투 시작 버튼이나 Enter 로 고른 세트를 분대에 넣고 닫는다(Confirmed).
    /// 뒤의 어두운 판은 UI 시스템의 공유 Dim 이고, 눌러도 뒤로가기로도 닫히지 않는다(고르고 시작해야만 닫힌다).
    /// 화면 구성은 프리팹에 있다. 열(LoadoutColumn)과 카드(LoadoutCard)도 각각 프리팹이라 캐릭터 수, 세트 수만큼 복제하고,
    /// 배치와 크기는 레이아웃이 정한다(카드는 글 양만큼 자란다).
    /// 여기서는 글과 색만 넣는다.
    /// </summary>
    public sealed class LoadoutSelectPopup : UIPopup
    {
        [Header("구성")]
        [Tooltip("캐릭터 한 열의 프리팹(LoadoutColumn). 캐릭터 수만큼 columnParent 아래에 복제한다.")]
        [SerializeField] private LoadoutColumnView columnTemplate;
        [SerializeField] private Transform columnParent;
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
        private SquadController squad;
        private LoadoutCardView[][] cards;
        private int[] chosen;
        private int focus;

        protected override UIViewOptions DefaultOptions => new() { BlockClose = true };

        /// <summary>고를 세트가 있는지. 모든 캐릭터의 세트가 하나뿐이면 이 팝업을 열 필요가 없다.</summary>
        public static bool HasChoice(SquadController squad)
        {
            for (int i = 0; i < squad.Count; i++)
            {
                CharacterData character = squad.CharacterOf(i);
                if (character != null && character.SetCount > 1) return true;
            }
            return false;
        }

        private void Awake()
        {
            startButton.onClick.AddListener(Confirm);
        }

        /// <summary>분대를 받아 열과 카드를 만든다. 연 직후 한 번 부른다.</summary>
        public void Bind(SquadController target)
        {
            squad = target;
            chosen = new int[squad.Count];
            for (int i = 0; i < squad.Count; i++) chosen[i] = SquadLoadout.Get(squad.CharacterOf(i));
            focus = Mathf.Clamp(squad.SelectedIndex, 0, squad.Count - 1);
            BuildColumns();
            Refresh();
        }

        // UI 와 키 입력은 게임 시간과 상관없이 받는다(선택 중에는 시간이 멈춰 있다).
        private void Update()
        {
            if (!IsOpen || squad == null) return;
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
            if (!IsOpen || chosen == null || member < 0 || member >= chosen.Length) return;
            CharacterData character = squad.CharacterOf(member);
            if (character == null) return;
            chosen[member] = Mathf.Clamp(set, 0, character.SetCount - 1);
            focus = member;
            Refresh();
        }

        /// <summary>고른 세트를 분대에 넣고 닫는다. 전투 감독이 닫힘을 기다렸다가 전투를 시작한다.</summary>
        public void Confirm()
        {
            if (!IsOpen || chosen == null) return;
            for (int i = 0; i < chosen.Length; i++) squad.SetSkillSet(i, chosen[i]);
            CloseConfirmed();
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

        // 열과 카드를 프리팹에서 복제한다. 놓는 자리는 부모의 레이아웃이 정한다.
        private void BuildColumns()
        {
            cards = new LoadoutCardView[squad.Count][];
            for (int i = 0; i < squad.Count; i++)
            {
                LoadoutColumnView column = Instantiate(columnTemplate, columnParent);
                column.name = "Column_" + squad.NameOf(i);

                CharacterData character = squad.CharacterOf(i);
                Sprite face = squad.FaceOf(i);
                column.Face.sprite = face;
                column.Face.enabled = face != null;
                SkillData burst = character != null ? character.BurstSkill : null;
                column.Title.text = "<color=#" + ColorUtility.ToHtmlStringRGB(AccentOf(i)) + ">" + squad.NameOf(i) + "</color>  <size=28>"
                    + (character != null ? character.DisplayName : "") + "</size>\n<size=24><color=#B8C0CC>"
                    + (burst != null ? "버스트 " + StageNames[burst.BurstStage] + " 단계" : "") + "</color></size>";

                int sets = character != null ? character.SetCount : 0;
                cards[i] = new LoadoutCardView[sets];
                for (int set = 0; set < sets; set++)
                {
                    LoadoutCardView card = Instantiate(column.CardTemplate, column.CardParent);
                    card.name = "Card_" + (char)('A' + set);
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
