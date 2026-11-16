using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 엄폐물. 내구도가 있고, 엄폐 중인 아군 대신 적 공격을 받는다. 내구도가 다하면 부서져 더는 막지 못한다.
    /// 충돌체가 없다. 아군 사격 판정 광선이 엄폐물에 걸리지 않게 하기 위해서다.
    /// 온전한 모델과 부서진 모델을 겹쳐 두고, 내구도가 기준 아래로 떨어지면 부서진 모델로 바꾼다.
    /// 손상 정도는 모델(텍스처의 금, 깨진 형태)로 보여주므로 색이나 크기는 바꾸지 않는다.
    /// </summary>
    public sealed class CoverObject : MonoBehaviour
    {
        [SerializeField] private float maxDurability = 400f;
        [SerializeField] private Renderer body;
        [SerializeField] private Renderer brokenBody;
        [Tooltip("내구도 비율이 이 값 이하가 되면 부서진 모델로 바꾼다.")]
        [SerializeField, Range(0f, 1f)] private float damagedRatio = 0.5f;

        public float Durability { get; private set; }
        public float MaxDurability => maxDurability;
        public bool IsBroken => Durability <= 0f;

        private void Awake()
        {
            Durability = maxDurability;
            if (brokenBody != null) brokenBody.gameObject.SetActive(false);
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

        // 내구도가 기준 아래로 떨어지면 부서진 모델로 바꾼다.
        private void Refresh()
        {
            if (brokenBody == null) return;
            float ratio = maxDurability > 0f ? Durability / maxDurability : 0f;
            bool damaged = ratio <= damagedRatio;
            body.gameObject.SetActive(!damaged);
            brokenBody.gameObject.SetActive(damaged);
        }
    }
}
