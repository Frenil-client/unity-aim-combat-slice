using UnityEngine;

namespace AimCombat.Battle
{
    /// <summary>
    /// 원거리형 적의 탄환. 히트스캔이 아니라 실제로 날아가는 오브젝트다(SPEC 2절).
    /// 쏜 순간 대상 아군의 자세를 기억하고, 사격 자세였으면 사격 위치로, 엄폐 자세였으면 엄폐 위치로 날아간다.
    /// 도착했을 때 아군이 자세를 바꿨으면 아군은 맞지 않는다(엄폐 위치로 온 탄은 엄폐물이 받는다).
    /// 5단계 부하 장면에서 풀링 대상이 된다.
    /// </summary>
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 9f;

        private AllyUnit target;
        private Vector3 destination;
        private float damage;
        private bool aimedAtFirePose;

        public void Launch(Vector3 from, AllyUnit ally, float hitDamage)
        {
            transform.position = from;
            target = ally;
            aimedAtFirePose = ally.InFirePose;
            destination = aimedAtFirePose ? ally.AimPoint : ally.CoverPoint;
            damage = hitDamage;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            if ((transform.position - destination).sqrMagnitude > 0.0001f) return;

            if (target != null) target.TakeProjectile(damage, aimedAtFirePose);
            Destroy(gameObject);
        }
    }
}
