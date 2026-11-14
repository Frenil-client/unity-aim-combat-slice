using UnityEngine;

namespace AimCombat.Feel
{
    /// <summary>
    /// 히트스톱. 정해진 실제 시간 동안 Time.timeScale 을 낮췄다가 되돌린다.
    /// - 멈추기 전의 배율(평소 1, 확인 도구가 빨리 돌릴 때 4)을 기억해 두고 그 배율에 곱한다. 끝나면 그 배율로 돌아간다.
    /// - 멈춘 동안 다시 요청이 오면 더 긴 쪽, 더 낮은 배율 쪽을 따른다. 짧은 간격의 명중이 쌓여 길게 멈추지 않게 더하지는 않는다.
    /// 시간은 Time.unscaledDeltaTime 으로 센다. 멈춘 동안 게임 시간은 거의 흐르지 않기 때문이다.
    /// </summary>
    public sealed class HitStop : MonoBehaviour
    {
        [Tooltip("한 번에 멈출 수 있는 최대 시간(초). 여러 요청이 겹쳐도 넘지 않는다.")]
        [SerializeField] private float maxDuration = 0.3f;

        private float remaining;
        private float scale = 1f;
        private float baseline = 1f;

        public bool IsStopped => remaining > 0f;

        public void Request(float duration, float timeScale)
        {
            if (duration <= 0f) return;
            if (!IsStopped)
            {
                baseline = Time.timeScale;
                scale = 1f;
            }
            remaining = Mathf.Min(maxDuration, Mathf.Max(remaining, duration));
            scale = Mathf.Min(scale, Mathf.Clamp01(timeScale));
            Time.timeScale = baseline * scale;
        }

        private void Update()
        {
            if (!IsStopped) return;
            remaining -= Time.unscaledDeltaTime;
            if (remaining > 0f) return;
            remaining = 0f;
            Time.timeScale = baseline;
        }

        // 씬을 떠날 때 멈춘 채로 남지 않게 한다.
        private void OnDisable()
        {
            if (!IsStopped) return;
            remaining = 0f;
            Time.timeScale = baseline;
        }
    }
}
