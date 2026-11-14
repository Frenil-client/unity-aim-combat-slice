using AimCombat.Battle;
using AimCombat.Weapons;
using UnityEngine;

namespace AimCombat.Range
{
    /// <summary>
    /// 사격장 표적. 맞으면 번쩍이고 살짝 커졌다 돌아온다. 좌우나 상하로 왕복시킬 수 있다.
    /// 조준과 명중을 눈으로 확인하는 용도이고 체력이나 피해 계산은 없다. 관통탄은 표적을 꿰뚫고 지나간다.
    /// </summary>
    public sealed class RangeTarget : MonoBehaviour, IPierceable, IHittable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("이동")]
        [Tooltip("처음 위치에서 양쪽으로 왕복하는 폭. 0이면 고정 표적.")]
        [SerializeField] private Vector3 moveOffset;
        [SerializeField] private float movePeriod = 3f;

        [Header("명중 반응")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.12f;
        [Tooltip("맞는 순간 커지는 비율.")]
        [SerializeField] private float punchScale = 0.12f;

        private Vector3 startPosition;
        private Vector3 startScale;
        private Color[] baseColors;
        private MaterialPropertyBlock block;
        private float flash;
        private float punch;

        public int HitCount { get; private set; }

        private void Awake()
        {
            startPosition = transform.position;
            startScale = transform.localScale;
            block = new MaterialPropertyBlock();
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                baseColors[i] = renderers[i].sharedMaterial.GetColor(BaseColorId);
            }
        }

        public float Defense => 0f;

        public void TakeHit(float damage, float multiplier, Vector3 point) => Hit(multiplier);

        /// <summary>맞았다. multiplier 는 피해 배수로, 클수록 크게 튄다(최대 3배).</summary>
        public void Hit(float multiplier = 1f)
        {
            HitCount++;
            flash = 1f;
            punch = Mathf.Max(punch, Mathf.Clamp(multiplier, 1f, 3f));
        }

        private void Update()
        {
            if (movePeriod > 0f && moveOffset != Vector3.zero)
            {
                float wave = Mathf.Sin(Time.time * (2f * Mathf.PI / movePeriod));
                transform.position = startPosition + moveOffset * wave;
            }

            if (flash <= 0f) return;

            flash = flashDuration > 0f ? Mathf.Max(0f, flash - Time.deltaTime / flashDuration) : 0f;
            if (flash <= 0f) punch = 0f;
            transform.localScale = startScale * (1f + punchScale * punch * flash);
            for (int i = 0; i < renderers.Length; i++)
            {
                block.SetColor(BaseColorId, Color.Lerp(baseColors[i], flashColor, flash));
                renderers[i].SetPropertyBlock(block);
            }
        }
    }
}
