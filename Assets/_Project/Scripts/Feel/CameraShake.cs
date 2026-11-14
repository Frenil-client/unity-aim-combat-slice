using AimCombat.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace AimCombat.Feel
{
    /// <summary>
    /// 카메라 흔들림. 충격량(trauma, 0~1)을 쌓고 시간이 지나면 줄인다. 실제 흔들림은 충격량의 제곱에 비례해
    /// 약한 충격은 거의 티가 안 나고 큰 충격만 크게 흔들린다. 방향은 펄린 노이즈라 연속으로 부드럽게 흔들린다.
    /// - 흔들림은 그리는 동안에만 더하고(beginCameraRendering) 다 그리면 뺀다(endCameraRendering).
    ///   그래서 카메라를 따라가는 코드, 조준 광선, 피해 숫자 투영은 흔들리지 않은 위치를 본다. 흔들리는 건 화면뿐이다.
    /// - 위치는 화면 평면(카메라의 좌우, 위아래)으로만 밀고 앞뒤로는 밀지 않는다.
    /// 시간은 실제 시간으로 센다. 히트스톱으로 멈춘 동안에도 흔들림은 이어진다.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField] private FeelSettings settings;

        private Camera cam;
        private float trauma;
        private float seed;
        private Vector3 offset;
        private float roll;
        private bool applied;

        public float Trauma => trauma;

        public void SetSettings(FeelSettings value) => settings = value;

        public void Add(float amount)
        {
            if (amount <= 0f) return;
            trauma = Mathf.Clamp01(trauma + amount);
        }

        public void Clear()
        {
            trauma = 0f;
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            seed = Random.value * 100f;
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
            RenderPipelineManager.endCameraRendering += OnEndCamera;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
            RenderPipelineManager.endCameraRendering -= OnEndCamera;
            Remove();
        }

        private void Update()
        {
            offset = Vector3.zero;
            roll = 0f;
            if (settings == null) return;

            if (trauma > 0f) trauma = Mathf.Max(0f, trauma - settings.shakeRecovery * Time.unscaledDeltaTime);
            if (trauma <= 0f) return;

            float shake = trauma * trauma;
            float t = Time.unscaledTime * settings.shakeFrequency;
            float x = Mathf.PerlinNoise(seed, t) * 2f - 1f;
            float y = Mathf.PerlinNoise(seed + 10f, t) * 2f - 1f;
            float r = Mathf.PerlinNoise(seed + 20f, t) * 2f - 1f;
            offset = new Vector3(x, y, 0f) * (settings.maxShakeOffset * shake);
            roll = r * settings.maxShakeRoll * shake;
        }

        private void OnBeginCamera(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering != cam || applied || (offset == Vector3.zero && roll == 0f)) return;
            transform.position += transform.rotation * offset;
            transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, roll);
            applied = true;
        }

        private void OnEndCamera(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering == cam) Remove();
        }

        private void Remove()
        {
            if (!applied) return;
            transform.rotation = transform.rotation * Quaternion.Euler(0f, 0f, -roll);
            transform.position -= transform.rotation * offset;
            applied = false;
        }
    }
}
