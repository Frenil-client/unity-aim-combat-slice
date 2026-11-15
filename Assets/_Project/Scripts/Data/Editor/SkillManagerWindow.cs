using System.Collections.Generic;
using AimCombat.Core.Combat;
using UnityEditor;
using UnityEngine;

namespace AimCombat.Data.Editors
{
    /// <summary>
    /// 스킬 관리 창(Aim Combat/스킬 관리). 캐릭터를 고르면 스킬 1, 스킬 2, 버스트 스킬 세 칸을 보여 주고,
    /// 스킬마다 발동 블록(트리거, 조건)과 효과를 더하거나 빼고 값을 고칠 수 있다.
    /// - 고친 내용은 창 안에만 머물다가 "저장"을 누를 때 에셋에 적용된다. "되돌리기"는 저장하지 않은 수정을 버린다.
    /// - 검증과 발동 요약은 저장 전의 수정 내용을 기준으로 보여 준다. 규칙은 Core 의 SkillValidator 하나다.
    /// - "전체 표"는 모든 캐릭터의 발동 블록을 한 표로 나열한다. 줄을 누르면 그 캐릭터의 편집 화면으로 간다.
    /// </summary>
    public sealed class SkillManagerWindow : EditorWindow
    {
        private const string SkillFolder = "Assets/_Project/Data/Skills";
        private const string CharacterFolder = "Assets/_Project/Data/Characters";

        private static readonly SkillSlot[] Slots = { SkillSlot.Skill1, SkillSlot.Skill2, SkillSlot.Burst };
        private static readonly string[] SlotLabels = { "스킬 1", "스킬 2", "버스트 스킬" };
        private static readonly string[] SlotProperties = { "skill1", "skill2", "burstSkill" };
        private static readonly string[] SlotFileSuffix = { "1", "2", "Burst" };

        // 저장 전 수정 내용으로 만든 미리보기. 검증 결과와 요약 문장에 쓴다.
        private sealed class Preview
        {
            public SkillDef Def;
            public List<string> Errors;
        }

        private readonly List<CharacterData> characters = new List<CharacterData>();
        private readonly Dictionary<Object, SerializedObject> objects = new Dictionary<Object, SerializedObject>();
        private readonly Dictionary<SkillData, Preview> previews = new Dictionary<SkillData, Preview>();
        private readonly bool[] slotOpen = { true, true, true };
        private CharacterData selected;
        private bool tableView;
        private Vector2 listScroll;
        private Vector2 detailScroll;
        private Vector2 tableScroll;
        private GUIStyle wrapLabel;

        [MenuItem("Aim Combat/스킬 관리")]
        public static void Open()
        {
            GetWindow<SkillManagerWindow>("스킬 관리").minSize = new Vector2(820f, 480f);
        }

        public static void Open(CharacterData character)
        {
            Open();
            var window = GetWindow<SkillManagerWindow>("스킬 관리");
            window.ReloadCharacters();
            window.selected = character;
            window.tableView = false;
        }

        private void OnEnable()
        {
            saveChangesMessage = "스킬 관리 창에 저장하지 않은 수정이 있습니다.";
            ReloadCharacters();
        }

        private void OnProjectChange()
        {
            ReloadCharacters();
            Repaint();
        }

        private void ReloadCharacters()
        {
            characters.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:CharacterData"))
            {
                var character = AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                if (character != null) characters.Add(character);
            }
            characters.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            if (selected == null && characters.Count > 0) selected = characters[0];
        }

        // ---- 수정 내용 보관 ----

        // 에셋마다 SerializedObject 를 하나씩 들고 있는다. 수정은 여기에만 쌓이고 저장할 때 에셋에 적용한다.
        private SerializedObject ObjectOf(Object asset)
        {
            if (!objects.TryGetValue(asset, out SerializedObject so) || so.targetObject == null)
            {
                so = new SerializedObject(asset);
                objects[asset] = so;
            }
            return so;
        }

        private bool IsDirty(Object asset)
        {
            return asset != null && objects.TryGetValue(asset, out SerializedObject so) && so.targetObject != null && so.hasModifiedProperties;
        }

        private bool AnyDirty()
        {
            foreach (SerializedObject so in objects.Values) if (so.targetObject != null && so.hasModifiedProperties) return true;
            return false;
        }

