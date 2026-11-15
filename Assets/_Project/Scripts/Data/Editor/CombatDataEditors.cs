using System;
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
            var skill = (SkillData)target;
            SkillDef def = skill.BuildFresh();
            CombatDataEditorUtil.DrawResult(SkillValidator.Validate(def));

            var unresolved = new HashSet<string>();
            string description = SkillDescription.Resolve(skill.DescriptionTemplate, def, token => unresolved.Add(token));
            CombatDataEditorUtil.DrawDescription(description, unresolved);
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
        public static void DrawDescription(string description, HashSet<string> unresolved)
        {
            if (string.IsNullOrEmpty(description)) return;
            EditorGUILayout.LabelField("설명 미리보기", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(description, MessageType.None);
            if (unresolved.Count > 0)
                EditorGUILayout.HelpBox("값이나 상태효과 연결을 확인할 변수\n" + string.Join(", ", unresolved), MessageType.Warning);
        }

        private static class EnumOptions<T> where T : Enum
        {
            public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
            public static readonly GUIContent[] Labels = Array.ConvertAll(Values, value => new GUIContent(EnumLabel(value)));
        }

        public static T EnumField<T>(SerializedProperty property, GUIContent label) where T : Enum
        {
            T[] values = EnumOptions<T>.Values;
            int index = property.enumValueIndex;
            Rect rect = EditorGUILayout.GetControlRect(label != GUIContent.none);
            using (new EditorGUI.PropertyScope(rect, label, property))
            {
                int next = EditorGUI.Popup(rect, label, index, EnumOptions<T>.Labels);
                if (next != index) property.enumValueIndex = next;
                return next >= 0 && next < values.Length ? values[next] : default;
            }
        }

        public static string EnumLabel(Enum value) => value + " (" + KoreanName(value) + ")";

        private static string KoreanName(object value) => value switch
        {
            SkillTrigger.Active => "쿨다운마다 자동 사용",
            SkillTrigger.Burst => "버스트 단계에서 직접 사용",
            SkillTrigger.BattleStart => "전투 시작",
            SkillTrigger.Periodic => "일정 주기마다 발동",
            SkillTrigger.BurstUsed => "자신이 버스트 사용",
            SkillTrigger.AllyBurstUsed => "아군이 버스트 사용 (자신 포함)",
            SkillTrigger.FullBurstStart => "풀 버스트 시작",
            SkillTrigger.FullBurstEnd => "풀 버스트 종료",
            SkillTrigger.ShotFired => "일반 공격 발사",
            SkillTrigger.Hit => "일반 공격 명중",
            SkillTrigger.CriticalHit => "치명타 명중",
            SkillTrigger.WeakPointHit => "약점 명중",
            SkillTrigger.FullChargeHit => "풀 차지 공격 명중",
            SkillTrigger.LastBulletFired => "마지막 탄환 발사",
            SkillTrigger.LastBulletHit => "마지막 탄환 명중",
            SkillTrigger.Reloaded => "재장전 완료",
            SkillTrigger.Kill => "적 처치",
            SkillTrigger.Damaged => "피격",
            SkillTrigger.HealthBelow => "체력이 기준 이하로 감소",
            SkillTrigger.Healed => "회복 효과 받음",
            SkillTrigger.StatusGained => "상태효과 획득 또는 중첩 증가",
            SkillTrigger.StatusMaxStacks => "상태효과 최대 중첩 도달",
            SkillTrigger.StatusExpired => "상태효과 시간 만료",
            BurstCondition.Any => "제한 없음",
            BurstCondition.FullBurstOnly => "풀 버스트 중",
            BurstCondition.NotFullBurst => "풀 버스트 외",
            SkillEffectKind.ApplyStatus => "상태효과 부여",
            SkillEffectKind.Damage => "피해",
            SkillEffectKind.Heal => "회복",
            SkillEffectKind.RefillAmmo => "탄창 채우기",
            SkillEffectKind.RemoveStatus => "상태효과 제거",
            SkillTarget.Self => "자신",
            SkillTarget.AllAllies => "아군 전체",
            SkillTarget.LowestHealthAlly => "체력 비율이 가장 낮은 아군",
            SkillTarget.FocusEnemy => "조준 중이거나 가장 가까운 적",
            SkillTarget.NearestEnemies => "가까운 적 N기",
            SkillTarget.AllEnemies => "적 전체",
            SkillTarget.RandomEnemies => "무작위 적 N기",
            SkillTarget.TriggerEnemy => "발동을 일으킨 적",
            SkillTarget.TriggerAlly => "발동을 일으킨 아군",
            StatusRule.Stack => "중첩",
            StatusRule.Refresh => "지속 시간 갱신",
            StatusRule.Highest => "가장 강한 효과 적용",
            StatusRule.DamageOverTime => "지속 피해",
            StatusRule.Control => "행동 불가",
            StatusRule.Marker => "표식",
            StatKind.None => "능력치 변경 없음",
            StatKind.AttackUp => "공격력 증가",
            StatKind.CritChanceUp => "치명 확률 증가",
            StatKind.CritBonusUp => "치명 항목 증가",
            StatKind.WeakPointBonusUp => "약점 항목 증가",
            StatKind.DamageTakenUp => "받는 피해 증가",
            StatKind.Slow => "이동 속도 감소",
            StatKind.DamageUp => "주는 피해 증가",
            _ => "정의되지 않은 값",
        };

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

            string label = def.Trigger + " (" + when + ")";
            return conditions.Count > 0 ? label + "\n" + string.Join(", ", conditions) : label;
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
                    string what = EnumLabel(effect.Kind);
                    if (effect.Kind == SkillEffectKind.ApplyStatus || effect.Kind == SkillEffectKind.RemoveStatus) what += " " + Name(effect.Status);
                    if (effect.Kind == SkillEffectKind.Damage || effect.Kind == SkillEffectKind.Heal) what += " " + effect.Coefficient;
                    if (effect.Kind == SkillEffectKind.Damage && effect.CoreHit) what += " (코어 명중)";
                    string target = EnumLabel(effect.Target).Replace("N기", effect.Count + "기");
                    if (effect.TargetStatus != null) target += " 중 " + effect.TargetStatus + " 상태";
                    lines.Add("  " + (lines.Count + 1) + ". " + what + " -> " + target);
                }
            }
            return lines.Count > 0 ? string.Join("\n", lines) : "  (효과 없음)";
        }
    }
}
