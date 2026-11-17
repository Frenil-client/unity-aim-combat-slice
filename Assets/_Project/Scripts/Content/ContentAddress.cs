namespace AimCombat.Content
{
    /// <summary>
    /// Addressables 주소 규칙. 그룹 동기화 도구(ContentGroupSync)가 같은 규칙으로 주소를 붙이고, 실행 코드는 여기서만 주소를 만든다.
    /// - 씬: 씬 이름 그대로(Battle, SampleScene).
    /// - 캐릭터: Characters/{이름} 프리팹, Characters/{이름}/Data 스킬 구성, Characters/{이름}/Face, Characters/{이름}/CutIn 그림.
    /// - 적: Enemies/{종류} 프리팹(Rusher, Ranged, Boss).
    /// - 데이터: Data/{폴더}/{파일 이름}. 타격감은 Data/Feel/FeelSettings, Data/Feel/Feel_{무기}.
    /// - UI 팝업: UI/{뷰 타입 전체 이름}. UI 시스템의 프리팹 공급원이 타입으로 찾는다.
    /// </summary>
    public static class ContentAddress
    {
        public const string BattleScene = "Battle";
        public const string RangeScene = "SampleScene";

        public static string Character(string name) => "Characters/" + name;
        public static string CharacterData(string name) => "Characters/" + name + "/Data";
        public static string CharacterFace(string name) => "Characters/" + name + "/Face";
        public static string CharacterCutIn(string name) => "Characters/" + name + "/CutIn";

        public static string Enemy(string kind) => "Enemies/" + kind;

        public static string Data(string folder, string assetName) => "Data/" + folder + "/" + assetName;
        public static string FeelSettings => Data("Feel", "FeelSettings");
        public static string FeelPreset(string weapon) => Data("Feel", "Feel_" + weapon);

        public static string UIView(System.Type viewType) => "UI/" + (viewType.FullName ?? viewType.Name);
    }
}
