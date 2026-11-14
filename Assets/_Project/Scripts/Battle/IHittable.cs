using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>아군 사격에 맞을 수 있는 대상(적, 사격장 표적).</summary>
    public interface IHittable
    {
        /// <summary>방어력. 피해 계산의 기본 피해 단계에서 공격력에서 뺀다(SPEC 3.1).</summary>
        float Defense { get; }

        /// <summary>피해를 받는다. multiplier 는 차지 배율로, 반응 크기에 쓴다.</summary>
        void TakeHit(float damage, float multiplier, Vector3 point);
    }
}
