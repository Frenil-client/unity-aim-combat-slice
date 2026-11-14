using System.Collections.Generic;
using AimCombat.Core.Combat;
using UnityEditor;
using UnityEngine;

namespace AimCombat.Data.Editors
{
    /// <summary>
    /// 스킬 에셋 인스펙터. 기본 필드 아래에 Core 의 SkillValidator 결과를 보여준다.
    /// 실행 시 로드와 같은 규칙이라 여기서 통과하면 게임에서도 같은 판정을 받는다.
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
            EditorGUILayout.LabelField("실행 순서", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(CombatDataEditorUtil.DescribeOrder(def), MessageType.None);
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
        private static readonly SkillEffectKind[] Stages =
        {
            SkillEffectKind.ApplyStatus, SkillEffectKind.Damage, SkillEffectKind.Heal, SkillEffectKind.RefillAmmo,
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

        // SkillRunner 와 같은 단계 순서로 효과를 나열한다. 데이터 순서와 실제 실행 순서가 다를 수 있음을 눈으로 보여준다.
        public static string DescribeOrder(SkillDef def)
        {
            var lines = new List<string>();
            foreach (SkillEffectKind stage in Stages)
            {
                foreach (SkillEffectDef effect in def.Effects)
                {
                    if (effect.Kind != stage) continue;
                    string what = effect.Kind == SkillEffectKind.ApplyStatus
                        ? "상태효과 " + (effect.Status != null ? effect.Status.ToString() : "(없음)")
                        : effect.Kind + " " + effect.Coefficient;
                    string target = effect.Target == SkillTarget.NearestEnemies ? "가까운 적 " + effect.Count + "기" : effect.Target.ToString();
                    lines.Add((lines.Count + 1) + ". " + what + " -> " + target);
                }
            }
            return lines.Count > 0 ? string.Join("\n", lines) : "(효과 없음)";
        }
    }
}
