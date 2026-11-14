using UnityEngine;

namespace AimCombat.Character
{
    /// <summary>
    /// 사격 포즈의 조준과 반동 표현.
    /// 몸은 고정하고, 팔꿈치 피벗에 붙은 총과 손 한 덩어리를 돌려 조준한다.
    /// 상하는 z축 회전(화면에서 총이 들리고 내려감), 좌우는 y축 회전(총이 화면 안쪽으로 틀어져 짧아 보임)으로 나눈다.
    /// 총은 정해진 범위 안에서만 돌고, 범위를 벗어난 방향은 탄 궤적이 맡는다.
    /// 전투 판정에는 관여하지 않는다. 화면에 보이는 자세만 다룬다.
    /// </summary>
    public sealed class CharacterAimRig : MonoBehaviour
    {
        [Header("파츠")]
        [SerializeField] private Transform aimPivot;
        [SerializeField] private Transform recoilPivot;
        [SerializeField] private Transform muzzle;
        [Tooltip("총과 손 파츠의 크기 배율. 팔꿈치를 기준으로 줄고 늘어난다. 실행 중에 바꿔도 바로 반영된다.")]
        [Range(0.5f, 1.2f)]
        [SerializeField] private float gunScale = 1f;

        [Header("조준 공통")]
        [Tooltip("조준점이 캐릭터 평면에서 화면 안쪽으로 이만큼 떨어져 있다고 보고 각도를 구한다(유닛). 작을수록 총이 크게 움직인다.")]
        [SerializeField] private float aimDepth = 1.2f;
        [Tooltip("목표 각도를 따라가는 빠르기. 클수록 즉시 따라간다.")]
        [SerializeField] private float aimSharpness = 18f;

        [Header("상하 (z축 회전)")]
        [Tooltip("그림에서 총열이 수평보다 올라가 있는 각도. 이 각도일 때 z축 회전이 0이다.")]
        [SerializeField] private float restAngle = 29f;
        [Tooltip("가장 아래로 내린 각도. 수평이 0. 기본 그림에서 60도 내리면 -31이다.")]
        [SerializeField] private float minAngle = -31f;
        [SerializeField] private float maxAngle = 55f;

        [Header("좌우 (y축 회전)")]
        [Tooltip("조준점이 오른쪽 멀리 있으면 0, 정면이면 90, 왼쪽이면 90을 넘는다. 이 값에서 멈춘다. 90 근처에서는 총이 선으로 보이고, 넘으면 총이 왼쪽을 향해 뒤집혀 보인다.")]
        [Range(0f, 180f)]
        [SerializeField] private float maxYaw = 160f;
        [Tooltip("계산한 좌우 각도에 곱하는 비율. 1이면 그대로.")]
        [Range(0f, 1.5f)]
        [SerializeField] private float yawScale = 1f;

        [Header("반동")]
        [Tooltip("한 발에 총이 뒤로 밀리는 거리(유닛).")]
        [SerializeField] private float kickDistance = 0.04f;
        [Tooltip("한 발에 총구가 들리는 각도.")]
        [SerializeField] private float kickAngle = 2.5f;
        [Tooltip("반동이 쌓일 수 있는 최대 배수.")]
        [SerializeField] private float maxKick = 3f;
        [Tooltip("반동이 풀리는 빠르기.")]
        [SerializeField] private float recoverSharpness = 14f;

        [Header("몸 반동")]
        [Tooltip("사격 자세 전체의 뿌리. 발 위치가 기준점이어야 발은 붙어 있고 상체만 밀린다.")]
        [SerializeField] private Transform bodyRecoilRoot;
        [Tooltip("한 발에 몸이 뒤로 젖혀지는 각도.")]
        [SerializeField] private float bodyKickAngle = 0.5f;
        [Tooltip("한 발에 몸이 세로로 눌리는 비율.")]
        [SerializeField] private float bodySquash = 0.01f;
        [Tooltip("몸 반동이 풀리는 빠르기. 총보다 느리게 두면 총이 먼저 돌아오고 몸이 뒤따른다.")]
        [SerializeField] private float bodyRecoverSharpness = 9f;

        private float targetAngle;
        private float currentAngle;
        private float targetYaw;
        private float currentYaw;
        private float kick;
        private float bodyKick;
        private Vector3 recoilRestPosition;
        private Vector3 muzzleRestPosition;

        /// <summary>총구 위치. 총구 화염과 탄 궤적의 시작점.</summary>
        public Transform Muzzle => muzzle;

        /// <summary>현재 상하 각도. 수평이 0, 위쪽이 양수.</summary>
        public float AimAngle => currentAngle;

        /// <summary>현재 좌우 각도. 그림 그대로가 0, 화면 안쪽으로 틀수록 커진다.</summary>
        public float AimYaw => currentYaw;

        private void Awake()
        {
            recoilRestPosition = recoilPivot.localPosition;
            muzzleRestPosition = muzzle.localPosition;

            targetAngle = restAngle;
            currentAngle = restAngle;
        }

