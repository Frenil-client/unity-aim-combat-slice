using System;
using System.Collections.Generic;
using AimCombat.Input;
using AimCombat.Squad;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AimCombat.Battle
{
    public enum BattleState
    {
        Playing,
        Victory,
        Defeat,
    }

    /// <summary>
    /// 한 판 진행. 웨이브를 차례로 내보내고 승패를 정한다.
    /// - 승리: 모든 웨이브의 적을 처치(보스는 2단계에서 마지막 웨이브로 붙는다).
    /// - 패배: 아군 전원 전투 불능 또는 제한 시간 초과.
    /// 끝나면 방아쇠를 누르거나 R 키로 다시 시작한다.
    /// </summary>
    public sealed class BattleDirector : MonoBehaviour
    {
        [Serializable]
        public sealed class Wave
        {
            public int rushers = 6;
            public int ranged;
            [Tooltip("적이 한 기씩 나오는 간격(초).")]
            public float spawnInterval = 0.8f;
        }

        [SerializeField] private SquadController squad;
        [SerializeField] private CombatInput input;
        [SerializeField] private EnemyUnit rusherTemplate;
        [SerializeField] private EnemyUnit rangedTemplate;
        [SerializeField] private Wave[] waves;
        [SerializeField] private float timeLimit = 150f;
        [Tooltip("웨이브 사이 쉬는 시간(초).")]
        [SerializeField] private float waveBreak = 2.5f;

        [Header("등장 위치")]
        [SerializeField] private Vector2 rusherSpawnX = new Vector2(-6f, 6f);
        [SerializeField] private Vector2 rusherSpawnZ = new Vector2(22f, 28f);
        [SerializeField] private Vector2 rangedSpawnX = new Vector2(-5f, 5f);
        [SerializeField] private Vector2 rangedSpawnZ = new Vector2(12f, 17f);

        private readonly List<EnemyUnit> spawned = new List<EnemyUnit>();
        private readonly Queue<EnemyKind> pending = new Queue<EnemyKind>();
        private int waveIndex = -1;
        private float nextSpawnTime;
        private float waveStartTime;
        private float startTime;

        public BattleState State { get; private set; } = BattleState.Playing;
        public int WaveNumber => Mathf.Max(1, waveIndex + 1);
        public int WaveCount => waves.Length;
        public float TimeLeft => Mathf.Max(0f, timeLimit - (Time.time - startTime));
        public int EnemiesLeft => pending.Count + EnemyUnit.Alive.Count;

        /// <summary>다음 웨이브까지 쉬는 중이면 true.</summary>
        public bool InBreak => Time.time < waveStartTime;

        private void Start()
        {
            startTime = Time.time;
            StartNextWave(0.5f);
        }

        private void Update()
        {
            if (State != BattleState.Playing)
            {
                bool restart = input.FirePressedThisFrame
                    || (UnityEngine.InputSystem.Keyboard.current?.rKey.wasPressedThisFrame ?? false);
                if (restart) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (squad.AllDown || TimeLeft <= 0f)
            {
                End(BattleState.Defeat);
                return;
            }

            if (Time.time < waveStartTime) return;

            if (pending.Count > 0)
            {
                if (Time.time >= nextSpawnTime)
                {
                    Spawn(pending.Dequeue());
                    nextSpawnTime = Time.time + waves[waveIndex].spawnInterval;
                }
                return;
            }

            if (EnemyUnit.Alive.Count > 0) return;

            if (waveIndex + 1 < waves.Length) StartNextWave(waveBreak);
            else End(BattleState.Victory);
        }

        private void StartNextWave(float delay)
        {
            waveIndex++;
            waveStartTime = Time.time + delay;
            nextSpawnTime = waveStartTime;

            // 돌격형과 원거리형을 섞어 내보낸다. 원거리형은 웨이브 중간쯤 나온다.
            Wave wave = waves[waveIndex];
            int total = wave.rushers + wave.ranged;
            int rangedLeft = wave.ranged;
            for (int i = 0; i < total; i++)
            {
                bool ranged = rangedLeft > 0 && (i % 3 == 1 || total - i <= rangedLeft);
                if (ranged) rangedLeft--;
                pending.Enqueue(ranged ? EnemyKind.Ranged : EnemyKind.Rusher);
            }
        }

        private void Spawn(EnemyKind kind)
        {
            EnemyUnit template = kind == EnemyKind.Rusher ? rusherTemplate : rangedTemplate;
            Vector2 xRange = kind == EnemyKind.Rusher ? rusherSpawnX : rangedSpawnX;
            Vector2 zRange = kind == EnemyKind.Rusher ? rusherSpawnZ : rangedSpawnZ;
            Vector3 position = new Vector3(
                UnityEngine.Random.Range(xRange.x, xRange.y), template.transform.position.y,
                UnityEngine.Random.Range(zRange.x, zRange.y));

            EnemyUnit enemy = Instantiate(template, template.transform.parent);
            enemy.name = template.name.Replace("Template", "") + spawned.Count;
            enemy.Spawn(position, PickAllyTarget);
            spawned.Add(enemy);
        }

        // 살아 있는 아군 중 하나를 고른다.
        private AllyUnit PickAllyTarget()
        {
            int count = squad.Count;
            int start = UnityEngine.Random.Range(0, count);
            for (int step = 0; step < count; step++)
            {
                AllyUnit unit = squad.UnitOf((start + step) % count);
                if (unit != null && !unit.IsDown) return unit;
            }
            return null;
        }

        private void End(BattleState result)
        {
            State = result;
            // 남은 적은 멈춘다. 결과 화면 뒤에서 계속 공격하지 않게 한다.
            foreach (EnemyUnit enemy in spawned)
            {
                if (enemy != null) enemy.enabled = false;
            }
        }
    }
}
