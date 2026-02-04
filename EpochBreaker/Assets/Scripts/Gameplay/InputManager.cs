using UnityEngine;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Centralized input state. Supports keyboard (editor) and touch (mobile).
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

        /// <summary>True on the frame pause was pressed</summary>
        public static bool PausePressed { get; set; }

        /// <summary>
        /// Call once per frame from GameManager.Update() to read keyboard input.
        /// Touch input is set directly by TouchControlsUI.
        /// </summary>
        public static void UpdateKeyboard()
        {
            // Horizontal
            float kb = 0f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                kb -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                kb += 1f;

            // Keyboard always sets MoveX (touch overrides via direct assignment)
            MoveX = kb;

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                JumpPressed = true;
            JumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

            // Attack / target cycle
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.J))
                AttackPressed = true;

            // Pause
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                PausePressed = true;
        }

        /// <summary>
        /// Call at end of frame to clear single-frame inputs.
        /// </summary>
        public static void LateReset()
        {
            JumpPressed = false;
            AttackPressed = false;
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
            PausePressed = false;
        }
    }
}
