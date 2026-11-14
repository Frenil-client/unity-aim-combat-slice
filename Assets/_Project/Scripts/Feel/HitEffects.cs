using System.Collections.Generic;
using UnityEngine;

namespace AimCombat.Feel
{
    /// <summary>
    /// 명중 불꽃, 탄피, 총구 화염(SPEC 4절). 오브젝트를 새로 만들지 않는다.
    /// - 불꽃과 탄피는 파티클 시스템 하나씩에 Emit 으로 한 알씩 넣는다. 시스템은 빌더가 씬에 만들어 둔다(머티리얼이 빌드에 포함되게).
    /// - 총구 화염은 아군마다 총구에 붙인 스프라이트 하나를 켰다 끈다. 그림은 실행할 때 절차적으로 만든다.
    /// 시간은 게임 시간이라 히트스톱 동안 함께 멈춘다(멈춘 장면에 불꽃이 걸려 있는 편이 손맛이 산다).
    /// </summary>
    public sealed class HitEffects : MonoBehaviour
    {
        private sealed class MuzzleFlash
        {
            public SpriteRenderer renderer;
            public Transform transform;
            public float endTime;
            public float duration;
            public float scale;
        }

        [SerializeField] private ParticleSystem sparks;
        [SerializeField] private ParticleSystem shells;
        [Tooltip("총구 화염 스프라이트의 그리기 순서. 캐릭터 그림보다 위에 그린다(같은 SortingGroup 안).")]
        [SerializeField] private int muzzleSortingOrder = 100;
        [SerializeField] private Color muzzleColor = new Color(1f, 0.85f, 0.45f, 1f);
        [Tooltip("총구 화염 크기 1일 때의 지름(유닛).")]
        [SerializeField] private float muzzleBaseSize = 0.55f;

        private readonly List<MuzzleFlash> flashes = new List<MuzzleFlash>();
        private static Sprite flashSprite;

        public ParticleSystem Sparks { get => sparks; set => sparks = value; }
        public ParticleSystem Shells { get => shells; set => shells = value; }

        /// <summary>총구 화염을 붙인다. 아군 순서대로 부른다. 반환값은 Muzzle 에 넘길 번호.</summary>
        public int RegisterMuzzle(Transform muzzle, Material spriteMaterial)
        {
            var go = new GameObject("MuzzleFlash");
            go.transform.SetParent(muzzle, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFlashSprite();
            if (spriteMaterial != null) renderer.sharedMaterial = spriteMaterial;
            renderer.sortingOrder = muzzleSortingOrder;
            renderer.color = muzzleColor;
            renderer.enabled = false;
            flashes.Add(new MuzzleFlash { renderer = renderer, transform = go.transform });
            return flashes.Count - 1;
        }

        public void Muzzle(int index, float scale, float duration)
        {
            if (index < 0 || index >= flashes.Count || scale <= 0f || duration <= 0f) return;
            MuzzleFlash f = flashes[index];
            f.duration = duration;
            f.endTime = Time.time + duration;
            f.scale = scale * muzzleBaseSize;
            f.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 90f));
            f.renderer.enabled = true;
        }

        /// <summary>
        /// 맞은 지점에서 불꽃을 튀긴다. size 와 speed 는 카메라에서 ReferenceDistance 떨어진 곳 기준이다.
        /// - 불꽃은 날아가는 방향으로 늘여 그린다(Stretch). 카메라 쪽으로 날리면 늘인 선이 화면에서 점으로 줄어 안 보이므로
        ///   화면 평면(카메라의 좌우, 위아래)으로 퍼뜨리고 카메라 쪽으로는 조금만 띄운다.
        /// - 크기와 속도에 카메라 거리 비율을 곱해 멀리 있는 적에게서도 화면에서 비슷한 크기로 보이게 한다.
        /// </summary>
        public void Spark(Vector3 point, Transform view, int count, float speed, float size, Color color)
        {
            if (sparks == null || count <= 0 || view == null) return;
            float distanceScale = Mathf.Max(0.2f, Vector3.Distance(view.position, point) / ReferenceDistance);
            var emit = new ParticleSystem.EmitParams
            {
                position = point,
                startSize = size * distanceScale,
                startColor = color,
                applyShapeToPosition = false,
            };
            Vector3 toCamera = (view.position - point).normalized;
            for (int i = 0; i < count; i++)
            {
                Vector2 spread = Random.insideUnitCircle.normalized;
                Vector3 direction = view.right * spread.x + view.up * (spread.y * 0.8f + 0.3f) + toCamera * 0.25f;
                emit.velocity = direction.normalized * (speed * distanceScale * Random.Range(0.5f, 1f));
                emit.startLifetime = Random.Range(0.15f, 0.3f);
                sparks.Emit(emit, 1);
            }
        }

        /// <summary>불꽃 크기와 속도의 기준 거리(유닛). 이 거리에서 프리셋 값 그대로 보인다.</summary>
        public const float ReferenceDistance = 10f;

        /// <summary>탄피를 튀긴다. side 는 탄피가 튀는 옆 방향(총의 오른쪽).</summary>
        public void Shell(Vector3 position, Vector3 side, int count)
        {
            if (shells == null || count <= 0) return;
            var emit = new ParticleSystem.EmitParams { position = position, applyShapeToPosition = false };
            for (int i = 0; i < count; i++)
            {
                emit.velocity = side * Random.Range(1.2f, 2f) + Vector3.up * Random.Range(1.5f, 2.5f) + Random.insideUnitSphere * 0.3f;
                emit.rotation = Random.Range(0f, 360f);
                emit.angularVelocity = Random.Range(-720f, 720f);
                shells.Emit(emit, 1);
            }
        }

        private void LateUpdate()
        {
            float now = Time.time;
            foreach (MuzzleFlash f in flashes)
            {
                if (!f.renderer.enabled) continue;
                float t = f.duration > 0f ? 1f - (f.endTime - now) / f.duration : 1f;
                if (t >= 1f)
                {
                    f.renderer.enabled = false;
                    continue;
                }
                // 처음에 크고 밝다가 줄어든다.
                float size = f.scale * Mathf.Lerp(1.2f, 0.6f, t);
                f.transform.localScale = new Vector3(size, size, 1f);
                Color c = muzzleColor;
                c.a = 1f - t * t;
                f.renderer.color = c;
            }
        }

        // 가운데가 밝고 네 갈래로 뻗는 별 모양. 텍스처 파일 없이 실행할 때 한 번 만든다.
        private static Sprite GetFlashSprite()
        {
            if (flashSprite != null) return flashSprite;
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "MuzzleFlash" };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f;
                    float v = (y + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(u * u + v * v);
                    float glow = Mathf.Clamp01(1f - r);
                    glow *= glow;
                    float spikes = Mathf.Clamp01(1f - Mathf.Abs(u) * 9f) * Mathf.Clamp01(1f - Mathf.Abs(v)) +
                                   Mathf.Clamp01(1f - Mathf.Abs(v) * 9f) * Mathf.Clamp01(1f - Mathf.Abs(u));
                    float a = Mathf.Clamp01(glow + spikes * 0.8f);
                    byte b = (byte)(a * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, b);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            flashSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return flashSprite;
        }
    }
}
