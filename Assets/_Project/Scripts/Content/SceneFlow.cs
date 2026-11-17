using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace AimCombat.Content
{
    /// <summary>
    /// 씬 전환. 전투와 사격장 씬은 Addressables 씬이라 빌드 설정에는 시작 씬(Boot)만 있고, 씬은 주소로 연다.
    /// - 다음 씬의 기초 리소스 목록(ContentCatalog)을 알면 씬을 여는 동안 그 리소스를 함께 불러온다(ContentCache).
    /// - 다음 씬의 리소스가 이미 다 있으면(다시 시작, 같은 리소스를 쓰는 씬) 로딩 화면 없이 넘어간다. 앞 씬은 새 씬이 켜질 때까지 그대로 보인다.
    /// - 앞 씬만 쓰던 리소스는 새 씬이 열린 뒤 그 씬의 SceneContent 가 반환한다(앞 씬이 그려지는 동안 반환하면 모델과 재질이 빈다).
    /// - 한 장씩(Single) 열어 앞 씬은 Addressables 가 내린다.
    /// 에디터에서 전투 씬을 바로 열어 실행했을 때도 다시 시작이 같은 길로 가도록, 주소를 모르면 지금 씬 이름을 주소로 쓴다.
    /// </summary>
    public static class SceneFlow
    {
        private static string current;

        /// <summary>씬별 기초 리소스 목록. 시작 씬(BootLoader)이 넣는다. 없으면 씬이 열린 뒤 그 씬이 스스로 불러온다.</summary>
        public static ContentCatalog Catalog { get; set; }

        public static string Current => string.IsNullOrEmpty(current) ? SceneManager.GetActiveScene().name : current;

        public static bool Loading { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            current = null;
            Catalog = null;
            Loading = false;
        }

        public static async Awaitable LoadAsync(string address)
        {
            if (Loading) return;
            Loading = true;
            try
            {
                SceneContentSet set = Catalog != null ? Catalog.Find(address) : null;
                // 목록을 모르거나 아직 없는 리소스가 있으면 화면을 덮는다. 진행률은 씬 열기 절반, 리소스 절반이다.
                bool cover = !ContentCache.IsReady(set);
                if (cover) LoadingScreen.Show();
                float sceneProgress = 0f;
                float contentProgress = set != null ? 0f : 1f;
                Awaitable prepare = set != null ? ContentCache.PrepareAsync(set, p => contentProgress = p) : null;

                var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Single);
                while (!handle.IsDone)
                {
                    sceneProgress = handle.PercentComplete;
                    if (cover) LoadingScreen.Report(0.5f * sceneProgress + 0.5f * contentProgress);
                    await Awaitable.NextFrameAsync();
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError("[SceneFlow] 씬을 열지 못했다: " + address + " " + handle.OperationException?.Message);
                    LoadingScreen.Hide();
                    return;
                }
                current = address;

                // 리소스 실패는 씬의 SceneContent 가 같은 목록을 기다리다 알린다. 여기서는 끝나기만 기다린다.
                if (prepare != null)
                {
                    try { await prepare; }
                    catch (System.Exception) { }
                }
                // 불러올 실행 리소스가 없는 씬이면 여기서 걷는다. 있으면 그 씬이 묶음을 켤 때 걷는다.
                if (cover && Object.FindFirstObjectByType<SceneContent>() == null) LoadingScreen.Hide();
            }
            finally
            {
                Loading = false;
            }
        }

        /// <summary>지금 씬을 처음부터 다시 연다(전투 다시 시작). 리소스는 보관소에 남아 있어 다시 받지 않는다.</summary>
        public static void Reload() => _ = LoadAsync(Current);
    }
}
