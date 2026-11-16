using System.Collections.Generic;
using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 고리 모양 UI 그림을 실행할 때 만들어 준다(임시. 리소스가 준비되면 그림 파일로 바꾼다).
    /// 같은 크기와 두께는 한 번만 만들어 함께 쓴다. 조준점과 보스 저지 표적이 쓴다.
    /// </summary>
    public static class RingSprite
    {
        private static readonly Dictionary<(int, int), Sprite> cache = new Dictionary<(int, int), Sprite>();

        /// <summary>size 는 그림 한 변의 픽셀 수, inner 는 바깥 반지름에 대한 안쪽 반지름의 비율(0이면 꽉 찬 원)이다.</summary>
        public static Sprite Get(int size, float inner)
        {
            var key = (size, Mathf.RoundToInt(inner * 1000f));
            if (cache.TryGetValue(key, out Sprite sprite) && sprite != null) return sprite;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Ring_" + size + "_" + key.Item2,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            float outerRadius = half - 1f;
            float innerRadius = outerRadius * inner;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - half;
                    float dy = y + 0.5f - half;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    // 가장자리는 한 픽셀 폭으로 부드럽게 한다.
                    float alpha = Mathf.Clamp01(outerRadius - distance);
                    if (inner > 0f) alpha *= Mathf.Clamp01(distance - innerRadius);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            cache[key] = sprite;
            return sprite;
        }
    }
}
