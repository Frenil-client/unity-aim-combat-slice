using System.Collections.Generic;

namespace AimCombat.Core.Combat
{
    /// <summary>
    /// 스킬과 상태효과 정의의 검증 규칙. 에디터 인스펙터, 스킬 관리 창, 실행 시 로드, 이후의 테스트가 같은 규칙을 쓴다.
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

        /// <summary>스킬 하나(스킬 1, 스킬 2, 버스트)를 검증한다. 칸에 따라 쓸 수 있는 트리거가 다르다.</summary>
        public static List<string> Validate(SkillDef skill)
        {
            var errors = new List<string>();
            if (skill == null)
            {
                errors.Add("스킬이 비어 있다.");
                return errors;
            }

            if (string.IsNullOrEmpty(skill.Id)) errors.Add("Id 가 비어 있다.");

            int burstBlocks = 0;
            foreach (SkillBlockDef block in skill.Blocks) if (block != null && block.Trigger == SkillTrigger.Burst) burstBlocks++;

            if (skill.IsBurst)
            {
                if (skill.Cooldown <= Fixed.Zero) errors.Add("버스트 쿨다운이 0보다 커야 한다.");
                if (skill.BurstStage < 1 || skill.BurstStage > BurstChain.StageCount) errors.Add("버스트 단계는 1에서 " + BurstChain.StageCount + " 사이여야 한다.");
                if (skill.BurstStage < BurstChain.StageCount && skill.NextStageWindow <= Fixed.Zero) errors.Add("1, 2단계 버스트는 다음 단계 창 시간이 0보다 커야 한다.");
                if (burstBlocks == 0) errors.Add("버스트 스킬에는 Burst 트리거 발동이 하나 이상 있어야 한다. 없으면 써도 아무 일이 없다.");
            }
            else if (burstBlocks > 0)
            {
                errors.Add("Burst 트리거 발동은 버스트 스킬 칸에서만 쓴다.");
            }

            if (skill.Blocks.Count == 0) errors.Add("발동이 하나도 없다.");
            for (int i = 0; i < skill.Blocks.Count; i++)
            {
                string at = "발동 " + (i + 1) + ": ";
                SkillBlockDef block = skill.Blocks[i];
                if (block == null)
                {
                    errors.Add(at + "비어 있다.");
                    continue;
                }
                if (skill.IsBurst && block.Trigger == SkillTrigger.Active) errors.Add(at + "Active 트리거 발동은 스킬 1, 2 칸에서만 쓴다.");
                AddBlockErrors(block, errors, at);
            }
            return errors;
        }

        /// <summary>발동 블록 하나를 검증한다. 칸에 따른 규칙은 스킬 검증이 본다.</summary>
        public static List<string> Validate(SkillBlockDef block)
        {
            var errors = new List<string>();
            if (block == null) errors.Add("발동이 비어 있다.");
            else AddBlockErrors(block, errors, "");
            return errors;
        }

        private static void AddBlockErrors(SkillBlockDef block, List<string> errors, string prefix)
        {
            if (block.Trigger == SkillTrigger.Active)
            {
                if (block.Cooldown <= Fixed.Zero) errors.Add(prefix + "쿨다운이 0보다 커야 한다.");
            }
            else if (block.Cooldown < Fixed.Zero)
            {
                errors.Add(prefix + "재발동 간격은 음수일 수 없다.");
            }

            AddTriggerErrors(block, errors, prefix);
            if (block.Effects.Count == 0) errors.Add(prefix + "효과가 하나도 없다.");

            for (int i = 0; i < block.Effects.Count; i++)
            {
                SkillEffectDef effect = block.Effects[i];
                string at = prefix + "효과 " + (i + 1) + ": ";
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

                    case SkillEffectKind.RemoveStatus:
                        if (effect.Status == null) errors.Add(at + "지울 상태효과가 없다.");
                        break;
                }

                if (effect.CoreHit && effect.Kind != SkillEffectKind.Damage) errors.Add(at + "코어 명중은 피해 효과에만 쓴다.");
                if ((effect.Target == SkillTarget.NearestEnemies || effect.Target == SkillTarget.RandomEnemies) && effect.Count < 1) errors.Add(at + "대상 적 수가 1 이상이어야 한다.");
                if (effect.Target == SkillTarget.TriggerEnemy && !SkillTriggers.HasTriggerEnemy(block.Trigger))
                    errors.Add(at + block.Trigger + " 트리거에는 트리거를 일으킨 적이 없다. 명중, 처치 트리거에서만 쓴다.");
                if (effect.Target == SkillTarget.TriggerAlly && block.Trigger != SkillTrigger.AllyBurstUsed)
                    errors.Add(at + "트리거를 일으킨 아군은 AllyBurstUsed 트리거에서만 쓴다.");
            }
        }

        // 트리거마다 필요한 설정과 발동 조건을 확인한다.
        private static void AddTriggerErrors(SkillBlockDef block, List<string> errors, string prefix)
        {
            if (SkillTriggers.IsCounted(block.Trigger) && block.TriggerCount < 1) errors.Add(prefix + "발동 횟수가 1 이상이어야 한다.");
            if (SkillTriggers.WatchesStatus(block.Trigger) && block.TriggerStatus == null) errors.Add(prefix + block.Trigger + " 트리거는 지켜볼 상태효과가 있어야 한다.");
            if (block.Trigger == SkillTrigger.StatusExpired && block.TriggerStatus != null && block.TriggerStatus.Permanent)
                errors.Add(prefix + "지속형 상태효과(" + block.TriggerStatus + ")는 만료되지 않아 StatusExpired 가 발동하지 않는다.");

            if (block.Trigger == SkillTrigger.Periodic)
            {
                if (block.Interval <= Fixed.Zero) errors.Add(prefix + "반복 주기가 0보다 커야 한다.");
                if (block.AltIntervalStatus != null && block.AltInterval <= Fixed.Zero) errors.Add(prefix + "대체 주기가 0보다 커야 한다.");
                if (block.AltIntervalStatus == null && block.AltInterval > Fixed.Zero) errors.Add(prefix + "대체 주기를 쓰려면 대체 주기 상태효과가 있어야 한다.");
            }

            if (block.Trigger == SkillTrigger.HealthBelow && (block.HealthThreshold <= Fixed.Zero || block.HealthThreshold >= Fixed.One))
                errors.Add(prefix + "체력 기준은 0과 1 사이여야 한다.");

            if (block.Chance <= Fixed.Zero || block.Chance > Fixed.One) errors.Add(prefix + "발동 확률은 0보다 크고 1 이하여야 한다.");
            if (block.RequireSelfStatus != null && block.RequireSelfStatus == block.ForbidSelfStatus) errors.Add(prefix + "같은 상태효과를 필요 조건과 금지 조건에 함께 둘 수 없다.");

            if (block.IsPassive) return;
            if (block.RequireSelfStatus != null || block.ForbidSelfStatus != null || block.BurstState != BurstCondition.Any || block.Chance < Fixed.One)
                errors.Add(prefix + "발동 조건(상태 보유, 풀 버스트 여부, 확률)은 패시브 트리거에만 쓴다.");
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
            if (!def.Permanent && def.Duration <= Fixed.Zero) errors.Add(at + "지속 시간이 0보다 커야 한다.");
            if (def.Permanent && (def.Rule == StatusRule.DamageOverTime || def.Rule == StatusRule.Control)) errors.Add(at + "지속 피해와 행동 불가는 지속형으로 둘 수 없다.");

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
