using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬 전환. 전투와 사격장 씬은 Addressables 씬이라 빌드 설정에는 시작 씬(Boot)만 있고, 씬은 주소로 연다.
    /// 한 장씩(Single) 열기 때문에 Addressables 가 앞 씬과 그 씬이 끌어온 번들을 함께 놓는다.
    /// 에디터에서 전투 씬을 바로 열어 실행했을 때도 다시 시작이 같은 길로 가도록, 주소를 모르면 지금 씬 이름을 주소로 쓴다.
    /// </summary>
    public static class SceneFlow
    {
        private static string current;

        public static string Current => string.IsNullOrEmpty(current) ? SceneManager.GetActiveScene().name : current;

        public static bool Loading { get; private set; }

        public static async Awaitable LoadAsync(string address)
        {
            if (Loading) return;
            Loading = true;
            try
            {
                var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Single);
                await handle.Task;
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) current = address;
                else Debug.LogError("[SceneFlow] 씬을 열지 못했다: " + address + " " + handle.OperationException?.Message);
            }
            finally
            {
                Loading = false;
            }
        }

        /// <summary>지금 씬을 처음부터 다시 연다(전투 다시 시작).</summary>
        public static void Reload() => _ = LoadAsync(Current);
    }
}
