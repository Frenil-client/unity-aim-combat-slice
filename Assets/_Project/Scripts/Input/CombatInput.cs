using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AimCombat.Input
{
    /// <summary>
    /// 전투 입력 계층. 마우스와 터치를 같은 결과(조준점 화면 좌표, 방아쇠, 대상 전환)로 바꾼다.
    /// 이 값을 쓰는 화면 코드는 기기 종류를 모른다.
    /// - 마우스: 왼쪽 버튼을 누르는 동안만 조준점이 커서 위치를 따르고, 누르는 동안이 방아쇠다. 떼면 조준점은 그 자리에 남는다.
    /// - 터치: 손가락을 끌면 조준점이 그만큼 움직이고(상대 이동), 손가락이 닿아 있는 동안이 방아쇠.
    /// - 키보드: Z, X, C, V, B 로 배치 순서대로 대상 전환. 숫자 1-5 는 지금 버스트 단계에서 쓸 수 있는 아군 목록(배치 순서)의
    ///   n번째 버스트, Q 는 그 목록의 첫 번째 버스트.
    ///   Tab 으로 자동 버스트, 왼쪽 Shift 로 고른 아군의 자동 전투를 켜고 끈다. 터치는 HUD 버튼을 쓴다.
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

        /// <summary>
        /// 누르지 않을 때도 마우스 커서가 조준점을 움직이는지. 기본은 꺼짐이다(2026-11-14: 조준점은 클릭 중에만 커서를 따른다).
        /// 켜면 1단계처럼 커서를 늘 따른다. 자동 전투는 누르지 않는 동안 분대가 조준점을 직접 옮긴다(SetAim).
        /// </summary>
        public bool HoverAims { get; set; } = false;

        /// <summary>uGUI 가 아닌 화면 요소(즉시 모드 GUI 의 튜닝 패널)가 이 화면 좌표를 덮고 있는지. 덮고 있으면 그 누름은 사격으로 치지 않는다.</summary>
        public static System.Predicate<Vector2> ExtraUiHitTest;

        /// <summary>조준점을 옮긴다. 자동 전투가 손을 뗀 동안 조준점을 목표로 옮길 때 쓴다. 터치의 상대 이동은 여기서부터 이어진다.</summary>
        public void SetAim(Vector2 screenPosition)
        {
            aim = screenPosition;
            aimInitialized = true;
        }

        /// <summary>방아쇠를 누르고 있는지.</summary>
        public bool FireHeld { get; private set; }

        /// <summary>이번 프레임에 방아쇠를 새로 눌렀는지.</summary>
        public bool FirePressedThisFrame { get; private set; }

        /// <summary>이번 프레임에 요청된 대상 번호(0부터). 없으면 -1.</summary>
        public int SelectRequest { get; private set; } = -1;

        /// <summary>이번 프레임에 요청된 버스트 후보 번호(0부터, 지금 단계에서 쓸 수 있는 아군 목록의 순서). 없으면 -1.</summary>
        public int BurstRequest { get; private set; } = -1;

        /// <summary>이번 프레임에 자동 버스트 전환(Tab)을 눌렀는지.</summary>
        public bool AutoBurstTogglePressed { get; private set; }

        /// <summary>이번 프레임에 고른 아군의 자동 전투 전환(왼쪽 Shift)을 눌렀는지.</summary>
        public bool AutoBattleTogglePressed { get; private set; }


        private void Update()
        {
            if (!aimInitialized)
            {
                aim = new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
                aimInitialized = true;
            }

            bool pressed = false;
            bool touching = false;
            Vector2 touchDelta = Vector2.zero;
            Vector2 mousePosition = Vector2.zero;
            Vector2 touchPosition = Vector2.zero;
            Touchscreen touch = Touchscreen.current;
            Mouse mouse = Mouse.current;

            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                usingTouch = true;
                touching = true;
                pressed = true;
                touchDelta = touch.primaryTouch.delta.ReadValue();
                touchPosition = touch.primaryTouch.position.ReadValue();
            }
            else if (mouse != null)
            {
                mousePosition = mouse.position.ReadValue();
                // 터치를 쓰다가 마우스를 실제로 움직였을 때만 마우스로 넘어간다.
                if (!usingTouch || (mousePosition - lastMousePosition).sqrMagnitude > 1f) usingTouch = false;
                lastMousePosition = mousePosition;
                pressed = mouse.leftButton.isPressed;
            }

            if (pressed && !wasPressed)
            {
                // Input System UI 모듈에서는 인자 없는 호출이 마지막으로 움직인 포인터(마우스나 터치)를 본다.
                pressStartedOverUi = (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    || (ExtraUiHitTest != null && ExtraUiHitTest(touching ? touchPosition : mousePosition));
            }

            FirePressedThisFrame = pressed && !wasPressed && !pressStartedOverUi;
            FireHeld = pressed && !pressStartedOverUi;
            wasPressed = pressed;

            // 조준점: 누르는 동안은 손가락(상대 이동)이나 커서를 따른다. 누르지 않을 때 커서를 따라갈지는 HoverAims 가 정한다.
            // UI 버튼을 누르는 동안은 조준점을 움직이지 않는다.
            if (touching)
            {
                if (FireHeld) aim += touchDelta * touchSensitivity;
            }
            else if (!usingTouch && (FireHeld || (HoverAims && !pressed)))
            {
                aim = mousePosition;
            }

            aim.x = Mathf.Clamp(aim.x, 0f, Screen.width);
            aim.y = Mathf.Clamp(aim.y, 0f, Screen.height);

            SelectRequest = -1;
            BurstRequest = -1;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.qKey.wasPressedThisFrame) BurstRequest = 0;
            AutoBurstTogglePressed = keyboard != null && keyboard.tabKey.wasPressedThisFrame;
            AutoBattleTogglePressed = keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame;
            if (keyboard == null) return;

            // 배치 순서 5칸까지 키를 둔다. 분대가 3명이면 4, 5번째 키는 무시된다.
            for (int i = 0; i < SelectKeys.Length; i++)
            {
                if (SelectRequest < 0 && keyboard[SelectKeys[i]].wasPressedThisFrame) SelectRequest = i;
                if (BurstRequest < 0 && keyboard[BurstKeys[i]].wasPressedThisFrame) BurstRequest = i;
            }
        }

        private static readonly Key[] SelectKeys = { Key.Z, Key.X, Key.C, Key.V, Key.B };
        private static readonly Key[] BurstKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
    }
}
