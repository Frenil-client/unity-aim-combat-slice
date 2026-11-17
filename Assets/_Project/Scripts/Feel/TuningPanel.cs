using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AimCombat.Data;
using AimCombat.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AimCombat.Feel
{
    /// <summary>
    /// 타격감 튜닝 패널(SPEC 4절). F1 로 연다. 실행 중에 무기별 프리셋과 공통 설정의 값을 슬라이더로 바꾸면 바로 반영되고,
    /// 저장하면 데이터 에셋에 쓴다(에디터). 빌드에서는 같은 내용을 기기 저장소의 JSON 으로 쓰고 다음 실행 때 읽는다.
    /// - 값 목록은 에셋의 공개 필드를 리플렉션으로 읽어 만든다. Range 가 붙은 숫자는 슬라이더, bool 은 토글, Color 는 RGB 슬라이더.
    ///   그래서 프리셋에 필드를 더해도 패널 코드를 고치지 않는다.
    /// - 되돌리기는 패널을 연 순간(또는 마지막 저장)의 값으로 돌린다.
    /// - 패널 위를 누른 것은 사격으로 치지 않는다.
    /// 개발용 도구라 즉시 모드 GUI 로 그린다. 화면의 짧은 변 1080 을 기준으로 화면에 맞춰 키운다(세로 화면은 폭).
    /// </summary>
    public sealed class TuningPanel : MonoBehaviour
    {
        private const float ReferenceHeight = 1080f;

        [SerializeField] private FeelDirector director;
        [SerializeField] private bool openAtStart;

        private bool open;
        private int tab;
        private Vector2 scroll;
        private readonly Dictionary<ScriptableObject, string> snapshots = new Dictionary<ScriptableObject, string>();
        private readonly List<ScriptableObject> assets = new List<ScriptableObject>();
        private readonly List<string> tabNames = new List<string>();
        private Rect panelRect;
        private string status = "";
        private GUIStyle label;
        private GUIStyle title;

        private void Awake()
        {
            for (int i = 0; i < director.PresetCount; i++)
            {
                FeelPreset preset = director.PresetAt(i);
                if (preset == null) continue;
                assets.Add(preset);
                tabNames.Add(preset.name.Replace("Feel_", ""));
            }
            if (director.Settings != null)
            {
                assets.Add(director.Settings);
                tabNames.Add("공통");
            }

#if !UNITY_EDITOR
            foreach (ScriptableObject asset in assets) LoadFromDevice(asset);
#endif
            open = openAtStart;
            if (open) TakeSnapshots();
            CombatInput.ExtraUiHitTest = IsOverPanel;
        }

        private void OnDestroy()
        {
            if (CombatInput.ExtraUiHitTest == (System.Predicate<Vector2>)IsOverPanel) CombatInput.ExtraUiHitTest = null;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.f1Key.wasPressedThisFrame) return;
            open = !open;
            if (open) TakeSnapshots();
        }

        // 화면 좌표(왼쪽 아래 원점)가 패널 위인지. 입력 계층이 사격 판정에서 뺀다.
        private bool IsOverPanel(Vector2 screenPoint)
        {
            if (!open) return false;
            float s = Mathf.Min(Screen.width, Screen.height) / ReferenceHeight;
            var guiPoint = new Vector2(screenPoint.x / s, (Screen.height - screenPoint.y) / s);
            return panelRect.Contains(guiPoint);
        }

        private void OnGUI()
        {
            if (!open || assets.Count == 0) return;
            EnsureStyles();

            float s = Mathf.Min(Screen.width, Screen.height) / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            panelRect = new Rect(20f, 140f, 560f, 820f);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, panelRect.height - 20f));
            GUILayout.Label("타격감 튜닝 (F1 닫기, F2 연출 켜고 끄기)", title);

            director.FeelEnabled = GUILayout.Toggle(director.FeelEnabled, " 연출 켜기", GUILayout.Height(28f));
            tab = GUILayout.Toolbar(tab, tabNames.ToArray(), GUILayout.Height(34f));

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(600f));
            DrawFields(assets[tab]);
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("저장", GUILayout.Height(40f))) Save(assets[tab]);
            if (GUILayout.Button("되돌리기", GUILayout.Height(40f))) Revert(assets[tab]);
            GUILayout.EndHorizontal();
            GUILayout.Label(status, label);
            GUILayout.EndArea();
        }

        // 공개 필드를 순서대로 그린다. Header 가 있으면 소제목을 먼저 그린다.
        private void DrawFields(ScriptableObject asset)
        {
            foreach (FieldInfo field in asset.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null) GUILayout.Label(header.header, title);

                var range = field.GetCustomAttribute<RangeAttribute>();
                string name = field.Name;
                if (field.FieldType == typeof(float))
                {
                    float value = (float)field.GetValue(asset);
                    float min = range != null ? range.min : 0f;
                    float max = range != null ? range.max : Mathf.Max(1f, value * 2f);
                    field.SetValue(asset, Slider(name, value, min, max, "0.000"));
                }
                else if (field.FieldType == typeof(int))
                {
                    int value = (int)field.GetValue(asset);
                    float min = range != null ? range.min : 0f;
                    float max = range != null ? range.max : Mathf.Max(10f, value * 2f);
                    field.SetValue(asset, Mathf.RoundToInt(Slider(name, value, min, max, "0")));
                }
                else if (field.FieldType == typeof(bool))
                {
                    field.SetValue(asset, GUILayout.Toggle((bool)field.GetValue(asset), " " + name, GUILayout.Height(26f)));
                }
                else if (field.FieldType == typeof(Color))
                {
                    Color c = (Color)field.GetValue(asset);
                    GUILayout.Label(name, label);
                    c.r = Slider("  R", c.r, 0f, 1f, "0.00");
                    c.g = Slider("  G", c.g, 0f, 1f, "0.00");
                    c.b = Slider("  B", c.b, 0f, 1f, "0.00");
                    field.SetValue(asset, c);
                }
            }
        }

        private float Slider(string name, float value, float min, float max, string format)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, label, GUILayout.Width(210f));
            float next = GUILayout.HorizontalSlider(value, min, max, GUILayout.Width(220f), GUILayout.Height(24f));
            GUILayout.Label(next.ToString(format), label, GUILayout.Width(70f));
            GUILayout.EndHorizontal();
            return next;
        }

        private void TakeSnapshots()
        {
            snapshots.Clear();
            foreach (ScriptableObject asset in assets) snapshots[asset] = JsonUtility.ToJson(asset);
            status = "";
        }

        private void Revert(ScriptableObject asset)
        {
            if (!snapshots.TryGetValue(asset, out string json)) return;
            JsonUtility.FromJsonOverwrite(json, asset);
            status = asset.name + " 되돌림";
        }

        private void Save(ScriptableObject asset)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(asset);
            status = asset.name + " 에셋에 저장";
#else
            string path = DevicePath(asset);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(asset, true));
            status = asset.name + " 기기에 저장";
#endif
            snapshots[asset] = JsonUtility.ToJson(asset);
        }

        private static string DevicePath(ScriptableObject asset) => Path.Combine(Application.persistentDataPath, "feel", asset.name + ".json");

        private static void LoadFromDevice(ScriptableObject asset)
        {
            string path = DevicePath(asset);
            if (File.Exists(path)) JsonUtility.FromJsonOverwrite(File.ReadAllText(path), asset);
        }

        private void EnsureStyles()
        {
            if (label != null) return;
            label = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            title = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
        }
    }
}
