using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AimCombat.Input
{
    /// <summary>
    /// 전투 입력 계층. 마우스와 터치를 같은 결과(조준점 화면 좌표, 방아쇠, 대상 전환)로 바꾼다.
    /// 이 값을 쓰는 화면 코드는 기기 종류를 모른다.
    /// - 마우스: 조준점은 커서 위치, 왼쪽 버튼이 방아쇠.
    /// - 터치: 손가락을 끌면 조준점이 그만큼 움직이고(상대 이동), 손가락이 닿아 있는 동안이 방아쇠.
    /// - 키보드: 숫자 1, 2, 3 으로 대상 전환, Q 로 궁극기.
    /// UI(초상 버튼 등) 위에서 시작한 누름은 방아쇠로 치지 않는다.
    /// </summary>
    public sealed class CombatInput : MonoBehaviour
    {
        [Tooltip("터치 드래그 감도. 손가락이 화면 높이만큼 움직이면 조준점이 화면 높이의 이 배수만큼 움직인다.")]
        [SerializeField] private float touchSensitivity = 1.2f;

        private Vector2 aim;
        private bool aimInitialized;
        private bool usingTouch;
        private bool pressStartedOverUi;
        private bool wasPressed;
        private Vector2 lastMousePosition;

        /// <summary>조준점의 화면 좌표(픽셀).</summary>
        public Vector2 AimScreenPosition => aim;

        /// <summary>방아쇠를 누르고 있는지.</summary>
        public bool FireHeld { get; private set; }

        /// <summary>이번 프레임에 방아쇠를 새로 눌렀는지.</summary>
        public bool FirePressedThisFrame { get; private set; }

        /// <summary>이번 프레임에 요청된 대상 번호(0부터). 없으면 -1.</summary>
        public int SelectRequest { get; private set; } = -1;

        /// <summary>이번 프레임에 궁극기 키(Q)를 눌렀는지. 터치는 HUD 의 궁극기 버튼을 쓴다.</summary>
        public bool UltimatePressedThisFrame { get; private set; }

        private void Update()
        {
            if (!aimInitialized)
            {
                aim = new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
                aimInitialized = true;
            }

            bool pressed = false;
            Touchscreen touch = Touchscreen.current;
            Mouse mouse = Mouse.current;

            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                usingTouch = true;
                pressed = true;
                Vector2 delta = touch.primaryTouch.delta.ReadValue();
                aim += delta * touchSensitivity;
            }
            else if (mouse != null)
            {
                Vector2 position = mouse.position.ReadValue();
                // 터치를 쓰다가 마우스를 실제로 움직였을 때만 마우스로 넘어간다.
                if (!usingTouch || (position - lastMousePosition).sqrMagnitude > 1f) usingTouch = false;
                lastMousePosition = position;
                if (!usingTouch) aim = position;
                pressed = mouse.leftButton.isPressed;
            }

            aim.x = Mathf.Clamp(aim.x, 0f, Screen.width);
            aim.y = Mathf.Clamp(aim.y, 0f, Screen.height);

            if (pressed && !wasPressed)
            {
                // Input System UI 모듈에서는 인자 없는 호출이 마지막으로 움직인 포인터(마우스나 터치)를 본다.
                pressStartedOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            }

            FirePressedThisFrame = pressed && !wasPressed && !pressStartedOverUi;
            FireHeld = pressed && !pressStartedOverUi;
            wasPressed = pressed;

            SelectRequest = -1;
            Keyboard keyboard = Keyboard.current;
            UltimatePressedThisFrame = keyboard != null && keyboard.qKey.wasPressedThisFrame;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) SelectRequest = 0;
                else if (keyboard.digit2Key.wasPressedThisFrame) SelectRequest = 1;
                else if (keyboard.digit3Key.wasPressedThisFrame) SelectRequest = 2;
            }
        }
    }
}
