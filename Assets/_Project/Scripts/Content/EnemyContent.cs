using AimCombat.Battle;
using UnityEngine;

namespace AimCombat.Content
{
    /// <summary>
    /// 적 원본(돌격형, 원거리형, 보스) 프리팹을 전투 감독에 넘긴다. 감독은 웨이브마다 이 원본을 복제한다.
    /// 쓰는 주소: Enemies/Rusher, Enemies/Ranged, Enemies/Boss. 씬의 기초 리소스 목록에 있어야 한다.
    /// </summary>
    public sealed class EnemyContent : ContentStep
    {
        [SerializeField] private BattleDirector director;
        [Tooltip("복제한 적을 놓는 부모.")]
        [SerializeField] private Transform enemyRoot;

        public override void Apply()
        {
            EnemyUnit rusher = Require<GameObject>(ContentAddress.Enemy("Rusher")).GetComponent<EnemyUnit>();
            EnemyUnit ranged = Require<GameObject>(ContentAddress.Enemy("Ranged")).GetComponent<EnemyUnit>();
            EnemyUnit boss = Require<GameObject>(ContentAddress.Enemy("Boss")).GetComponent<EnemyUnit>();
            director.SetTemplates(rusher, ranged, boss, enemyRoot);
        }
    }
}
