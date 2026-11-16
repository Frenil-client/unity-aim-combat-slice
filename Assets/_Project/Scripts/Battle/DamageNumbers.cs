using System.Collections.Generic;
using AimCombat.Core.Combat;
using AimCombat.Data;
using TMPro;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace AimCombat.Battle
{
    /// <summary>
    /// 피해 숫자 표시(SPEC 4절). 맞은 지점 위에 숫자를 띄워 위로 떠오르며 사라지게 한다.
    /// - 일반은 흰색, 치명타는 노란색으로 크게, 약점은 주황색으로 크게, 둘 다면 가장 크게 보인다.
    /// - 자동 사격 아군의 피해는 작고 흐리게 보여 조작 중인 아군의 숫자가 먼저 읽히게 한다.
    /// - 스킬 피해는 하늘색으로 크게, 지속 피해는 주황색으로 작게 보인다(적의 NonShotDamaged 를 구독한다).
    /// - 같은 프레임에 같은 대상을 같은 종류로 여러 번 맞히면(산탄 탄알, 연사, 광역 스킬) 한 숫자로 합친다.
    /// 숫자는 화면 좌표 UI 라서 맞은 지점을 매 프레임 화면에 투영해 따라간다. 글자 오브젝트는 미리 만들어 돌려 쓴다.
    /// </summary>
    public sealed class DamageNumbers : UIElement
    {
        private sealed class Number
        {
            public TMP_Text text;
            public RectTransform rect;
            public Vector3 world;
            public Vector2 jitter;
            public float startTime;
            public float baseSize;
            public Color color;
        }

        private sealed class Pending
        {
            public Vector3 world;
            public float amount;
            public bool critical;
            public bool weakPoint;
            public bool controlled;
            public DamageSource source;
        }

        [SerializeField] private Camera viewCamera;
        [Tooltip("글자 원본. 실행하면 같은 설정으로 여러 개를 만들어 돌려 쓴다.")]
        [SerializeField] private TMP_Text template;
        [SerializeField] private int poolSize = 48;

        [Header("움직임")]
        [SerializeField] private float lifetime = 0.75f;
        [Tooltip("사라질 때까지 떠오르는 높이(픽셀, 1080 기준).")]
        [SerializeField] private float rise = 70f;
        [Tooltip("겹치지 않게 좌우로 흩뜨리는 폭(픽셀).")]
        [SerializeField] private float jitter = 24f;
        [Tooltip("처음 나타날 때 커졌다 줄어드는 배율.")]
        [SerializeField] private float popScale = 1.5f;
        [SerializeField] private float popDuration = 0.1f;

        [Header("모양")]
        [SerializeField] private int normalSize = 34;
        [SerializeField] private int emphasisSize = 46;
        [SerializeField] private int bothSize = 56;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = new Color(1f, 0.88f, 0.2f);
        [SerializeField] private Color weakPointColor = new Color(1f, 0.5f, 0.15f);
        [SerializeField] private Color skillColor = new Color(0.45f, 0.9f, 1f);
        [SerializeField] private Color dotColor = new Color(1f, 0.62f, 0.3f);
        [Tooltip("지속 피해 숫자의 크기 배율.")]
        [SerializeField] private float dotSizeScale = 0.75f;
        [Tooltip("자동 사격 아군 숫자의 크기 배율.")]
        [SerializeField] private float autoSizeScale = 0.7f;
        [Tooltip("자동 사격 아군 숫자의 불투명도.")]
        [SerializeField] private float autoAlpha = 0.7f;

        private readonly List<Number> pool = new List<Number>();
        private readonly List<Number> active = new List<Number>();
        private readonly Dictionary<(object, DamageSource), Pending> pending = new Dictionary<(object, DamageSource), Pending>();
        private float canvasScale = 1f;

        private void Awake()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            // 원본은 프리팹(DamageNumber)이다. 미리 꺼 둔 복제본을 풀에 넣고 쓸 때 켠다.
            for (int i = 0; i < poolSize; i++)
            {
                TMP_Text text = Instantiate(template, transform);
                text.gameObject.SetActive(false);
                text.name = "Number_" + i;
                pool.Add(new Number { text = text, rect = text.rectTransform });
            }
        }

        /// <summary>
        /// 피해를 하나 알린다. target 은 합치는 기준(같은 대상, 같은 프레임이면 합친다). controlled 는 조작 중인 아군의 사격인지.
        /// </summary>
        public void Add(object target, Vector3 worldPoint, float amount, bool critical, bool weakPoint, bool controlled)
        {
            Queue(target, DamageSource.Shot, worldPoint, amount, critical, weakPoint, controlled);
        }

        private void OnEnable() => EnemyUnit.NonShotDamaged += OnNonShotDamaged;

        private void OnDisable() => EnemyUnit.NonShotDamaged -= OnNonShotDamaged;

        // 스킬과 지속 피해는 맞은 지점이 따로 없어 적의 몸 중심에 띄운다.
        private void OnNonShotDamaged(EnemyUnit enemy, DamageEvent e)
        {
            Queue(enemy, e.Kind, enemy.AimPoint, FixedConvert.ToFloat(e.Amount), e.Critical, false, true);
        }

        private void Queue(object target, DamageSource source, Vector3 worldPoint, float amount, bool critical, bool weakPoint, bool controlled)
        {
            if (pending.TryGetValue((target, source), out Pending p))
            {
                p.amount += amount;
                p.critical |= critical;
                p.weakPoint |= weakPoint;
                p.controlled |= controlled;
                return;
            }
            pending[(target, source)] = new Pending
            {
                world = worldPoint, amount = amount, critical = critical, weakPoint = weakPoint, controlled = controlled, source = source,
            };
        }

        private void LateUpdate()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            canvasScale = canvas != null ? canvas.scaleFactor : 1f;

            foreach (Pending p in pending.Values) Spawn(p);
            pending.Clear();

            float now = Time.time;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Number n = active[i];
                float t = (now - n.startTime) / lifetime;
                Vector3 screen = viewCamera.WorldToScreenPoint(n.world);
                if (t >= 1f || screen.z <= 0f)
                {
                    Release(n, i);
                    continue;
                }

                // 떠오르기는 처음에 빠르고 끝에 느리게, 사라지기는 뒤쪽 40% 에서만.
                float eased = 1f - (1f - t) * (1f - t);
                Vector2 offset = n.jitter + Vector2.up * (rise * eased);
                n.rect.position = (Vector2)screen + offset * canvasScale;

                float pop = popDuration > 0f ? Mathf.Clamp01((now - n.startTime) / popDuration) : 1f;
                float scale = Mathf.Lerp(popScale, 1f, pop);
                n.rect.localScale = new Vector3(scale, scale, 1f);

                Color color = n.color;
                color.a *= t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
                n.text.color = color;
            }
        }

        private void Spawn(Pending p)
        {
            Number n;
            if (pool.Count > 0)
            {
                n = pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
            }
            else
            {
                // 다 쓰고 있으면 가장 오래된 숫자를 다시 쓴다.
                n = active[0];
                active.RemoveAt(0);
            }

            int size = p.critical && p.weakPoint ? bothSize : p.critical || p.weakPoint ? emphasisSize : normalSize;
            Color color = p.weakPoint ? weakPointColor : p.critical ? criticalColor : normalColor;
            if (p.source == DamageSource.Skill)
            {
                size = p.critical ? bothSize : emphasisSize;
                color = skillColor;
            }
            else if (p.source == DamageSource.Dot)
            {
                size = Mathf.RoundToInt(normalSize * dotSizeScale);
                color = dotColor;
            }
            else if (!p.controlled)
            {
                size = Mathf.RoundToInt(size * autoSizeScale);
                color.a = autoAlpha;
            }

            n.world = p.world;
            n.jitter = new Vector2(Random.Range(-jitter, jitter), Random.Range(0f, jitter * 0.5f));
            n.startTime = Time.time;
            n.color = color;
            n.text.fontSize = size;
            n.text.text = Mathf.Max(1, Mathf.RoundToInt(p.amount)).ToString() + (p.critical ? "!" : "");
            n.text.color = color;
            n.text.gameObject.SetActive(true);
            n.rect.SetAsLastSibling();
            active.Add(n);
        }

        private void Release(Number n, int index)
        {
            n.text.gameObject.SetActive(false);
            active.RemoveAt(index);
            pool.Add(n);
        }
    }
}
