using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 엄폐물. 내구도가 있고, 엄폐 중인 아군 대신 적 공격을 받는다. 내구도가 다하면 부서져 더는 막지 못한다.
    /// 충돌체가 없다. 아군 사격 판정 광선이 엄폐물에 걸리지 않게 하기 위해서다.
    /// </summary>
    public sealed class CoverObject : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private float maxDurability = 400f;
        [SerializeField] private Renderer body;
        [SerializeField] private Color brokenColor = new Color(0.25f, 0.2f, 0.2f);

        private Color baseColor;
        private Vector3 baseScale;
        private MaterialPropertyBlock block;

        public float Durability { get; private set; }
        public float MaxDurability => maxDurability;
        public bool IsBroken => Durability <= 0f;

        private void Awake()
        {
            Durability = maxDurability;
            baseScale = body.transform.localScale;
            baseColor = body.sharedMaterial.GetColor(BaseColorId);
            block = new MaterialPropertyBlock();
        }

        /// <summary>피해를 받는다. 막고 남은 피해(내구도를 넘친 양)를 돌려준다.</summary>
        public float Absorb(float damage)
        {
            if (IsBroken) return damage;
            float absorbed = Mathf.Min(Durability, damage);
            Durability -= absorbed;
            Refresh();
            return damage - absorbed;
        }

        // 내구도가 줄수록 낮아지고 어두워진다. 부서지면 바닥에 깔린 잔해 높이로 남는다.
        private void Refresh()
        {
            float ratio = maxDurability > 0f ? Durability / maxDurability : 0f;
            float height = IsBroken ? 0.2f : Mathf.Lerp(0.55f, 1f, ratio);
            Vector3 scale = baseScale;
            scale.y *= height;
            body.transform.localScale = scale;
            Vector3 position = body.transform.localPosition;
            position.y = scale.y * 0.5f;
            body.transform.localPosition = position;

            block.SetColor(BaseColorId, IsBroken ? brokenColor : Color.Lerp(brokenColor, baseColor, 0.4f + 0.6f * ratio));
            body.SetPropertyBlock(block);
        }
    }
}