        /// <summary>
        /// 월드 좌표의 한 점을 조준한다. 그 점을 캐릭터 평면에 놓고, 실제 목표는 그보다 aimDepth 만큼 안쪽에 있다고 본다.
        /// </summary>
        public void AimAt(Vector3 worldPoint)
        {
            Vector3 target = transform.InverseTransformPoint(worldPoint);
            Vector3 pivot = transform.InverseTransformPoint(aimPivot.position);

            float dx = target.x - pivot.x;
            float dy = target.y - pivot.y;

            // 좌우: 위에서 내려다본 방향. 오른쪽 멀리면 0도, 정면(화면 안쪽)이면 90도, 왼쪽이면 90도를 넘는다.
            float yaw = Mathf.Clamp(Mathf.Atan2(aimDepth, dx) * Mathf.Rad2Deg * yawScale, 0f, maxYaw);

            // 상하: 화면에 보이는 총열의 연장선이 조준점을 지나게 한다.
            // y축으로 틀면 화면에서 가로만 cos(yaw) 배로 줄어든다. 조준점의 가로를 그만큼 늘려 놓고
            // 줄어들기 전 평면에서 각도를 구하면, 줄어든 뒤 화면에서도 총열이 조준점을 향한다.
            float squash = Mathf.Cos(yaw * Mathf.Deg2Rad);
            if (Mathf.Abs(squash) < 0.05f) squash = squash < 0f ? -0.05f : 0.05f;
            float flatX = dx / squash;
            float distance = Mathf.Sqrt(flatX * flatX + dy * dy);
            float angle = Mathf.Atan2(dy, flatX) * Mathf.Rad2Deg;

            // 총열은 피벗을 지나지 않고 barrelOffset 만큼 비켜 있다. 그만큼 각도를 보정해야
            // 피벗이 아니라 총열의 연장선이 조준점을 지나고, 총구에서 긋는 궤적과 총열이 한 줄이 된다.
            float barrelOffset = BarrelOffset();
            if (distance > Mathf.Abs(barrelOffset))
            {
                angle -= Mathf.Asin(barrelOffset / distance) * Mathf.Rad2Deg;
            }

            SetAim(angle, yaw);
        }

        // 회전하지 않은 상태에서 피벗부터 총열 연장선까지의 수직 거리. 총구가 총열 방향의 왼쪽(위)에 있으면 양수.
        // 총 크기 배율을 실행 중에 바꿀 수 있어서 매번 구한다.
        private float BarrelOffset()
        {
            Vector3 muzzleFromPivot = recoilRestPosition + muzzleRestPosition * gunScale;
            float rest = restAngle * Mathf.Deg2Rad;
            return Mathf.Cos(rest) * muzzleFromPivot.y - Mathf.Sin(rest) * muzzleFromPivot.x;
        }

        /// <summary>조준 각도를 직접 지정한다. 상하는 수평이 0이고 위쪽이 양수, 좌우는 그림 그대로가 0이다.</summary>
        public void SetAim(float angle, float yaw)
        {
            targetAngle = Mathf.Clamp(angle, minAngle, maxAngle);
            targetYaw = Mathf.Clamp(yaw, 0f, maxYaw);
        }

        /// <summary>한 발의 반동을 더한다.</summary>
        public void Kick(float strength = 1f)
        {
            kick = Mathf.Min(kick + strength, maxKick);
            bodyKick = Mathf.Min(bodyKick + strength, maxKick);
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;

            // 프레임 시간에 상관없이 같은 빠르기로 줄어드는 지수 감쇠.
            float keep = Mathf.Exp(-aimSharpness * dt);
            currentAngle = Mathf.Lerp(targetAngle, currentAngle, keep);
            currentYaw = Mathf.Lerp(targetYaw, currentYaw, keep);
            kick *= Mathf.Exp(-recoverSharpness * dt);

            // z축으로 먼저 들고 내린 뒤 y축으로 튼다. y축의 음수 방향이 총구를 화면 안쪽(카메라 반대쪽)으로 보낸다.
            aimPivot.localRotation = Quaternion.Euler(0f, -currentYaw, currentAngle - restAngle);

            // 반동은 총열 축을 따라 뒤로 밀리고 총구가 들린다.
            float axis = restAngle * Mathf.Deg2Rad;
            Vector3 back = new Vector3(-Mathf.Cos(axis), -Mathf.Sin(axis), 0f);
            recoilPivot.localPosition = recoilRestPosition + back * (kickDistance * kick);
            recoilPivot.localRotation = Quaternion.Euler(0f, 0f, kickAngle * kick);
            recoilPivot.localScale = new Vector3(gunScale, gunScale, 1f);

            // 몸 반동: 발을 기준으로 뒤(왼쪽)로 살짝 젖혀지고 세로로 눌린다.
            if (bodyRecoilRoot != null)
            {
                bodyKick *= Mathf.Exp(-bodyRecoverSharpness * dt);
                bodyRecoilRoot.localRotation = Quaternion.Euler(0f, 0f, bodyKickAngle * bodyKick);
                float squash = bodySquash * bodyKick;
                bodyRecoilRoot.localScale = new Vector3(1f + squash * 0.3f, 1f - squash, 1f);
            }
        }
    }
}
