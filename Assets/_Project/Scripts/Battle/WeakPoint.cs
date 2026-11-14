using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 약점 부위 표시. 이 컴포넌트가 붙은 충돌체에 맞은 사격은 약점 보너스를 받는다(SPEC 3.1).
    /// 피해는 부모의 IHittable 이 받는다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class WeakPoint : MonoBehaviour
    {
    }
}
