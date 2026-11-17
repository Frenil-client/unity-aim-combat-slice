using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AimCombat.Content
{
    /// <summary>
    /// 시작 씬(Boot). 빌드에 들어가는 유일한 씬이다. Addressables 를 준비한 뒤 첫 씬(전투)을 주소로 연다.
    /// 지금은 모든 그룹이 앱에 들어가는 로컬 그룹이라 원격 카탈로그 확인(패치)은 하지 않는다.
    /// 원격 그룹을 두게 되면 여기서 Addressables Delivery 의 패치 서비스(CheckAsync, DownloadAsync)를 먼저 부른다.
    /// </summary>
    public sealed class BootLoader : MonoBehaviour
    {
        [SerializeField] private string firstScene = ContentAddress.BattleScene;

        private async void Start()
        {
            await Addressables.InitializeAsync().Task;
            await SceneFlow.LoadAsync(firstScene);
        }
    }
}
