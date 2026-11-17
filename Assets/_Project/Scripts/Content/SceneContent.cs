using System;
using AddressablesDelivery.Loading;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬의 실행 리소스를 Addressables 로 불러온 뒤 게임 오브젝트 묶음을 켠다.
    /// - 씬에는 배경과 관리자(분대, 전투 감독, HUD)만 있고, 캐릭터, 적, 데이터는 주소로 불러온다. 묶음(gameplayRoot)은 꺼진 채 저장돼 있어
    ///   불러오기가 끝나기 전에는 누구의 Awake 도 돌지 않는다. 단계(ContentStep)가 꽂아 넣은 뒤 켠다.
    /// - 불러온 리소스는 이 씬이 소유한다(AssetHandleSet 하나). 씬이 내려갈 때 Dispose 로 한 번에 반환한다.
    ///   복제한 인스턴스는 씬 오브젝트라 씬과 함께 사라진다.
    /// </summary>
    public sealed class SceneContent : MonoBehaviour
    {
        [Tooltip("불러오기가 끝나면 켜는 게임 오브젝트 묶음. 씬에는 꺼진 채 저장한다.")]
        [SerializeField] private GameObject gameplayRoot;
        [Tooltip("차례로 부르는 불러오기 단계.")]
        [SerializeField] private ContentStep[] steps;

        private AssetHandleSet handles;

        /// <summary>모든 단계가 끝나 묶음을 켰으면 true.</summary>
        public bool Ready { get; private set; }

        private async void Awake()
        {
            if (gameplayRoot != null) gameplayRoot.SetActive(false);
            handles = new AssetHandleSet(gameObject.scene.name);
            try
            {
                foreach (ContentStep step in steps)
                {
                    await step.LoadAsync(handles);
                    if (this == null) return;   // 불러오는 사이 씬이 내려갔다
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                return;
            }

            Ready = true;
            if (gameplayRoot != null) gameplayRoot.SetActive(true);
        }

        private void OnDestroy() => handles?.Dispose();
    }
}
