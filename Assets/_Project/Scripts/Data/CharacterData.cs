using System.Collections.Generic;
using AimCombat.Core.Combat;
using UnityEngine;

namespace AimCombat.Data
{
    /// <summary>
    /// 캐릭터의 스킬 구성 에셋(SPEC 3.2). 모든 캐릭터가 스킬 1, 스킬 2, 버스트 스킬 세 칸을 가진다.
    /// 칸 수가 같아서 스킬 정보를 보여 주는 UI 가 캐릭터마다 같은 모양이 된다.
    /// 분대원(SquadController)에 연결하고, 스킬 관리 창(Aim Combat/스킬 관리)에서 한꺼번에 보고 고친다.
    /// </summary>
    [CreateAssetMenu(menuName = "Aim Combat/Character", fileName = "Character_")]
    public sealed class CharacterData : ScriptableObject
    {
        [SerializeField] private string displayName = "";
        [SerializeField] private SkillData skill1;
        [SerializeField] private SkillData skill2;
        [SerializeField] private SkillData burstSkill;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public SkillData Skill1 => skill1;
        public SkillData Skill2 => skill2;
        public SkillData BurstSkill => burstSkill;

        public SkillData SkillOf(SkillSlot slot)
        {
            switch (slot)
            {
                case SkillSlot.Skill1: return skill1;
                case SkillSlot.Skill2: return skill2;
                default: return burstSkill;
            }
        }

        /// <summary>칸과 스킬이 맞는지, 각 스킬이 검증을 통과하는지 확인한다. 비어 있으면 통과다. 빈 칸은 허용한다.</summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            AddSlotErrors(SkillSlot.Skill1, "스킬 1", errors);
            AddSlotErrors(SkillSlot.Skill2, "스킬 2", errors);
            AddSlotErrors(SkillSlot.Burst, "버스트 스킬", errors);
            if (skill1 != null && skill1 == skill2) errors.Add("스킬 1과 스킬 2가 같은 에셋이다.");
            return errors;
        }

        private void AddSlotErrors(SkillSlot slot, string label, List<string> errors)
        {
            SkillData skill = SkillOf(slot);
            if (skill == null) return;
            if (skill.Slot != slot) errors.Add(label + ": " + skill.name + " 은 " + skill.Slot + " 칸용 스킬이다.");
            foreach (string error in skill.Validate()) errors.Add(label + " " + skill.name + ": " + error);
        }
    }
}
