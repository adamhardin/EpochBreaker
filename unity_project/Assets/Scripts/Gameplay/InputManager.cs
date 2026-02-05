using UnityEngine;
using UnityEngine.InputSystem;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Centralized input state using the new Input System.
    /// Supports keyboard, gamepad, and touch (via on-screen controls).
    /// Touch controls feed into these values via TouchControlsUI.
    /// Poll these values from PlayerController and other systems.
    /// </summary>
    public static class InputManager
    {
        /// <summary>Horizontal movement: -1 (left), 0 (none), 1 (right)</summary>
        public static float MoveX { get; set; }

        /// <summary>True on the frame jump was pressed</summary>
        public static bool JumpPressed { get; set; }

        /// <summary>True while jump is held (variable height)</summary>
        public static bool JumpHeld { get; set; }

        /// <summary>True on the frame attack/target-cycle was pressed</summary>
        public static bool AttackPressed { get; set; }

        /// <summary>True on the frame stomp/down was pressed (while airborne → ground pound)</summary>
        public static bool StompPressed { get; set; }

        /// <summary>True on the frame pause was pressed</summary>
        public static bool PausePressed { get; set; }

        // Input actions loaded from asset
        private static InputActionAsset _inputAsset;
        private static InputAction _moveAction;
        private static InputAction _jumpAction;
        private static InputAction _attackAction;
        private static InputAction _stompAction;
        private static InputAction _pauseAction;

        private static bool _initialized;

        /// <summary>
        /// Initialize the Input System. Called automatically on first use.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            // Load the input action asset from Resources or Settings folder
            _inputAsset = Resources.Load<InputActionAsset>("EpochBreakerInput");

            if (_inputAsset == null)
            {
                // Fallback: try loading from Settings folder (requires moving to Resources)
                Debug.LogWarning("InputManager: Could not load EpochBreakerInput asset from Resources. Using fallback keyboard input.");
                _initialized = true;
                return;
            }

            var gameplay = _inputAsset.FindActionMap("Gameplay");
            if (gameplay == null)
            {
                Debug.LogError("InputManager: Could not find Gameplay action map!");
                _initialized = true;
                return;
            }

            _moveAction = gameplay.FindAction("Move");
            _jumpAction = gameplay.FindAction("Jump");
            _attackAction = gameplay.FindAction("Attack");
            _stompAction = gameplay.FindAction("Stomp");
            _pauseAction = gameplay.FindAction("Pause");

            // Enable all actions
            gameplay.Enable();

            _initialized = true;
        }

        /// <summary>
        /// Call once per frame from GameManager.Update() to read input.
        /// Touch input is set directly by TouchControlsUI.
        /// </summary>
        public static void UpdateInput()
        {
            if (!_initialized)
                Initialize();

            // If using Input System actions
            if (_moveAction != null)
            {
                // Read move value directly from action
                MoveX = _moveAction.ReadValue<float>();

                // Jump (button)
                if (_jumpAction.WasPressedThisFrame())
                    JumpPressed = true;
                JumpHeld = _jumpAction.IsPressed();

                // Attack
                if (_attackAction.WasPressedThisFrame())
                    AttackPressed = true;

                // Stomp
                if (_stompAction.WasPressedThisFrame())
                    StompPressed = true;

                // Pause
                if (_pauseAction.WasPressedThisFrame())
                    PausePressed = true;
            }
            else
            {
                // Fallback to legacy Input (in case asset failed to load)
                UpdateLegacyKeyboard();
            }
        }

        /// <summary>
        /// Fallback keyboard input using legacy Input class.
        /// Used only if InputActionAsset fails to load.
        /// </summary>
        private static void UpdateLegacyKeyboard()
        {
            // Horizontal
            float kb = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    kb -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    kb += 1f;
            }

            MoveX = kb;

            // Jump
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.wKey.wasPressedThisFrame ||
                    Keyboard.current.upArrowKey.wasPressedThisFrame)
                    JumpPressed = true;
                JumpHeld = Keyboard.current.spaceKey.isPressed ||
                           Keyboard.current.wKey.isPressed ||
                           Keyboard.current.upArrowKey.isPressed;
            }

            // Attack
            if (Keyboard.current != null)
            {
                if (Keyboard.current.xKey.wasPressedThisFrame ||
                    Keyboard.current.jKey.wasPressedThisFrame)
                    AttackPressed = true;
            }

            // Stomp
            if (Keyboard.current != null)
            {
                if (Keyboard.current.sKey.wasPressedThisFrame ||
                    Keyboard.current.downArrowKey.wasPressedThisFrame)
                    StompPressed = true;
            }

            // Pause
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame ||
                    Keyboard.current.pKey.wasPressedThisFrame)
                    PausePressed = true;
            }
        }

        /// <summary>
        /// Call at end of frame to clear single-frame inputs.
        /// </summary>
        public static void LateReset()
        {
            JumpPressed = false;
            AttackPressed = false;
            StompPressed = false;
            PausePressed = false;
            // MoveX and JumpHeld persist until next frame's update
        }

        /// <summary>
        /// Reset all input state. Called on state transitions.
        /// </summary>
        public static void Clear()
        {
            MoveX = 0f;
            JumpPressed = false;
            JumpHeld = false;
            AttackPressed = false;
            StompPressed = false;
            PausePressed = false;
        }

        /// <summary>
        /// Clean up when the game ends.
        /// </summary>
        public static void Dispose()
        {
            if (_inputAsset != null)
            {
                _inputAsset.Disable();
            }
            _initialized = false;
        }
    }
}
