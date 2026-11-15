using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using AimCombat.Core;
using AimCombat.Core.Combat;

namespace AimCombat.Data
{
    public static class SkillDescription
    {
        private static readonly Regex Tokens = new Regex(@"\{([^{}\r\n]+)\}");

        /// <summary>발동 블록과 효과는 에셋 순서대로 1부터 센다. Block_ 접두사를 생략하면 첫 블록이다.</summary>
        public static string Resolve(string template, SkillDef skill, Action<string> unresolved = null)
        {
            if (string.IsNullOrEmpty(template)) return template;
            return Tokens.Replace(template, match =>
            {
                string path = match.Groups[1].Value;
                const string percentSuffix = ":percent";
                bool percent = path.EndsWith(percentSuffix, StringComparison.Ordinal);
                if (percent) path = path.Substring(0, path.Length - percentSuffix.Length);

                object value = ReadSkill(skill, path);
                if (value is Fixed number)
                {
                    double display = number.ToDoubleForDisplay();
                    return (percent ? display * 100 : display).ToString("0.###", CultureInfo.InvariantCulture)
                        + (percent ? "%" : "");
                }
                if (value != null && !percent) return Convert.ToString(value, CultureInfo.InvariantCulture);

                unresolved?.Invoke(match.Value);
                return match.Value;
            });
        }

        private static object ReadSkill(SkillDef skill, string path)
        {
            switch (path)
            {
                case "Skill_cooldown": return skill.Cooldown;
                case "Skill_burstStage": return skill.BurstStage;
                case "Skill_nextStageWindow": return skill.NextStageWindow;
            }

            int index = 0;
            if (path.StartsWith("Block_", StringComparison.Ordinal) && !TryReadIndex(ref path, "Block_", out index))
                return null;
            SkillBlockDef block = At(skill.Blocks, index);
            return block == null ? null : ReadBlock(block, path);
        }

        private static object ReadBlock(SkillBlockDef block, string path)
        {
            if (path.StartsWith("Effect_", StringComparison.Ordinal))
            {
                if (!TryReadIndex(ref path, "Effect_", out int index)) return null;
                SkillEffectDef effect = At(block.Effects, index);
                return effect == null ? null : ReadEffect(effect, path);
            }

            string field = Field(ref path);
            switch (field)
            {
                case "altIntervalStatus": return ReadStatus(block.AltIntervalStatus, path);
                case "triggerStatus": return ReadStatus(block.TriggerStatus, path);
                case "requireSelfStatus": return ReadStatus(block.RequireSelfStatus, path);
                case "forbidSelfStatus": return ReadStatus(block.ForbidSelfStatus, path);
            }
            if (path != null) return null;
            switch (field)
            {
                case "trigger": return block.Trigger;
                case "cooldown": return block.Cooldown;
                case "triggerCount": return block.TriggerCount;
                case "interval": return block.Interval;
                case "altInterval": return block.AltInterval;
                case "healthThreshold": return block.HealthThreshold;
                case "chance": return block.Chance;
                case "burstState": return block.BurstState;
                default: return null;
            }
        }

        private static object ReadEffect(SkillEffectDef effect, string path)
        {
            string field = Field(ref path);
            switch (field)
            {
                case "status": return ReadStatus(effect.Status, path);
                case "targetStatus": return ReadStatus(effect.TargetStatus, path);
            }
            if (path != null) return null;
            switch (field)
            {
                case "kind": return effect.Kind;
                case "target": return effect.Target;
                case "count": return effect.Count;
                case "coefficient": return effect.Coefficient;
                case "coreHit": return effect.CoreHit;
                default: return null;
            }
        }

        private static object ReadStatus(StatusDef status, string field)
        {
            if (status == null) return null;
            switch (field)
            {
                case null:
                case "displayName": return status.ToString();
                case "rule": return status.Rule;
                case "stat": return status.Stat;
                case "value": return status.Value;
                case "duration": return status.Duration;
                case "permanent": return status.Permanent;
                case "maxStacks": return status.MaxStacks;
                case "tickInterval": return status.TickInterval;
                case "immunityAfter": return status.ImmunityAfter;
                default: return null;
            }
        }

        private static string Field(ref string path)
        {
            int dot = path.IndexOf('.');
            string field = dot < 0 ? path : path.Substring(0, dot);
            path = dot < 0 ? null : path.Substring(dot + 1);
            return field;
        }

        private static bool TryReadIndex(ref string path, string prefix, out int index)
        {
            int start = prefix.Length;
            int end = path.IndexOf('_', start);
            index = -1;
            if (end < 0 || !int.TryParse(path.Substring(start, end - start), NumberStyles.None,
                CultureInfo.InvariantCulture, out int oneBased) || oneBased < 1) return false;
            index = oneBased - 1;
            path = path.Substring(end + 1);
            return true;
        }

        private static T At<T>(List<T> items, int index) where T : class => index < items.Count ? items[index] : null;
    }
}
