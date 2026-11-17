using AddressablesDelivery.Loading;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬이 시작할 때 Addressables 로 불러와 꽂아 넣는 일 하나(분대 캐릭터, 적 원본, 타격감 데이터).
    /// SceneContent 가 차례로 부르고, 모두 끝나면 게임 오브젝트 묶음을 켠다. 불러온 리소스는 넘겨받은 묶음(AssetHandleSet)이 소유한다.
    /// 실패하면 예외를 던진다. 꽂아 넣을 대상은 아직 꺼져 있으므로(Awake 전) 대상의 초기화 순서를 건드리지 않는다.
    /// </summary>
    public abstract class ContentStep : MonoBehaviour
    {
        public abstract Awaitable LoadAsync(AssetHandleSet handles);

        /// <summary>불러오고, 없으면 주소를 담은 예외를 던진다.</summary>
        protected static async Awaitable<T> Require<T>(AssetHandleSet handles, string address) where T : Object
        {
            T asset = await handles.LoadAsync<T>(address);
            if (asset == null) throw new MissingReferenceException("[Content] 불러오지 못했다: " + address);
            return asset;
        }
    }
}
