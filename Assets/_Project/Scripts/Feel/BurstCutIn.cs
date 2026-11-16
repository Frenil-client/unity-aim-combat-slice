using AimCombat.Character;
using AimCombat.Core.Combat;
using AimCombat.Data;
using AimCombat.Squad;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace AimCombat.Feel
{
    /// <summary>
    /// 버스트 컷인(SPEC 4절). 버스트를 쓰면 화면을 가로지르는 띠와 그 아군의 컷인 그림, 단계, 스킬 이름을 띄운다.
    /// - 움직임은 Timeline 에셋 하나(BurstCutIn.playable)가 정한다. 띠가 들어오고, 그림이 밀려 들어오고, 빠져나간다.
    ///   타이밍은 Timeline 창에서 바로 고칠 수 있고 코드는 내용(그림, 글, 색)만 바꿔 끼운다.
    /// - 실제 시간으로 돈다. 히트스톱으로 게임 시간이 멈춰도 컷인은 끝까지 흐른다.
    /// - 게임을 멈추지 않는다. 니케처럼 연계가 이어지는 동안 짧게 지나간다. 연달아 쓰면 처음부터 다시 튼다.
    /// - 그림은 아군의 컷인 일러스트(SquadController 의 cutIn)를 쓴다. 없으면 엄폐 포즈 그림으로 대신한다.
    ///   그림은 띠의 형제라 띠 마스크에 잘리지 않고 위아래로 조금 넘친다(머리와 총이 다 보이게).
    /// - 타격감 연출을 끄면(F2) 컷인도 띄우지 않는다.
    /// </summary>
    public sealed class BurstCutIn : MonoBehaviour
    {
        private static readonly string[] StageNames = { "", "I", "II", "III" };

        [SerializeField] private SquadController squad;
        [SerializeField] private FeelDirector feel;
        [SerializeField] private PlayableDirector director;
        [SerializeField] private Image band;
        [SerializeField] private Image portrait;
        [SerializeField] private TMP_Text stageText;
        [SerializeField] private TMP_Text skillText;
        [Tooltip("아군 배치 순서의 대표색(띠 색). 무기 종류와 UI 색을 맞춘다(RESOURCES.md 1.1).")]
        [SerializeField] private Color[] memberColors =
        {
            new Color(0.55f, 0.3f, 0.85f, 0.92f),
            new Color(0.15f, 0.7f, 0.65f, 0.92f),
            new Color(0.95f, 0.5f, 0.15f, 0.92f),
        };
        [Tooltip("풀 버스트로 넘어가는 III단계 컷인의 띠 색.")]
        [SerializeField] private Color fullBurstColor = new Color(0.85f, 0.15f, 0.2f, 0.95f);

        private void Awake()
        {
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.playOnAwake = false;
            squad.SkillUsed += OnSkillUsed;
        }

        /// <summary>지금 버스트를 쓰면 컷인을 띄우는지. 연출을 끄면(F2) 띄우지 않는다. HUD 가 버스트 알림을 겹쳐 띄우지 않을 때 본다.</summary>
        public bool WillShow => isActiveAndEnabled && (feel == null || feel.FeelEnabled);

        private void OnSkillUsed(int member, SkillData skill)
        {
            if (!skill.IsBurst || !WillShow) return;

            // 컷인 그림이 있으면 그것을, 없으면 엄폐 포즈 그림을 임시로 쓴다.
            Sprite sprite = squad.CutInOf(member);
            if (sprite == null)
            {
                CharacterPoseView pose = squad.RigOf(member).GetComponent<CharacterPoseView>();
                sprite = pose != null ? pose.CoverSprite : null;
            }
            portrait.sprite = sprite;
            portrait.enabled = sprite != null;
            if (sprite != null) portrait.preserveAspect = true;

            int stage = Mathf.Clamp(skill.BurstStage, 1, 3);
            bool full = stage >= BurstChain.StageCount;
            band.color = full ? fullBurstColor : member < memberColors.Length ? memberColors[member] : Color.gray;
            stageText.text = "BURST " + StageNames[stage] + (full ? "  FULL BURST" : "");
            skillText.text = squad.NameOf(member) + "  " + skill.DisplayName;

            director.Stop();
            director.time = 0;
            director.Play();
        }
    }
}
