using System;
using System.Collections.Generic;
using AimCombat.Input;
using AimCombat.Squad;
using UISystem;
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
    /// - 일반 웨이브를 다 넘기면 마지막 웨이브로 보스 1체가 나온다(SPEC 1절).
    /// - 승리: 제한 시간 안에 보스 처치. 보스 원본이 없으면 모든 웨이브의 적을 처치.
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
        [Tooltip("마지막 웨이브의 보스. 비어 있으면 일반 웨이브만으로 끝난다.")]
        [SerializeField] private EnemyUnit bossTemplate;
        [SerializeField] private Vector3 bossSpawn = new Vector3(0f, 0f, 30f);
        [SerializeField] private float timeLimit = 150f;
        [Tooltip("전투 시작 전에 스킬 세트 선택 팝업을 연다(고를 세트가 있을 때만). 끄면 바로 전투를 시작한다.")]
        [SerializeField] private bool selectLoadout = true;
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
        private bool started;
        private bool bossSpawned;
        private bool selecting;
        private float pausedTimeScale = 1f;

        public BattleState State { get; private set; } = BattleState.Playing;
        public int WaveNumber => Mathf.Max(1, waveIndex + 1);
        public int WaveCount => waves.Length + (bossTemplate != null ? 1 : 0);

        /// <summary>보스 웨이브에 들어왔으면 true.</summary>
        public bool BossWave => waveIndex >= waves.Length;

        /// <summary>나와 있는 보스. 나오기 전이거나 쓰러졌으면 null 이거나 IsDead.</summary>
        public EnemyUnit Boss { get; private set; }

        /// <summary>보스가 나온 순간. 등장 연출(BossIntro)이 받는다.</summary>
        public event Action<EnemyUnit> BossSpawned;
        /// <summary>남은 제한 시간. 전투가 시작되기 전(스킬 세트 선택 중)에는 제한 시간 전체다.</summary>
        public float TimeLeft => started ? Mathf.Max(0f, timeLimit - (Time.time - startTime)) : timeLimit;
        public int EnemiesLeft => pending.Count + EnemyUnit.Alive.Count;

        /// <summary>다음 웨이브까지 쉬는 중이면 true.</summary>
        public bool InBreak => Time.time < waveStartTime;

        // 고를 세트가 있으면 전투 시작 전에 스킬 세트 선택 팝업을 UI 스택으로 연다.
        // 분대와 이 감독을 꺼 두면 둘의 Start(전투 시작)가 팝업이 닫힌 뒤로 미뤄진다.
        // 게임 시간도 멈춘다. 꺼 둔 두 컴포넌트 밖의 것(상태효과 시간, 연출)까지 흐르지 않게 하기 위해서다.
        private void Awake()
        {
            if (!selectLoadout || UIManager.Instance == null || !LoadoutSelectPopup.HasChoice(squad)) return;
            selecting = true;
            squad.enabled = false;
            enabled = false;
            pausedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _ = SelectLoadoutAsync();
        }

        private async Awaitable SelectLoadoutAsync()
        {
            try
            {
                LoadoutSelectPopup popup = await UIManager.Instance.OpenAsync<LoadoutSelectPopup>();
                if (popup != null)
                {
                    popup.Bind(squad);
                    await popup.WaitForCloseAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
            finally
            {
                EndSelection();
            }
        }

        // 선택이 끝나면 시간을 되돌리고 분대와 감독을 켠다(Start 가 돌아 전투가 시작된다).
        // 선택 중에 씬을 떠나면(다시 시작, 플레이 종료) 시간만 되돌린다.
        private void EndSelection()
        {
            if (!selecting) return;
            selecting = false;
            Time.timeScale = pausedTimeScale > 0f ? pausedTimeScale : 1f;
            if (this == null) return;
            if (squad != null) squad.enabled = true;
            enabled = true;
        }

        private void OnDestroy() => EndSelection();

        private void Start()
        {
            startTime = Time.time;
            started = true;
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

            if (BossWave)
            {
                if (!bossSpawned) SpawnBoss();
                else if (Boss == null || Boss.IsDead) End(BattleState.Victory); // 쓰러지면 오브젝트가 지워진다
                return;
            }

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
            else if (bossTemplate != null) StartBossWave(waveBreak);
            else End(BattleState.Victory);
        }

        private void StartBossWave(float delay)
        {
            waveIndex = waves.Length;
            waveStartTime = Time.time + delay;
        }

        private void SpawnBoss()
        {
            EnemyUnit boss = Instantiate(bossTemplate, bossTemplate.transform.parent);
            boss.name = "Boss";
            boss.Spawn(new Vector3(bossSpawn.x, bossTemplate.transform.position.y, bossSpawn.z), PickAllyTarget);
            spawned.Add(boss);
            Boss = boss;
            bossSpawned = true;
            BossSpawned?.Invoke(boss);
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
            // 남은 적과 분대를 멈춘다. 결과 화면 뒤에서 계속 쏘거나 스킬을 쓰지 않게 한다.
            foreach (EnemyUnit enemy in spawned)
            {
                if (enemy != null) enemy.enabled = false;
            }
            if (squad != null) squad.StopForResult();
        }
    }
}
