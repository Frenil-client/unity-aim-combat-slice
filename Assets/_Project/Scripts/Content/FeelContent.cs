using AimCombat.Data;
using AimCombat.Feel;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 타격감 데이터(공통 설정, 무기별 프리셋)를 연출 지휘(FeelDirector)에 넘긴다. 프리셋 순서는 무기 종류 순서다.
    /// 쓰는 주소: Data/Feel/FeelSettings, Data/Feel/Feel_무기. 씬의 기초 리소스 목록에 있어야 한다.
    /// </summary>
    public sealed class FeelContent : ContentStep
    {
        [SerializeField] private FeelDirector feel;
        [Tooltip("무기 종류 순서(연사형, 차지형, 산탄형)의 프리셋 이름. 주소 Data/Feel/Feel_이름.")]
        [SerializeField] private string[] presets = { "Rapid", "Charge", "Shotgun" };

        public override void Apply()
        {
            FeelSettings settings = Require<FeelSettings>(ContentAddress.FeelSettings);
            var loaded = new FeelPreset[presets.Length];
            for (int i = 0; i < presets.Length; i++) loaded[i] = Require<FeelPreset>(ContentAddress.FeelPreset(presets[i]));
            feel.SetData(settings, loaded);
        }
    }
}
