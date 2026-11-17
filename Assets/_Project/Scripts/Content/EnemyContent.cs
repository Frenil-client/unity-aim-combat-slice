using AddressablesDelivery.Loading;
using AimCombat.Battle;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>적 원본(돌격형, 원거리형, 보스) 프리팹을 주소로 불러와 전투 감독에 넘긴다. 감독은 웨이브마다 이 원본을 복제한다.</summary>
    public sealed class EnemyContent : ContentStep
    {
        [SerializeField] private BattleDirector director;
        [Tooltip("복제한 적을 놓는 부모.")]
        [SerializeField] private Transform enemyRoot;

        public override async Awaitable LoadAsync(AssetHandleSet handles)
        {
            EnemyUnit rusher = (await Require<GameObject>(handles, ContentAddress.Enemy("Rusher"))).GetComponent<EnemyUnit>();
            EnemyUnit ranged = (await Require<GameObject>(handles, ContentAddress.Enemy("Ranged"))).GetComponent<EnemyUnit>();
            EnemyUnit boss = (await Require<GameObject>(handles, ContentAddress.Enemy("Boss"))).GetComponent<EnemyUnit>();
            director.SetTemplates(rusher, ranged, boss, enemyRoot);
        }
    }
}
