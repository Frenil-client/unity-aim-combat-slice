using AimCombat.Core.Combat;
using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>아군 사격에 맞을 수 있는 대상(적, 사격장 표적).</summary>
    public interface IHittable
    {
        /// <summary>체력과 방어력, 상태효과를 가진 전투 단위. 피해를 받지 않는 대상(사격장 표적)은 null.</summary>
        Combatant Combatant { get; }

        /// <summary>
        /// 사격에 맞은 반응(번쩍임, 튐). 피해는 쏘는 쪽이 Combatant 에 넣고, 여기서는 보이는 반응만 맡는다.
        /// multiplier 는 차지 배율로 반응 크기에 쓴다.
        /// </summary>
        void OnShot(float multiplier, Vector3 point);
    }
}