        // 수정하지 않은 에셋은 바깥(인스펙터 등)에서 바뀐 값을 따라가게 한다. 수정 중인 에셋은 건드리지 않는다.
        private void SyncUntouched()
        {
            List<Object> gone = null;
            foreach (KeyValuePair<Object, SerializedObject> pair in objects)
            {
                if (pair.Key == null || pair.Value.targetObject == null)
                {
                    (gone ?? (gone = new List<Object>())).Add(pair.Key);
                    continue;
                }
                if (!pair.Value.hasModifiedProperties && pair.Value.UpdateIfRequiredOrScript() && pair.Key is SkillData skill) previews.Remove(skill);
            }
            if (gone == null) return;
            foreach (Object key in gone) objects.Remove(key); // 지워진 에셋. 다른 에셋의 수정은 그대로 둔다.
            previews.Clear();
        }

        public override void SaveChanges()
        {
            foreach (SerializedObject so in objects.Values)
            {
                if (so.targetObject == null || !so.hasModifiedProperties) continue;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
            }
            AssetDatabase.SaveAssets();
            previews.Clear();
            base.SaveChanges();
        }

        public override void DiscardChanges()
        {
            foreach (SerializedObject so in objects.Values) if (so.targetObject != null) so.Update();
            previews.Clear();
            base.DiscardChanges();
        }

        // 저장 전 수정 내용을 임시 복사본에 옮겨 Core 정의를 만들고 검증한다. 에셋 자체는 건드리지 않는다.
        private Preview PreviewOf(SkillData skill)
        {
            if (previews.TryGetValue(skill, out Preview preview)) return preview;

            SkillData temp = Instantiate(skill);
            temp.name = skill.name;
            temp.hideFlags = HideFlags.HideAndDontSave;
            var tempObject = new SerializedObject(temp);
            SerializedProperty property = ObjectOf(skill).GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath != "m_Script") tempObject.CopyFromSerializedProperty(property);
            }
            tempObject.ApplyModifiedPropertiesWithoutUndo();

