using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬이 시작할 때 불러온 리소스를 꽂아 넣는 일 하나(분대 캐릭터, 적 원본, 타격감 데이터).
    /// 리소스는 씬의 기초 리소스 목록(SceneContentSet)대로 보관소(ContentCache)에 미리 불러와 있고, 여기서는 꺼내 쓰기만 한다.
    /// SceneContent 가 차례로 부르고, 모두 끝나면 게임 오브젝트 묶음을 켠다. 꽂아 넣을 대상은 아직 꺼져 있으므로(Awake 전) 대상의 초기화 순서를 건드리지 않는다.
    /// 실패하면 예외를 던진다.
    /// </summary>
    public abstract class ContentStep : MonoBehaviour
    {
        public abstract void Apply();

        /// <summary>보관소에서 꺼낸다. 없으면 목록에서 빠진 것이므로 주소를 담은 예외를 던진다.</summary>
        protected static T Require<T>(string address) where T : Object
        {
            T asset = ContentCache.Get<T>(address);
            if (asset == null) throw new MissingReferenceException("[Content] 씬의 기초 리소스 목록에 없거나 불러오지 못했다: " + address);
            return asset;
        }

        /// <summary>있으면 꺼내고 없으면 null(없어도 되는 그림 등).</summary>
        protected static T Optional<T>(string address) where T : Object => ContentCache.Get<T>(address);
    }
}
