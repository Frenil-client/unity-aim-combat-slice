using System.Collections.Generic;

namespace AimCombat.Core.Combat
{
    /// <summary>
    /// 스킬과 상태효과 정의의 검증 규칙. 에디터 인스펙터, 실행 시 로드, 이후의 테스트가 같은 규칙을 쓴다.
    /// 결과는 사람이 읽는 문장 목록이고 비어 있으면 통과다.
    /// </summary>
    public static class SkillValidator
    {
        public static List<string> Validate(StatusDef def)
        {
            var errors = new List<string>();
            AddStatusErrors(def, errors, "");
            return errors;
        }

        public static List<string> Validate(SkillDef skill)
        {
            var errors = new List<string>();
            if (skill == null)
            {
                errors.Add("스킬이 비어 있다.");
                return errors;
            }

            if (string.IsNullOrEmpty(skill.Id)) errors.Add("Id 가 비어 있다.");
            if (skill.Cooldown <= Fixed.Zero) errors.Add("쿨다운이 0보다 커야 한다.");
            if (skill.Trigger == SkillTrigger.Burst)
            {
                if (skill.BurstStage < 1 || skill.BurstStage > BurstChain.StageCount) errors.Add("버스트 단계는 1에서 " + BurstChain.StageCount + " 사이여야 한다.");
                if (skill.BurstStage < BurstChain.StageCount && skill.NextStageWindow <= Fixed.Zero) errors.Add("1, 2단계 버스트는 다음 단계 창 시간이 0보다 커야 한다.");
            }
            if (skill.Effects.Count == 0) errors.Add("효과가 하나도 없다.");

            for (int i = 0; i < skill.Effects.Count; i++)
            {
                SkillEffectDef effect = skill.Effects[i];
                string at = "효과 " + (i + 1) + ": ";
                if (effect == null)
                {
                    errors.Add(at + "비어 있다.");
                    continue;
                }

                bool enemySide = effect.TargetsEnemies;
                switch (effect.Kind)
                {
                    case SkillEffectKind.ApplyStatus:
                        if (effect.Status == null)
                        {
                            errors.Add(at + "걸 상태효과가 없다.");
                            break;
                        }
                        if (effect.Status.IsHarmful && !enemySide) errors.Add(at + "해로운 효과(" + effect.Status + ")를 아군에게 건다.");
                        if (!effect.Status.IsHarmful && enemySide) errors.Add(at + "이로운 효과(" + effect.Status + ")를 적에게 건다.");
                        AddStatusErrors(effect.Status, errors, at);
                        break;

                    case SkillEffectKind.Damage:
                        if (!enemySide) errors.Add(at + "피해는 적에게만 줄 수 있다.");
                        if (effect.Coefficient <= Fixed.Zero) errors.Add(at + "피해 계수가 0보다 커야 한다.");
                        break;

                    case SkillEffectKind.Heal:
                        if (enemySide) errors.Add(at + "회복은 아군에게만 줄 수 있다.");
                        if (effect.Coefficient <= Fixed.Zero) errors.Add(at + "회복 비율이 0보다 커야 한다.");
                        break;

                    case SkillEffectKind.RefillAmmo:
                        if (enemySide) errors.Add(at + "탄창 채우기는 아군에게만 쓸 수 있다.");
                        break;
                }

                if (effect.Target == SkillTarget.NearestEnemies && effect.Count < 1) errors.Add(at + "가까운 적 수가 1 이상이어야 한다.");
            }
            return errors;
        }

        private static void AddStatusErrors(StatusDef def, List<string> errors, string prefix)
        {
            if (def == null)
            {
                errors.Add(prefix + "상태효과가 비어 있다.");
                return;
            }

            string at = prefix + "상태효과 " + def + ": ";
            if (string.IsNullOrEmpty(def.Id)) errors.Add(at + "Id 가 비어 있다.");
            if (def.Duration <= Fixed.Zero) errors.Add(at + "지속 시간이 0보다 커야 한다.");

            bool statRule = def.Rule == StatusRule.Stack || def.Rule == StatusRule.Refresh || def.Rule == StatusRule.Highest;
            if (statRule && def.Stat == StatKind.None) errors.Add(at + "능력치 효과인데 바꿀 능력치가 없다.");
            if (!statRule && def.Stat != StatKind.None) errors.Add(at + def.Rule + " 규칙은 능력치를 바꾸지 않는다. 능력치를 None 으로 둔다.");
            if (statRule && def.Value <= Fixed.Zero) errors.Add(at + "값이 0보다 커야 한다.");
            if (def.Rule == StatusRule.Stack && def.MaxStacks < 1) errors.Add(at + "최대 중첩 수가 1 이상이어야 한다.");
            if (def.Stat == StatKind.Slow && def.Rule != StatusRule.Highest) errors.Add(at + "감속은 최댓값형으로만 쓴다.");
            if (def.Stat == StatKind.Slow && def.Value >= Fixed.One) errors.Add(at + "감속 비율은 1보다 작아야 한다.");

            if (def.Rule == StatusRule.DamageOverTime)
            {
                if (def.TickInterval <= Fixed.Zero) errors.Add(at + "틱 간격이 0보다 커야 한다.");
                if (def.Value <= Fixed.Zero) errors.Add(at + "틱 계수가 0보다 커야 한다.");
            }

            if (def.Rule == StatusRule.Control && def.ImmunityAfter < Fixed.Zero) errors.Add(at + "면역 시간은 음수일 수 없다.");
        }
    }
}
