using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 보스 부위와 저지 표적 표시(SPEC 3.4). 화면 좌표 UI 라서 부위 위치를 매 프레임 화면에 투영해 따라간다.
    /// - 살아 있는 팔 위에 이름과 체력 막대를 띄운다. 손상 단계면 막대 색이 바뀐다.
    /// - 저지 표적이 뜬 부위에는 원을 띄운다. 밝은 호가 남은 저지 게이지이고, 깎인 만큼 붉은 바탕이 드러난다. 예고가 끝나갈수록 빠르게 맥동한다.
    ///   예고 중에는 보스 몸이 붉게 맥동하므로 남은 게이지는 붉은색이 아닌 밝은 노란색으로 둔다.
    /// - 강타 예고 중에는 보스 머리 위에 경고와 남은 시간을 띄운다.
    /// 모양은 씬에 구성돼 있다. 부위 표시는 원본(BossPartView)을 부위 수만큼 복제하고, 원 안팎의 배치는 원본 안의 앵커와 레이아웃이 정한다.
    /// 여기서는 화면 위치, 원 지름(부위의 화면 크기), 값만 넣는다. 맥동은 애니메이션 클립이고 빠르기만 정한다.
    /// </summary>
    public sealed class BossPartHud : MonoBehaviour
    {
        private static readonly int PulseId = Animator.StringToHash("Pulse");

        [SerializeField] private BattleDirector director;
        [SerializeField] private Camera viewCamera;
        [Tooltip("부위 표시 원본. 꺼 둔 채로 두고 부위 수만큼 복제한다.")]
        [SerializeField] private BossPartView viewTemplate;
        [Tooltip("경고 표시. 보스 머리 위 지점에 놓이고, 글자는 그 위로 떨어져 있다(씬에서 정한다).")]
        [SerializeField] private RectTransform warning;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private Animator warningPulse;

        [Header("모양")]
        [Tooltip("저지 표적 원의 가장 작은 지름(픽셀, 1080 기준). 보스가 멀어도 이보다 작아지지 않는다.")]
        [SerializeField] private float minRingSize = 90f;
        [SerializeField] private Color healthColor = new Color(0.95f, 0.95f, 0.98f, 1f);
        [SerializeField] private Color damagedColor = new Color(1f, 0.55f, 0.2f, 1f);
        [SerializeField] private Color markedLabelColor = new Color(1f, 0.4f, 0.35f, 1f);

        private readonly List<BossPartView> views = new List<BossPartView>();
        private BossBrain shownBoss;
        private bool showing;
        private float canvasScale = 1f;

        private void Awake()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            viewTemplate.gameObject.SetActive(false);
            warning.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            EnemyUnit unit = director != null ? director.Boss : null;
            BossBrain boss = unit != null && !unit.IsDead ? unit.Boss : null;
            // 보스가 없거나 쓰러졌거나 전투가 끝났으면 부위 표시와 경고를 거둔다.
            // 쓰러진 보스는 오브젝트가 지워져 Unity 의 null 비교로는 "보여 주던 보스"를 알 수 없으므로 따로 기억한 값으로 판단한다.
            bool battleOver = director != null && director.State != BattleState.Playing;
            if (boss == null || battleOver)
            {
                if (showing) HideAll();
                showing = false;
                shownBoss = null;
                return;
            }
            showing = true;

            Canvas canvas = GetComponentInParent<Canvas>();
            canvasScale = canvas != null ? canvas.scaleFactor : 1f;

            IReadOnlyList<BossPart> parts = boss.Parts;
            if (boss != shownBoss)
            {
                shownBoss = boss;
                HideAll();
                while (views.Count < parts.Count)
                {
                    BossPartView view = Instantiate(viewTemplate, viewTemplate.transform.parent);
                    view.name = "Part_" + views.Count;
                    views.Add(view);
                }
            }

            // 맥동은 예고가 끝나갈수록 빨라진다(클립 한 번에 1초, 처음 1배에서 끝에 4배).
            bool telegraph = boss.Phase == BossPhase.SlamTelegraph;
            float pulseSpeed = 1f + 3f * boss.TelegraphProgress;
            for (int i = 0; i < views.Count; i++)
            {
                if (i < parts.Count) Refresh(views[i], parts[i], pulseSpeed);
                else SetActive(views[i].gameObject, false);
            }

            RefreshWarning(boss, telegraph, pulseSpeed);
        }

        private void Refresh(BossPartView view, BossPart part, float pulseSpeed)
        {
            Vector3 screen = viewCamera.WorldToScreenPoint(part.MarkPoint);
            bool showBar = part.Destructible && !part.IsDestroyed;
            bool show = screen.z > 0f && (showBar || part.Marked);
            SetActive(view.gameObject, show);
            if (!show) return;

            view.Rect.position = (Vector2)screen;

            // 저지 표적: 부위의 화면 크기에 맞춘 원. 남은 저지 게이지만큼 밝은 호가 남는다.
            SetActive(view.Ring.gameObject, part.Marked);
            if (part.Marked)
            {
                Vector3 edge = viewCamera.WorldToScreenPoint(part.MarkPoint + viewCamera.transform.right * part.MarkRadius);
                float diameter = Mathf.Max(minRingSize, 2f * Mathf.Abs(edge.x - screen.x) / canvasScale);
                view.Ring.sizeDelta = new Vector2(diameter, diameter);
                view.RingFill.fillAmount = part.MarkRatio;
                view.RingPulse.SetBool(PulseId, true);
                view.RingPulse.speed = pulseSpeed;
            }

            // 이름은 팔에만 띄운다. 본체의 저지 지점은 서로 가까워 이름이 겹치므로 원만 보인다.
            SetActive(view.Label.gameObject, part.Destructible);
            view.Label.text = part.DisplayName;
            view.Label.color = part.Marked ? markedLabelColor : Color.white;
            SetActive(view.HealthBar, showBar);
            if (showBar)
            {
                view.HealthFill.fillAmount = part.HealthRatio;
                view.HealthFill.color = part.Stage == BossPartStage.Damaged ? damagedColor : healthColor;
            }
        }

        private void RefreshWarning(BossBrain boss, bool telegraph, float pulseSpeed)
        {
            Vector3 screen = viewCamera.WorldToScreenPoint(boss.WarningPoint);
            bool show = telegraph && screen.z > 0f;
            SetActive(warning.gameObject, show);
            if (!show) return;

            warning.position = (Vector2)screen;
            warningText.text = "! WARNING !\n<size=58>" + boss.TelegraphTimeLeft.ToString("0.0") + "</size>";
            warningPulse.SetBool(PulseId, true);
            warningPulse.speed = pulseSpeed;
        }

        private void HideAll()
        {
            foreach (BossPartView view in views) SetActive(view.gameObject, false);
            SetActive(warning.gameObject, false);
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go.activeSelf != active) go.SetActive(active);
        }
    }
}
