using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 엄폐물. 내구도가 있고, 엄폐 중인 아군 대신 적 공격을 받는다. 내구도가 다하면 부서져 더는 막지 못한다.
    /// 충돌체가 없다. 아군 사격 판정 광선이 엄폐물에 걸리지 않게 하기 위해서다.
    /// 온전한 모델과 부서진 모델을 겹쳐 두고, 내구도가 기준 아래로 떨어지면 부서진 모델로 바꾼다.
    /// 두 모델은 바닥이 원점(피벗)이라 높이를 줄여도 바닥에 붙어 있다.
    /// </summary>
    public sealed class CoverObject : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private float maxDurability = 400f;
        [SerializeField] private Renderer body;
        [SerializeField] private Renderer brokenBody;
        [Tooltip("내구도 비율이 이 값 이하가 되면 부서진 모델로 바꾼다.")]
        [SerializeField, Range(0f, 1f)] private float damagedRatio = 0.5f;
        [Tooltip("완전히 부서진 뒤 잔해로 남는 높이 비율.")]
        [SerializeField, Range(0.05f, 1f)] private float rubbleHeight = 0.35f;
        [SerializeField] private Color brokenColor = new Color(0.25f, 0.2f, 0.2f);

        private Color baseColor;
        private Vector3 brokenScale;
        private MaterialPropertyBlock block;

        public float Durability { get; private set; }
        public float MaxDurability => maxDurability;
        public bool IsBroken => Durability <= 0f;

        private void Awake()
        {
            Durability = maxDurability;
            baseColor = body.sharedMaterial.GetColor(BaseColorId);
            block = new MaterialPropertyBlock();
            if (brokenBody != null)
            {
                brokenScale = brokenBody.transform.localScale;
                brokenBody.gameObject.SetActive(false);
            }
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

        // 내구도가 줄수록 어두워지고, 기준 아래에서는 부서진 모델로 바뀐다. 다 부서지면 낮은 잔해로 남는다.
        private void Refresh()
        {
            float ratio = maxDurability > 0f ? Durability / maxDurability : 0f;
            bool damaged = brokenBody != null && ratio <= damagedRatio;
            body.gameObject.SetActive(!damaged);
            Renderer shown = body;
            if (brokenBody != null)
            {
                brokenBody.gameObject.SetActive(damaged);
                if (damaged) shown = brokenBody;
                Vector3 scale = brokenScale;
                if (IsBroken) scale.y *= rubbleHeight;
                brokenBody.transform.localScale = scale;
            }

            block.SetColor(BaseColorId, IsBroken ? brokenColor : Color.Lerp(brokenColor, baseColor, 0.4f + 0.6f * ratio));
            shown.SetPropertyBlock(block);
        }
    }
}
