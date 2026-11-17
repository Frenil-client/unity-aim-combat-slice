using System.Collections.Generic;
using System.IO;
using System.Linq;
using UISystem;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace AimCombat.Content.Editor
{
    /// <summary>
    /// Addressables 그룹과 주소를 프로젝트 규칙(ContentAddress)에 맞춘다. 여러 번 눌러도 결과가 같다.
    /// 그룹은 모두 앱에 들어가는 로컬 그룹이다(정적 콘텐츠). 원격 배포가 필요해지면 그 그룹만 원격 경로로 바꾼다.
    ///
    /// | 그룹 | 담는 것 | 주소 |
    /// |---|---|---|
    /// | Scenes | 전투, 사격장 씬(파일 항목) | 씬 이름 |
    /// | Characters | 캐릭터 프리팹, 스킬 구성, 얼굴과 컷인 그림 | Characters/이름[/Data, /Face, /CutIn] |
    /// | Enemies | 적 프리팹(돌격형, 원거리형, 보스, 적 탄환) | Enemies/이름 |
    /// | Data | 스킬, 상태효과, 적 데이터, 타격감 데이터, 연출 Timeline | Data/폴더/파일 이름 |
    /// | UI | UI 시스템 팝업 프리팹 | UI/뷰 타입 전체 이름 |
    /// | Shared | 여러 번들이 함께 쓰는 글꼴, 셰이더, HUD 그림, 배경, 효과(폴더 항목) | Shared/폴더 |
    ///
    /// Shared 를 따로 두는 이유: 그룹에 넣지 않은 공유 에셋은 그것을 쓰는 번들마다 복사된다(나눔고딕 글꼴만 15MB).
    /// Resources(UIBootstrap)가 가리키는 UI 설정(Data/UI)과 UIRoot 는 UI 시스템의 약속대로 Resources 쪽에 두고 그룹에 넣지 않는다.
    /// 빌드 설정에는 시작 씬(Boot)만 남긴다. Addressables 씬을 빌드 설정에도 두면 씬과 그 의존성이 두 번 들어간다.
    /// </summary>
    public static class ContentGroupSync
    {
        private const string Root = "Assets/_Project";
        public const string BootScenePath = "Assets/Scenes/Boot.unity";
        private static readonly string[] SceneFiles = { "Assets/Scenes/Battle.unity", "Assets/Scenes/SampleScene.unity" };
        private static readonly string[] Characters = { "Charge", "Rapid", "Shotgun" };
        private static readonly string[] DataFolders = { "Skills", "Statuses", "Enemies", "Feel" };
        private static readonly string[] SharedFolders = { "Art/Fonts", "Art/Shaders", "Art/UI", "Art/Environment", "Art/VFX" };

        [MenuItem("Tools/Aim Combat/Addressables 그룹 동기화")]
        public static void Sync()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            int changed = 0;

            changed += Fill(settings, "Scenes", "scene", SceneFiles.Select(p => (p, Path.GetFileNameWithoutExtension(p))));

            var characters = new List<(string, string)>();
            foreach (string name in Characters)
            {
                characters.Add(($"{Root}/Prefabs/Characters/{name}.prefab", ContentAddress.Character(name)));
                characters.Add(($"{Root}/Data/Characters/Character_{name}.asset", ContentAddress.CharacterData(name)));
                characters.Add(($"{Root}/Art/Characters/{name}/{name}_Face.png", ContentAddress.CharacterFace(name)));
                characters.Add(($"{Root}/Art/Characters/{name}/{name}_CutIn.png", ContentAddress.CharacterCutIn(name)));
            }
            changed += Fill(settings, "Characters", "character", characters);

            changed += Fill(settings, "Enemies", "enemy", Assets($"{Root}/Prefabs/Enemies", "*.prefab")
                .Select(p => (p, ContentAddress.Enemy(Path.GetFileNameWithoutExtension(p)))));

            var data = new List<(string, string)>();
            foreach (string folder in DataFolders)
            {
                foreach (string path in Assets($"{Root}/Data/{folder}", "*.asset").Concat(Assets($"{Root}/Data/{folder}", "*.playable")))
                    data.Add((path, ContentAddress.Data(folder, Path.GetFileNameWithoutExtension(path))));
            }
            changed += Fill(settings, "Data", "data", data);

            var views = new List<(string, string)>();
            foreach (string path in Assets($"{Root}/Prefabs/UI/Popups", "*.prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                UIBase view = prefab != null ? prefab.GetComponent<UIBase>() : null;
                if (view != null) views.Add((path, ContentAddress.UIView(view.GetType())));
            }
            changed += Fill(settings, "UI", "ui", views);

            changed += Fill(settings, "Shared", "shared", SharedFolders.Select(f => ($"{Root}/{f}", "Shared/" + Path.GetFileName(f))));

            KeepOnlyBootInBuildSettings();
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();
            Debug.Log("[ContentGroupSync] Addressables 그룹 동기화: 바뀐 항목 " + changed);
        }

        // 그룹을 만들거나 찾아 로컬 정적 그룹으로 맞추고, 원하는 항목만 남긴다.
        private static int Fill(AddressableAssetSettings settings, string groupName, string label, IEnumerable<(string path, string address)> wanted)
        {
            AddressableAssetGroup group = settings.FindGroup(groupName)
                ?? settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            var bundled = group.GetSchema<BundledAssetGroupSchema>() ?? group.AddSchema<BundledAssetGroupSchema>();
            bundled.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
            bundled.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
            bundled.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            var update = group.GetSchema<ContentUpdateGroupSchema>() ?? group.AddSchema<ContentUpdateGroupSchema>();
            update.StaticContent = true;
            settings.AddLabel(label);

            int changed = 0;
            var keep = new HashSet<string>();
            foreach ((string path, string address) in wanted)
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning("[ContentGroupSync] 없는 에셋이라 건너뛴다: " + path);
                    continue;
                }
                keep.Add(guid);
                AddressableAssetEntry entry = settings.FindAssetEntry(guid);
                if (entry == null || entry.parentGroup != group)
                {
                    entry = settings.CreateOrMoveEntry(guid, group, false, false);
                    changed++;
                }
                if (entry.address != address)
                {
                    entry.SetAddress(address, false);
                    changed++;
                }
                if (!entry.labels.Contains(label)) entry.SetLabel(label, true, false, false);
            }

            foreach (AddressableAssetEntry stale in group.entries.Where(e => !keep.Contains(e.guid)).ToList())
            {
                group.RemoveAssetEntry(stale, false);
                changed++;
            }
            return changed;
        }

        private static IEnumerable<string> Assets(string folder, string pattern)
        {
            if (!Directory.Exists(folder)) return Enumerable.Empty<string>();
            return Directory.GetFiles(folder, pattern, SearchOption.TopDirectoryOnly).Select(p => p.Replace('\\', '/')).OrderBy(p => p);
        }

        private static void KeepOnlyBootInBuildSettings()
        {
            if (!File.Exists(BootScenePath)) return;
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 1 && scenes[0].path == BootScenePath && scenes[0].enabled) return;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootScenePath, true) };
        }
    }
}
