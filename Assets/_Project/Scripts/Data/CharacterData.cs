using System;
using System.Collections.Generic;
using AimCombat.Core.Combat;
using UnityEngine;

namespace AimCombat.Data
{
    /// <summary>추가 스킬 세트 하나. 스킬 1, 스킬 2, 버스트 스킬 한 묶음과 선택 화면에 보일 이름, 한 줄 설명이다.</summary>
    [Serializable]
    public sealed class SkillSetEntry
    {
        public string displayName = "";
        [TextArea(1, 3)]
        public string summary = "";
        public SkillData skill1;
        public SkillData skill2;
        public SkillData burstSkill;

        public SkillData SkillOf(SkillSlot slot)
        {
            switch (slot)
            {
                case SkillSlot.Skill1: return skill1;
                case SkillSlot.Skill2: return skill2;
                default: return burstSkill;
            }
        }
    }

    /// <summary>
    /// 캐릭터의 스킬 구성 에셋(SPEC 3.2). 모든 캐릭터가 스킬 1, 스킬 2, 버스트 스킬 세 칸을 가진다.
    /// 칸 수가 같아서 스킬 정보를 보여 주는 UI 가 캐릭터마다 같은 모양이 된다.
    /// 세 칸 묶음을 스킬 세트라 하고, 기본 세트(0번) 외에 추가 세트를 둘 수 있다. 한 판을 시작할 때 캐릭터마다 세트 하나를 고른다.
    /// 분대원(SquadController)에 연결하고, 기본 세트는 스킬 관리 창(Aim Combat/스킬 관리)에서 한꺼번에 보고 고친다.
    /// </summary>
    [CreateAssetMenu(menuName = "Aim Combat/Character", fileName = "Character_")]
    public sealed class CharacterData : ScriptableObject
    {
        [SerializeField] private string displayName = "";

        [Header("기본 스킬 세트(0번)")]
        [Tooltip("선택 화면에 보이는 세트 이름.")]
        [SerializeField] private string setName = "";
        [TextArea(1, 3)]
        [SerializeField] private string setSummary = "";
        [SerializeField] private SkillData skill1;
        [SerializeField] private SkillData skill2;
        [SerializeField] private SkillData burstSkill;

        [Header("추가 스킬 세트(1번부터)")]
        [Tooltip("버스트 단계는 기본 세트와 같아야 한다. 어떤 조합을 골라도 버스트 연계가 이어지게 하기 위해서다.")]
        [SerializeField] private List<SkillSetEntry> alternateSets = new List<SkillSetEntry>();

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public SkillData Skill1 => skill1;
        public SkillData Skill2 => skill2;
        public SkillData BurstSkill => burstSkill;

        /// <summary>고를 수 있는 세트 수. 기본 세트가 있어 늘 1 이상이다.</summary>
        public int SetCount => 1 + alternateSets.Count;

        /// <summary>기본 세트의 칸별 스킬.</summary>
        public SkillData SkillOf(SkillSlot slot) => SkillOf(slot, 0);

        /// <summary>set 번 세트의 칸별 스킬. 범위를 벗어나면 기본 세트를 준다.</summary>
        public SkillData SkillOf(SkillSlot slot, int set)
        {
            if (set > 0 && set < SetCount) return alternateSets[set - 1].SkillOf(slot);
            switch (slot)
            {
                case SkillSlot.Skill1: return skill1;
                case SkillSlot.Skill2: return skill2;
                default: return burstSkill;
            }
        }

        public string SetNameOf(int set)
        {
            string value = set > 0 && set < SetCount ? alternateSets[set - 1].displayName : setName;
            return string.IsNullOrEmpty(value) ? "세트 " + (char)('A' + Mathf.Clamp(set, 0, 25)) : value;
        }

        public string SetSummaryOf(int set) => set > 0 && set < SetCount ? alternateSets[set - 1].summary : setSummary;

        /// <summary>칸과 스킬이 맞는지, 각 스킬이 검증을 통과하는지 확인한다. 비어 있으면 통과다. 빈 칸은 허용한다.</summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            for (int set = 0; set < SetCount; set++)
            {
                string prefix = SetCount > 1 ? SetNameOf(set) + " " : "";
                AddSlotErrors(set, SkillSlot.Skill1, prefix + "스킬 1", errors);
                AddSlotErrors(set, SkillSlot.Skill2, prefix + "스킬 2", errors);
                AddSlotErrors(set, SkillSlot.Burst, prefix + "버스트 스킬", errors);

                SkillData first = SkillOf(SkillSlot.Skill1, set);
                if (first != null && first == SkillOf(SkillSlot.Skill2, set)) errors.Add(prefix + "스킬 1과 스킬 2가 같은 에셋이다.");

                // 세트가 달라도 버스트 단계는 같아야 한다(SPEC 3.2 스킬 세트 선택).
                SkillData burst = SkillOf(SkillSlot.Burst, set);
                if (set > 0 && burst != null && burstSkill != null && burst.BurstStage != burstSkill.BurstStage)
                    errors.Add(prefix + "버스트 단계(" + burst.BurstStage + ")가 기본 세트(" + burstSkill.BurstStage + ")와 다르다.");
            }
            return errors;
        }

        private void AddSlotErrors(int set, SkillSlot slot, string label, List<string> errors)
        {
            SkillData skill = SkillOf(slot, set);
            if (skill == null) return;
            if (skill.Slot != slot) errors.Add(label + ": " + skill.name + " 은 " + skill.Slot + " 칸용 스킬이다.");
            foreach (string error in skill.Validate()) errors.Add(label + " " + skill.name + ": " + error);
        }
    }
}
