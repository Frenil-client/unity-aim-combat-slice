using System.Collections.Generic;
using AimCombat.Core;
using AimCombat.Core.Combat;
using UnityEditor;
using UnityEngine;

namespace AimCombat.Data.Editors
{
    /// <summary>
    /// 스킬 에셋 인스펙터. 기본 필드 아래에 Core 의 SkillValidator 결과와 발동 블록별 요약을 보여준다.
    /// 실행 시 로드와 같은 규칙이라 여기서 통과하면 게임에서도 같은 판정을 받는다.
    /// 여러 스킬을 한꺼번에 보고 고칠 때는 스킬 관리 창(Aim Combat/스킬 관리)을 쓴다.
    /// </summary>
    [CustomEditor(typeof(SkillData))]
    public sealed class SkillDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            CombatDataEditorUtil.DrawResult(((SkillData)target).Validate());

            SkillDef def = ((SkillData)target).BuildFresh();
            EditorGUILayout.LabelField("발동 요약", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(CombatDataEditorUtil.DescribeSkill(def), MessageType.None);
        }
    }

    [CustomEditor(typeof(CharacterData))]
    public sealed class CharacterDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            CombatDataEditorUtil.DrawResult(((CharacterData)target).Validate());
            if (GUILayout.Button("스킬 관리 창에서 열기")) SkillManagerWindow.Open((CharacterData)target);
        }
    }

    [CustomEditor(typeof(StatusData))]
    public sealed class StatusDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            CombatDataEditorUtil.DrawResult(SkillValidator.Validate(((StatusData)target).BuildFresh()));
        }
    }

    internal static class CombatDataEditorUtil
    {
        public static void DrawResult(List<string> errors)
        {
            if (errors.Count == 0)
            {
                EditorGUILayout.HelpBox("검증 통과", MessageType.Info);
                return;
            }
            EditorGUILayout.HelpBox(string.Join("\n", errors), MessageType.Error);
        }

        /// <summary>스킬의 발동 블록을 "■ 언제, 조건 / 실행 순서" 로 풀어 쓴다.</summary>
        public static string DescribeSkill(SkillDef def)
        {
            if (def.Blocks.Count == 0) return "(발동 없음)";
            var lines = new List<string>();
            foreach (SkillBlockDef block in def.Blocks)
            {
                if (block == null) continue;
                if (lines.Count > 0) lines.Add("");
                lines.Add("■ " + DescribeTrigger(block, def));
                lines.Add(DescribeOrder(block));
            }
            return string.Join("\n", lines);
        }

        // 트리거와 발동 조건을 한 문장으로 풀어 쓴다. 니케 스킬 설명의 "~ 시, ~ 상태라면" 에 해당한다.
        public static string DescribeTrigger(SkillBlockDef def, SkillDef skill = null)
        {
            string count = def.TriggerCount > 1 ? def.TriggerCount + "회" : "";
            string when;
            switch (def.Trigger)
            {
                case SkillTrigger.Active: when = "쿨다운 " + def.Cooldown + "초마다 자동 사용"; break;
                case SkillTrigger.Burst: when = skill != null ? "버스트 " + skill.BurstStage + "단계에서 직접 사용" : "버스트 스킬 사용"; break;
                case SkillTrigger.BattleStart: when = "전투 시작 시"; break;
                case SkillTrigger.Periodic:
                    when = def.Interval + "초마다";
                    if (def.AltIntervalStatus != null) when += " (" + def.AltIntervalStatus + " 상태에서는 " + def.AltInterval + "초마다)";
                    break;
                case SkillTrigger.BurstUsed: when = "자신이 버스트 스킬 사용 시"; break;
                case SkillTrigger.AllyBurstUsed: when = "아군이 버스트 스킬 사용 시"; break;
                case SkillTrigger.FullBurstStart: when = "풀 버스트 타임 시작 시"; break;
                case SkillTrigger.FullBurstEnd: when = "풀 버스트 타임 종료 시"; break;
                case SkillTrigger.ShotFired: when = "일반 공격 " + (count.Length > 0 ? count + " " : "") + "시"; break;
                case SkillTrigger.Hit: when = "일반 공격 " + count + " 명중 시"; break;
                case SkillTrigger.CriticalHit: when = "치명 " + count + " 명중 시"; break;
                case SkillTrigger.WeakPointHit: when = "약점 " + count + " 명중 시"; break;
                case SkillTrigger.FullChargeHit: when = "풀 차지 공격 " + count + " 명중 시"; break;
                case SkillTrigger.LastBulletFired: when = "마지막 탄환 발사 시"; break;
                case SkillTrigger.LastBulletHit: when = "마지막 탄환 명중 시"; break;
                case SkillTrigger.Reloaded: when = "재장전 완료 시"; break;
                case SkillTrigger.Kill: when = "적 " + (count.Length > 0 ? count + " " : "") + "처치 시"; break;
                case SkillTrigger.Damaged: when = (count.Length > 0 ? count + " " : "") + "피격 시"; break;
                case SkillTrigger.HealthBelow: when = "체력이 " + def.HealthThreshold * Fixed.FromInt(100) + "% 이하로 내려갈 때"; break;
                case SkillTrigger.Healed: when = "회복 효과를 받을 때"; break;
                case SkillTrigger.StatusGained: when = Name(def.TriggerStatus) + " 획득 시"; break;
                case SkillTrigger.StatusMaxStacks: when = Name(def.TriggerStatus) + " 최대 중첩 시"; break;
                case SkillTrigger.StatusExpired: when = Name(def.TriggerStatus) + " 종료 시"; break;
                default: when = def.Trigger.ToString(); break;
            }

            var conditions = new List<string>();
            if (def.RequireSelfStatus != null) conditions.Add("자신이 " + def.RequireSelfStatus + " 상태라면");
            if (def.ForbidSelfStatus != null) conditions.Add("자신이 " + def.ForbidSelfStatus + " 상태가 아니라면");
            if (def.BurstState == BurstCondition.FullBurstOnly) conditions.Add("풀 버스트 타임이라면");
            if (def.BurstState == BurstCondition.NotFullBurst) conditions.Add("풀 버스트 타임이 아니라면");
            if (def.IsPassive && def.Chance < Fixed.One) conditions.Add(def.Chance * Fixed.FromInt(100) + "% 확률로");
            if (def.IsPassive && def.Cooldown > Fixed.Zero) conditions.Add("재발동 간격 " + def.Cooldown + "초");

            return conditions.Count > 0 ? when + ", " + string.Join(", ", conditions) : when;
        }

        private static string Name(StatusDef status) => status != null ? status.ToString() : "(상태효과 없음)";

        // SkillRunner 와 같은 단계 순서로 효과를 나열한다. 데이터 순서와 실제 실행 순서가 다를 수 있음을 눈으로 보여준다.
        public static string DescribeOrder(SkillBlockDef def)
        {
            var lines = new List<string>();
            foreach (SkillEffectKind stage in SkillRunner.Stages)
            {
                foreach (SkillEffectDef effect in def.Effects)
                {
                    if (effect == null || effect.Kind != stage) continue;
                    string what;
                    if (effect.Kind == SkillEffectKind.ApplyStatus) what = "상태효과 " + Name(effect.Status);
                    else if (effect.Kind == SkillEffectKind.RemoveStatus) what = "상태효과 지우기 " + Name(effect.Status);
                    else what = effect.Kind + " " + effect.Coefficient + (effect.CoreHit ? " (코어 명중)" : "");
                    string target = effect.Target == SkillTarget.NearestEnemies ? "가까운 적 " + effect.Count + "기"
                        : effect.Target == SkillTarget.RandomEnemies ? "무작위 적 " + effect.Count + "기"
                        : effect.Target.ToString();
                    if (effect.TargetStatus != null) target += " 중 " + effect.TargetStatus + " 상태";
                    lines.Add("  " + (lines.Count + 1) + ". " + what + " -> " + target);
                }
            }
            return lines.Count > 0 ? string.Join("\n", lines) : "  (효과 없음)";
        }
    }
}
