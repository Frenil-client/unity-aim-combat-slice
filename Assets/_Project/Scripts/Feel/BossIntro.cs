using AimCombat.Battle;
using AimCombat.Squad;
using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.Playables;

namespace AimCombat.Feel
{
    /// <summary>
    /// 보스 등장 연출. 보스가 나오는 순간 전투를 잠깐 멈추고, 카메라가 보스를 비추며 WARNING 경고 띠를 띄운 뒤 전투 시점으로 돌아온다.
    /// - 움직임은 Timeline 에셋 하나(BossIntro.playable)가 정한다. 붉은 경보 화면, 경고 띠와 사선 줄무늬, WARNING 글 깜빡임, 보스 이름,
    ///   카메라 포커스 비율(cameraFocus)까지 곡선 하나씩이다. 타이밍은 Timeline 창에서 바로 고친다. 코드는 보스 이름만 넣는다.
    /// - 연출 동안 게임 시간을 멈추고(Time.timeScale 0) 분대 조작을 끈다. Timeline 은 실제 시간으로 돈다.
    ///   제한 시간과 적, 보스의 패턴도 멈춘다. 보스 모델만 실제 시간으로 움직여 날개를 편 동작을 보여 준다.
    /// - 전투 HUD 는 그동안 숨긴다(조준점, 피해 숫자, 보스 부위 표시가 바뀐 카메라와 어긋나지 않게).
    /// - 타격감 연출을 끄면(F2) 버스트 컷인처럼 띄우지 않고 바로 싸운다.
    /// </summary>
    public sealed class BossIntro : UIElement
    {
        [SerializeField] private BattleDirector battle;
        [SerializeField] private SquadController squad;
        [SerializeField] private BattleHud hud;
        [SerializeField] private FeelDirector feel;
        [SerializeField] private HitStop hitStop;
        [SerializeField] private CameraFocus cameraFocus;
        [SerializeField] private PlayableDirector director;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text nameText;
        [Tooltip("카메라 포커스 비율. Timeline 이 움직인다(0 전투 시점, 1 보스).")]
        [SerializeField] private float cameraFocus01;

        private BossAnimator bossAnimator;
        private float resumeTimeScale = 1f;
        private bool squadWasEnabled;

        /// <summary>연출 중이면 true.</summary>
        public bool Playing { get; private set; }

        private void Awake()
        {
            director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            director.playOnAwake = false;
            director.stopped += OnStopped;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            battle.BossSpawned += OnBossSpawned;
        }

        private void OnDestroy()
        {
            if (director != null) director.stopped -= OnStopped;
            if (battle != null) battle.BossSpawned -= OnBossSpawned;
            Finish();
        }

        private void OnBossSpawned(EnemyUnit boss)
        {
            if (!isActiveAndEnabled || (feel != null && !feel.FeelEnabled)) return;

            Playing = true;
            nameText.text = boss.Data != null ? boss.Data.DisplayName : boss.name;

            // 히트스톱이 걸려 있으면 먼저 풀어 평소 배율을 되찾는다. 그 배율로 연출 뒤에 돌아간다.
            if (hitStop != null) hitStop.Release();
            resumeTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;

            squadWasEnabled = squad.enabled;
            squad.enabled = false;           // 조작과 카메라 옆 따라가기를 멈춘다
            hud.SetCinematic(true);

            bossAnimator = boss.GetComponent<BossAnimator>();
            if (bossAnimator != null) bossAnimator.PlayEntrance(true);

            cameraFocus01 = 0f;
            cameraFocus.Begin(boss.AimPoint);

            director.Stop();
            director.time = 0;
            director.Play();
        }

        // Timeline 이 움직인 비율을 카메라에 넘긴다. 애니메이션 평가는 Update 와 LateUpdate 사이라 여기서 그 프레임 값을 읽는다.
        private void LateUpdate()
        {
            if (Playing) cameraFocus.Weight = cameraFocus01;
        }

        private void OnStopped(PlayableDirector _) => Finish();

        private void Finish()
        {
            if (!Playing) return;
            Playing = false;

            if (cameraFocus != null) cameraFocus.End();
            if (bossAnimator != null) bossAnimator.PlayEntrance(false);
            bossAnimator = null;
            if (hud != null) hud.SetCinematic(false);
            if (squad != null) squad.enabled = squadWasEnabled;
            if (group != null) group.alpha = 0f;
            Time.timeScale = resumeTimeScale;
        }
    }
}
