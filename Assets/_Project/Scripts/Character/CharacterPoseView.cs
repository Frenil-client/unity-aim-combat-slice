using UnityEngine;

namespace AimCombat.Character
{
    public enum CharacterPose
    {
        Cover,
        Fire,
        Reload,
    }

    /// <summary>
    /// 엄폐, 사격, 재장전 포즈 그림을 갈아 끼운다.
    /// 포즈마다 그림이 통째로 다르므로 보간하지 않고 바꾸고, 바뀌는 순간에만 짧게 눌렀다 펴서 이어 보이게 한다.
    /// </summary>
    public sealed class CharacterPoseView : MonoBehaviour
    {
        [Header("포즈")]
        [SerializeField] private GameObject coverRoot;
        [SerializeField] private GameObject fireRoot;
        [SerializeField] private GameObject reloadRoot;
        [SerializeField] private CharacterPose initialPose = CharacterPose.Cover;

        [Header("전환")]
        [Tooltip("포즈가 바뀔 때 눌렀다 펴는 대상. 발 기준점에 있어야 바닥에서 눌린다.")]
        [SerializeField] private Transform squashTarget;
        [Tooltip("바뀌는 순간 세로로 눌리는 비율.")]
        [Range(0f, 0.3f)]
        [SerializeField] private float squashAmount = 0.08f;
        [SerializeField] private float squashDuration = 0.12f;

        private float squashTime;

        public CharacterPose Pose { get; private set; }

        /// <summary>엄폐 포즈의 전신 그림. 컷인 일러스트가 준비되기 전에 버스트 컷인의 임시 그림으로 쓴다.</summary>
        public Sprite CoverSprite
        {
            get
            {
                SpriteRenderer renderer = coverRoot != null ? coverRoot.GetComponentInChildren<SpriteRenderer>(true) : null;
                return renderer != null ? renderer.sprite : null;
            }
        }

        private void Awake()
        {
            Pose = initialPose;
            Apply();
            squashTime = squashDuration;
        }

        public void SetPose(CharacterPose pose)
        {
            if (pose == Pose) return;
            Pose = pose;
            Apply();
            squashTime = 0f;
        }

        private void Apply()
        {
            coverRoot.SetActive(Pose == CharacterPose.Cover);
            fireRoot.SetActive(Pose == CharacterPose.Fire);
            reloadRoot.SetActive(Pose == CharacterPose.Reload);
        }

        private void Update()
        {
            if (squashTarget == null || squashTime >= squashDuration) return;

            squashTime += Time.deltaTime;
            float t = squashDuration > 0f ? Mathf.Clamp01(squashTime / squashDuration) : 1f;

            // 처음에 가장 많이 눌리고 빠르게 풀린다. 넓이를 대략 유지하도록 가로는 반대로 늘린다.
            float squash = squashAmount * (1f - t) * (1f - t);
            squashTarget.localScale = new Vector3(1f + squash * 0.5f, 1f - squash, 1f);
        }
    }
}
