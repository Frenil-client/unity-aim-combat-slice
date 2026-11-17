using System;
using System.Collections.Generic;
using System.Threading;
using AddressablesDelivery.Loading;
using UISystem;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// UI 시스템(com.frenil.uisystem)의 프리팹 공급원을 Addressables 로 바꾼다. 뷰 타입 이름으로 만든 주소(UI/타입 전체 이름)로 불러온다.
    /// - 같은 뷰를 다시 열면 이미 불러온 프리팹을 그대로 쓴다.
    /// - UIManager 가 정리할 때(ReleaseAll) 붙잡은 핸들을 모두 놓는다. 직접 참조 표(UIPrefabTable)와 달리 여기서 놓지 않으면 번들이 내려가지 않는다.
    /// UIBootstrap(Resources)이 이 에셋을 가리키므로, 팝업 프리팹은 Resources 의존성으로 끌려 들어가지 않고 UI 그룹 번들에만 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "UIPrefabProvider", menuName = "Aim Combat/Addressables UI Prefab Provider")]
    public sealed class AddressablesUIPrefabProvider : ScriptableObject, IUIPrefabProvider
    {
        private readonly Dictionary<Type, GameObject> loaded = new Dictionary<Type, GameObject>();
        private AssetHandleSet handles;

        public async Awaitable<GameObject> LoadPrefabAsync(Type viewType, CancellationToken cancellationToken = default)
        {
            if (loaded.TryGetValue(viewType, out GameObject cached) && cached != null) return cached;

            handles ??= new AssetHandleSet("UI");
            AssetHandleSet owner = handles;
            string address = ContentAddress.UIView(viewType);
            GameObject prefab = await owner.LoadAsync<GameObject>(address);
            cancellationToken.ThrowIfCancellationRequested();
            if (prefab == null) throw new KeyNotFoundException("[UI] 프리팹을 불러오지 못했다: " + address);

            // 기다리는 사이 ReleaseAll 로 묶음이 바뀌었으면 이 결과는 이미 반환됐다.
            if (owner == handles) loaded[viewType] = prefab;
            return prefab;
        }

        public void ReleaseAll()
        {
            handles?.Dispose();
            handles = null;
            loaded.Clear();
        }

        // 에디터에서 플레이를 멈춰도 ScriptableObject 는 남으므로 다음 플레이에 지난 핸들을 들고 가지 않게 비운다.
        private void OnDisable() => ReleaseAll();
    }
}