            SkillDef def = temp.BuildFresh();
            preview = new Preview { Def = def, Errors = SkillValidator.Validate(def) };
            DestroyImmediate(temp);
            previews[skill] = preview;
            return preview;
        }

        private SkillData SlotSkill(CharacterData character, int slot)
        {
            return ObjectOf(character).FindProperty(SlotProperties[slot]).objectReferenceValue as SkillData;
        }

        // 캐릭터 한 명의 검증 결과(저장 전 수정 기준). CharacterData.Validate 와 같은 규칙이다.
        private List<string> ErrorsOf(CharacterData character)
        {
            var errors = new List<string>();
            for (int i = 0; i < Slots.Length; i++)
            {
                SkillData skill = SlotSkill(character, i);
                if (skill == null) continue;
                Preview preview = PreviewOf(skill);
                if (preview.Def.Slot != Slots[i]) errors.Add(SlotLabels[i] + ": " + skill.name + " 은 " + preview.Def.Slot + " 칸용 스킬이다.");
                foreach (string error in preview.Errors) errors.Add(SlotLabels[i] + ": " + error);
            }
            SkillData first = SlotSkill(character, 0);
            if (first != null && first == SlotSkill(character, 1)) errors.Add("스킬 1과 스킬 2가 같은 에셋이다.");
            return errors;
        }

        private bool CharacterDirty(CharacterData character)
        {
            if (IsDirty(character)) return true;
            for (int i = 0; i < Slots.Length; i++) if (IsDirty(SlotSkill(character, i))) return true;
            return false;
        }

        // ---- 화면 ----

        private void OnGUI()
        {
            if (wrapLabel == null) wrapLabel = new GUIStyle(EditorStyles.label) { wordWrap = true };
            characters.RemoveAll(c => c == null);
            if (selected == null && characters.Count > 0) selected = characters[0];
            SyncUntouched();

            DrawToolbar();

            EditorGUI.BeginChangeCheck();
            if (tableView)
            {
                DrawTable();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawCharacterList();
                DrawDetail();
                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck()) previews.Clear();

            hasUnsavedChanges = AnyDirty();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            tableView = GUILayout.Toggle(tableView, "전체 표", EditorStyles.toolbarButton, GUILayout.Width(70f));
            if (GUILayout.Button("새 캐릭터", EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                CreateCharacter();
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button("새로 고침", EditorStyles.toolbarButton, GUILayout.Width(70f))) ReloadCharacters();
            GUILayout.FlexibleSpace();

            bool dirty = AnyDirty();
            GUILayout.Label(dirty ? "저장하지 않은 수정이 있습니다" : "모두 저장됨", EditorStyles.miniLabel);
            using (new EditorGUI.DisabledScope(!dirty))
            {
                if (GUILayout.Button("되돌리기", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    GUI.FocusControl(null);
                    DiscardChanges();
                }
                if (GUILayout.Button("저장", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    GUI.FocusControl(null);
                    SaveChanges();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCharacterList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200f));
            EditorGUILayout.LabelField("캐릭터", EditorStyles.boldLabel);
            listScroll = EditorGUILayout.BeginScrollView(listScroll);
            foreach (CharacterData character in characters)
            {
                string label = CharacterName(character) + (CharacterDirty(character) ? " *" : "");
                if (ErrorsOf(character).Count > 0) label += "  (오류)";
                bool isSelected = character == selected;
                if (GUILayout.Toggle(isSelected, label, "Button") && !isSelected)
                {
                    selected = character;
                    GUI.FocusControl(null);
                }
            }
            if (characters.Count == 0) EditorGUILayout.HelpBox("캐릭터 에셋이 없습니다. '새 캐릭터'로 만드세요.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private string CharacterName(CharacterData character)
        {
            string displayName = ObjectOf(character).FindProperty("displayName").stringValue;
            return string.IsNullOrEmpty(displayName) ? character.name : displayName + "  (" + character.name + ")";
        }

        private void DrawDetail()
        {
            EditorGUILayout.BeginVertical();
            detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
            if (selected == null)
            {
                EditorGUILayout.HelpBox("왼쪽에서 캐릭터를 고르세요.", MessageType.Info);
            }
            else
            {
                SerializedObject character = ObjectOf(selected);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(selected.name, EditorStyles.boldLabel);
                if (GUILayout.Button("에셋 보기", GUILayout.Width(80f))) EditorGUIUtility.PingObject(selected);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(character.FindProperty("displayName"), new GUIContent("표시 이름"));

                for (int i = 0; i < Slots.Length; i++)
                {
                    EditorGUILayout.Space();
                    DrawSlot(character, i);
                }

                List<string> errors = ErrorsOf(selected);
                EditorGUILayout.Space();
                if (errors.Count == 0) EditorGUILayout.HelpBox("검증 통과", MessageType.Info);
                else EditorGUILayout.HelpBox(string.Join("\n", errors), MessageType.Error);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSlot(SerializedObject character, int slot)
        {
            SerializedProperty slotProperty = character.FindProperty(SlotProperties[slot]);
            var skill = slotProperty.objectReferenceValue as SkillData;
            string title = SlotLabels[slot] + "   " + (skill != null ? SkillName(skill) + (IsDirty(skill) ? " *" : "") : "(비어 있음)");

            slotOpen[slot] = EditorGUILayout.BeginFoldoutHeaderGroup(slotOpen[slot], title);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (!slotOpen[slot]) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(slotProperty, new GUIContent("스킬 에셋"));
            if (skill == null && GUILayout.Button("새로 만들기", GUILayout.Width(90f)))
            {
                slotProperty.objectReferenceValue = CreateSkill(slot);
                previews.Clear();
                GUIUtility.ExitGUI(); // 에셋을 만들면 화면 구성이 바뀐다. 이번 그리기는 여기서 끝낸다.
            }
            EditorGUILayout.EndHorizontal();

            if (skill == null)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            SerializedObject so = ObjectOf(skill);
            EditorGUILayout.PropertyField(so.FindProperty("displayName"), new GUIContent("이름"));
            EditorGUILayout.PropertyField(so.FindProperty("description"), new GUIContent("설명"));

            SerializedProperty slotValue = so.FindProperty("slot");
            if (slotValue.enumValueIndex != (int)Slots[slot])
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("이 스킬은 " + (SkillSlot)slotValue.enumValueIndex + " 칸용으로 표시돼 있습니다.", MessageType.Warning);
                if (GUILayout.Button("이 칸에 맞추기", GUILayout.Width(100f), GUILayout.Height(38f))) slotValue.enumValueIndex = (int)Slots[slot];
                EditorGUILayout.EndHorizontal();
            }

            if (Slots[slot] == SkillSlot.Burst)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(so.FindProperty("burstStage"), new GUIContent("버스트 단계"));
                EditorGUILayout.PropertyField(so.FindProperty("cooldown"), new GUIContent("쿨다운(초)"));
                EditorGUILayout.PropertyField(so.FindProperty("nextStageWindow"), new GUIContent("다음 단계 창(초)"));
                EditorGUILayout.EndHorizontal();
            }

            SerializedProperty blocks = so.FindProperty("blocks");
            Preview preview = PreviewOf(skill);
            int remove = -1;
            int moveFrom = -1;
            int moveTo = -1;
            for (int b = 0; b < blocks.arraySize; b++)
            {
                SkillBlockDef summary = preview.Def.Blocks.Count == blocks.arraySize ? preview.Def.Blocks[b] : null;
                int action = DrawBlock(blocks.GetArrayElementAtIndex(b), b, blocks.arraySize, summary, preview.Def);
                if (action == 0) remove = b;
                else if (action == -1 || action == 1)
                {
                    moveFrom = b;
                    moveTo = b + action;
                }
            }
            if (remove >= 0) blocks.DeleteArrayElementAtIndex(remove);
            if (moveFrom >= 0) blocks.MoveArrayElement(moveFrom, moveTo);

            if (GUILayout.Button("+ 발동 추가", GUILayout.Width(120f)))
            {
                blocks.arraySize++;
                ResetBlock(blocks.GetArrayElementAtIndex(blocks.arraySize - 1), Slots[slot]);
            }
            EditorGUILayout.EndVertical();
        }

        private string SkillName(SkillData skill)
        {
            string displayName = ObjectOf(skill).FindProperty("displayName").stringValue;
            return string.IsNullOrEmpty(displayName) ? skill.name : displayName;
        }

        // 발동 블록 하나. 돌려주는 값: 0 = 지우기, -1 = 위로, 1 = 아래로, 그 밖 = 그대로.
        private int DrawBlock(SerializedProperty block, int index, int count, SkillBlockDef summary, SkillDef skill)
        {
            int action = int.MinValue;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            string title = "발동 " + (index + 1) + (summary != null ? "   " + CombatDataEditorUtil.DescribeTrigger(summary, skill) : "");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("▲", GUILayout.Width(26f))) action = -1;
            }
            using (new EditorGUI.DisabledScope(index == count - 1))
            {
                if (GUILayout.Button("▼", GUILayout.Width(26f))) action = 1;
            }
            if (GUILayout.Button("삭제", GUILayout.Width(44f))) action = 0;
            EditorGUILayout.EndHorizontal();

            SerializedProperty triggerProperty = block.FindPropertyRelative("trigger");
            EditorGUILayout.PropertyField(triggerProperty, new GUIContent("트리거"));
            var trigger = (SkillTrigger)triggerProperty.enumValueIndex;
            bool passive = SkillTriggers.IsPassive(trigger);

            // 트리거에 필요한 설정만 보여 준다.
            if (trigger == SkillTrigger.Active) Field(block, "cooldown", "쿨다운(초)");
            if (SkillTriggers.IsCounted(trigger)) Field(block, "triggerCount", "발동 횟수(N회마다)");
            if (trigger == SkillTrigger.Periodic)
            {
                Field(block, "interval", "반복 주기(초)");
                Field(block, "altIntervalStatus", "주기가 바뀌는 상태");
                if (block.FindPropertyRelative("altIntervalStatus").objectReferenceValue != null) Field(block, "altInterval", "그 상태에서의 주기(초)");
            }
            if (SkillTriggers.WatchesStatus(trigger)) Field(block, "triggerStatus", "지켜볼 상태효과");
            if (trigger == SkillTrigger.HealthBelow) Field(block, "healthThreshold", "체력 기준(비율)");

            if (passive)
            {
                EditorGUILayout.LabelField("발동 조건", EditorStyles.miniBoldLabel);
                Field(block, "requireSelfStatus", "자신이 이 상태라면");
                Field(block, "forbidSelfStatus", "자신이 이 상태가 아니라면");
                Field(block, "burstState", "풀 버스트 여부");
                Field(block, "chance", "발동 확률");
                Field(block, "cooldown", "재발동 간격(초)");
            }

            EditorGUILayout.LabelField("효과", EditorStyles.miniBoldLabel);
            SerializedProperty effects = block.FindPropertyRelative("effects");
            int removeEffect = -1;
            for (int e = 0; e < effects.arraySize; e++)
            {
                if (DrawEffect(effects.GetArrayElementAtIndex(e), e)) removeEffect = e;
            }
            if (removeEffect >= 0) effects.DeleteArrayElementAtIndex(removeEffect);
            if (GUILayout.Button("+ 효과 추가", GUILayout.Width(120f)))
            {
                effects.arraySize++;
                ResetEffect(effects.GetArrayElementAtIndex(effects.arraySize - 1));
            }

            if (summary != null && effects.arraySize > 1)
                EditorGUILayout.LabelField("실행 순서\n" + CombatDataEditorUtil.DescribeOrder(summary), wrapLabel);

            EditorGUILayout.EndVertical();
            return action;
        }

        // 효과 한 줄. 종류에 필요한 값만 보여 준다. 지우기를 눌렀으면 true.
        private static bool DrawEffect(SerializedProperty effect, int index)
        {
            SerializedProperty kindProperty = effect.FindPropertyRelative("kind");
            SerializedProperty targetProperty = effect.FindPropertyRelative("target");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label((index + 1) + ".", GUILayout.Width(20f));
            EditorGUILayout.PropertyField(kindProperty, GUIContent.none, GUILayout.Width(110f));
            GUILayout.Label("→", GUILayout.Width(16f));
            EditorGUILayout.PropertyField(targetProperty, GUIContent.none, GUILayout.Width(130f));

            var kind = (SkillEffectKind)kindProperty.enumValueIndex;
            var target = (SkillTarget)targetProperty.enumValueIndex;
            if (target == SkillTarget.NearestEnemies || target == SkillTarget.RandomEnemies) Inline(effect, "count", "수", 22f, 40f);
            if (kind == SkillEffectKind.Damage || kind == SkillEffectKind.Heal) Inline(effect, "coefficient", kind == SkillEffectKind.Damage ? "계수" : "비율", 30f, 55f);
            if (kind == SkillEffectKind.ApplyStatus || kind == SkillEffectKind.RemoveStatus)
                EditorGUILayout.PropertyField(effect.FindPropertyRelative("status"), GUIContent.none, GUILayout.MinWidth(120f));
            if (kind == SkillEffectKind.Damage) Inline(effect, "coreHit", "코어 명중", 56f, 18f);
            GUILayout.FlexibleSpace();
            bool remove = GUILayout.Button("삭제", GUILayout.Width(44f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24f);
            GUILayout.Label("이 상태가 걸린 대상만", GUILayout.Width(130f));
            EditorGUILayout.PropertyField(effect.FindPropertyRelative("targetStatus"), GUIContent.none, GUILayout.MaxWidth(260f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            return remove;
        }

        private static void Field(SerializedProperty parent, string name, string label)
        {
            EditorGUILayout.PropertyField(parent.FindPropertyRelative(name), new GUIContent(label));
        }

        private static void Inline(SerializedProperty parent, string name, string label, float labelWidth, float fieldWidth)
        {
            GUILayout.Label(label, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(parent.FindPropertyRelative(name), GUIContent.none, GUILayout.Width(fieldWidth));
        }

        // 배열을 늘리면 앞 원소가 복사돼 들어온다. 새 블록은 기본값으로 비운다.
        private static void ResetBlock(SerializedProperty block, SkillSlot slot)
        {
            block.FindPropertyRelative("trigger").enumValueIndex = (int)(slot == SkillSlot.Burst ? SkillTrigger.Burst : SkillTrigger.BattleStart);
            block.FindPropertyRelative("cooldown").floatValue = 0f;
            block.FindPropertyRelative("triggerCount").intValue = 1;
            block.FindPropertyRelative("interval").floatValue = 0f;
            block.FindPropertyRelative("altInterval").floatValue = 0f;
            block.FindPropertyRelative("altIntervalStatus").objectReferenceValue = null;
            block.FindPropertyRelative("triggerStatus").objectReferenceValue = null;
            block.FindPropertyRelative("healthThreshold").floatValue = 0.5f;
            block.FindPropertyRelative("chance").floatValue = 1f;
            block.FindPropertyRelative("requireSelfStatus").objectReferenceValue = null;
            block.FindPropertyRelative("forbidSelfStatus").objectReferenceValue = null;
            block.FindPropertyRelative("burstState").enumValueIndex = 0;
            block.FindPropertyRelative("effects").arraySize = 0;
        }

        private static void ResetEffect(SerializedProperty effect)
        {
            effect.FindPropertyRelative("kind").enumValueIndex = 0;
            effect.FindPropertyRelative("target").enumValueIndex = 0;
            effect.FindPropertyRelative("count").intValue = 1;
            effect.FindPropertyRelative("coefficient").floatValue = 0f;
            effect.FindPropertyRelative("status").objectReferenceValue = null;
            effect.FindPropertyRelative("targetStatus").objectReferenceValue = null;
            effect.FindPropertyRelative("coreHit").boolValue = false;
        }

        // ---- 전체 표: 모든 캐릭터의 발동 블록을 한 줄씩 ----

        private void DrawTable()
        {
            const float nameWidth = 110f, slotWidth = 80f, skillWidth = 120f, whenWidth = 300f;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("캐릭터", GUILayout.Width(nameWidth));
            GUILayout.Label("칸", GUILayout.Width(slotWidth));
            GUILayout.Label("스킬", GUILayout.Width(skillWidth));
            GUILayout.Label("발동", GUILayout.Width(whenWidth));
            GUILayout.Label("효과");
            EditorGUILayout.EndHorizontal();

            tableScroll = EditorGUILayout.BeginScrollView(tableScroll);
            foreach (CharacterData character in characters)
            {
                for (int i = 0; i < Slots.Length; i++)
                {
                    SkillData skill = SlotSkill(character, i);
                    Preview preview = skill != null ? PreviewOf(skill) : null;
                    int rows = preview != null && preview.Def.Blocks.Count > 0 ? preview.Def.Blocks.Count : 1;
                    for (int b = 0; b < rows; b++)
                    {
                        SkillBlockDef block = preview != null && b < preview.Def.Blocks.Count ? preview.Def.Blocks[b] : null;
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(i == 0 && b == 0 ? CharacterDisplay(character) : "", EditorStyles.label, GUILayout.Width(nameWidth)))
                        {
                            selected = character;
                            tableView = false;
                        }
                        GUILayout.Label(b == 0 ? SlotLabel(i, preview) : "", GUILayout.Width(slotWidth));
                        GUILayout.Label(b == 0 ? (skill != null ? SkillName(skill) + (IsDirty(skill) ? " *" : "") : "(비어 있음)") : "", GUILayout.Width(skillWidth));
                        GUILayout.Label(block != null ? CombatDataEditorUtil.DescribeTrigger(block, preview.Def) : skill != null ? "(발동 없음)" : "", wrapLabel, GUILayout.Width(whenWidth));
                        GUILayout.Label(block != null ? CombatDataEditorUtil.DescribeOrder(block) : "", wrapLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                List<string> errors = ErrorsOf(character);
                if (errors.Count > 0) EditorGUILayout.HelpBox(string.Join("\n", errors), MessageType.Error);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
            EditorGUILayout.EndScrollView();
        }

        private string CharacterDisplay(CharacterData character)
        {
            string displayName = ObjectOf(character).FindProperty("displayName").stringValue;
            return (string.IsNullOrEmpty(displayName) ? character.name : displayName) + (CharacterDirty(character) ? " *" : "");
        }

        private static string SlotLabel(int slot, Preview preview)
        {
            if (Slots[slot] != SkillSlot.Burst || preview == null) return SlotLabels[slot];
            return "버스트 " + preview.Def.BurstStage + "단계";
        }

        // ---- 에셋 만들기. 만드는 것은 바로 파일로 생기고, 캐릭터 칸에 넣는 것은 저장을 눌러야 적용된다 ----

        private void CreateCharacter()
        {
            EnsureFolder(CharacterFolder);
            var character = CreateInstance<CharacterData>();
            AssetDatabase.CreateAsset(character, AssetDatabase.GenerateUniqueAssetPath(CharacterFolder + "/Character_New.asset"));
            AssetDatabase.SaveAssets();
            ReloadCharacters();
            selected = character;
            tableView = false;
        }

        private SkillData CreateSkill(int slot)
        {
            EnsureFolder(SkillFolder);
            string owner = selected.name.StartsWith("Character_") ? selected.name.Substring("Character_".Length) : selected.name;
            var skill = CreateInstance<SkillData>();
            AssetDatabase.CreateAsset(skill, AssetDatabase.GenerateUniqueAssetPath(SkillFolder + "/Skill_" + owner + "_" + SlotFileSuffix[slot] + ".asset"));

            var so = new SerializedObject(skill);
            so.FindProperty("slot").enumValueIndex = (int)Slots[slot];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skill);
            AssetDatabase.SaveAssets();
            return skill;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }
    }
}
