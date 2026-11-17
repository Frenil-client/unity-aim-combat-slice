using TMPro;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// Addressables 로 씬과 리소스를 불러오는 동안 화면을 덮는 로딩 화면.
    /// - 시작 씬(Boot)에 들어 있고 씬이 바뀌어도 남는다(DontDestroyOnLoad). 빌드에 들어가는 씬이라 Addressables 를 준비하기 전에도 보인다.
    ///   그래서 번들에 든 그림과 글꼴을 쓰지 않고 색과 기본 글꼴만 쓴다(번들 리소스를 가리키면 앱 본체에 한 벌 더 들어간다).
    /// - 진행률은 두 구간이다. 씬 열기(0~50%, SceneFlow)와 씬의 실행 리소스 불러오기(50~100%, SceneContent).
    ///   리소스를 다 꽂고 게임 오브젝트 묶음을 켜면 걷힌다. 다시 시작할 때도 같은 길로 다시 덮는다.
    /// - 덮는 동안 입력을 막는다. UI 스택(팝업)보다 위에 그린다. 게임 시간이 멈춰 있어도(선택 화면) 실제 시간으로 움직인다.
    /// - 너무 빨리 끝나면 깜빡임처럼 보이므로 최소 시간만큼은 보이고 서서히 걷힌다.
    /// 에디터에서 전투 씬을 바로 열어 실행하면 Boot 를 거치지 않아 로딩 화면이 없다(불러오는 동안 하늘만 보인다).
    /// </summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [Tooltip("진행률 막대의 찬 부분. 오른쪽 앵커를 진행률만큼 옮긴다(그림 없이 색만 쓰려고 Filled 이미지 대신).")]
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private RectTransform spinner;
        [Tooltip("한 번 덮으면 이 시간(초)만큼은 보인다.")]
        [SerializeField] private float minimumVisible = 0.6f;
        [SerializeField] private float fadeOut = 0.35f;
        [SerializeField] private float spinSpeed = 220f;

        private float shownAt;
        private float target;
        private float shown;
        private bool hideRequested;

        public static LoadingScreen Instance { get; private set; }

        /// <summary>지금 화면을 덮고 있는지.</summary>
        public bool Visible => group != null && group.alpha > 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ShowNow();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>화면을 덮고 진행률을 처음부터 센다.</summary>
        public static void Show()
        {
            if (Instance != null) Instance.ShowNow();
        }

        /// <summary>진행률(0~1). 뒤로 가지 않는다.</summary>
        public static void Report(float progress)
        {
            if (Instance != null) Instance.target = Mathf.Max(Instance.target, Mathf.Clamp01(progress));
        }

        /// <summary>다 불러왔다. 진행률을 채우고 최소 시간이 지나면 걷는다.</summary>
        public static void Hide()
        {
            if (Instance == null) return;
            Instance.target = 1f;
            Instance.hideRequested = true;
        }

        private void ShowNow()
        {
            shownAt = Time.unscaledTime;
            target = 0f;
            shown = 0f;
            hideRequested = false;
            group.alpha = 1f;
            group.blocksRaycasts = true;
            Refresh();
        }

        private void Update()
        {
            if (group.alpha <= 0f) return;
            float dt = Time.unscaledDeltaTime;
            spinner.localRotation = Quaternion.Euler(0f, 0f, spinner.localEulerAngles.z - spinSpeed * dt);
            // 막대는 목표를 따라 부드럽게 찬다(불러오기가 계단식으로 끝나도 막대는 흐른다).
            shown = Mathf.MoveTowards(shown, target, Mathf.Max(0.6f, (target - shown) * 6f) * dt);
            Refresh();

            if (!hideRequested || shown < 1f || Time.unscaledTime < shownAt + minimumVisible) return;
            group.alpha = fadeOut > 0f ? Mathf.MoveTowards(group.alpha, 0f, dt / fadeOut) : 0f;
            if (group.alpha <= 0f) group.blocksRaycasts = false;
        }

        private void Refresh()
        {
            progressFill.anchorMax = new Vector2(shown, progressFill.anchorMax.y);
            progressText.text = "NOW LOADING  " + Mathf.RoundToInt(shown * 100f) + "%";
        }
    }
}
