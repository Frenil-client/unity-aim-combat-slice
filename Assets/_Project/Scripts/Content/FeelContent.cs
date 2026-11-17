using AddressablesDelivery.Loading;
using AimCombat.Data;
using AimCombat.Feel;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>타격감 데이터(공통 설정, 무기별 프리셋)를 주소로 불러와 연출 지휘(FeelDirector)에 넘긴다. 프리셋 순서는 무기 종류 순서다.</summary>
    public sealed class FeelContent : ContentStep
    {
        [SerializeField] private FeelDirector feel;
        [Tooltip("무기 종류 순서(연사형, 차지형, 산탄형)의 프리셋 이름. 주소 Data/Feel/Feel_이름.")]
        [SerializeField] private string[] presets = { "Rapid", "Charge", "Shotgun" };

        public override async Awaitable LoadAsync(AssetHandleSet handles)
        {
            FeelSettings settings = await Require<FeelSettings>(handles, ContentAddress.FeelSettings);
            var loaded = new FeelPreset[presets.Length];
            for (int i = 0; i < presets.Length; i++) loaded[i] = await Require<FeelPreset>(handles, ContentAddress.FeelPreset(presets[i]));
            feel.SetData(settings, loaded);
        }
    }
}
