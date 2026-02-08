using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Follows the player horizontally with smooth tracking.
    /// Vertical offset keeps player in bottom ~30% of screen (side-scroller standard).
    /// Clamped to level bounds.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        private Transform _target;
        private LevelRenderer _tilemapRenderer;
        private Camera _camera;

        private float _smoothSpeedX = 8f;
        private float _smoothSpeedY = 5f;
        private float _verticalDeadZone = 1.0f;
        private float _lookAheadX = 1.5f;

        // Vertical offset: camera center sits above the player so the player
        // appears near the bottom of the screen. With ortho size 7 (14 units
        // visible), offset 2.8 puts the player at (7-2.8)/14 = 30% from bottom.
        private float _verticalOffset = 2.8f;

        // Trauma-based screen shake
        private float _trauma;
        private const float TRAUMA_DECAY_SPEED = 2.5f; // trauma per second
        private float _shakeTime; // running time for Perlin noise seed

        // Level intro pan
        private Coroutine _introCoroutine;

        // Boss arena camera lock
        private bool _arenaLocked;
        private float _arenaMinX;
        private float _arenaMaxX;
        private const float ARENA_ORTHO_SIZE = 8f;
        private const float NORMAL_ORTHO_SIZE = 7f;
        private float _targetOrthoSize = NORMAL_ORTHO_SIZE;

        private void Awake()
        {
            // Always claim Instance — newest camera wins.
            // Old camera's OnDestroy will check Instance != this and skip nulling.
            // This avoids the race condition where Destroy() is deferred to end-of-frame
            // but Awake() fires immediately, causing the new controller to self-destruct.
            Instance = this;
        }

        private void OnDestroy()
        {
            // Only null Instance if WE are still the active singleton.
            // If a newer camera replaced us, Instance != this, so we don't touch it.
            if (Instance == this)
                Instance = null;
        }

        public void Initialize(Transform target, LevelRenderer tilemapRenderer)
        {
            _target = target;
            _tilemapRenderer = tilemapRenderer;
            _camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Add trauma to the screen shake system. Trauma is clamped to [0,1].
        /// Shake magnitude = trauma^2 for a non-linear, satisfying feel.
        /// </summary>
        public void AddTrauma(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        /// <summary>
        /// Legacy convenience: converts intensity/duration to trauma amount.
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            AddTrauma(intensity * 2f);
        }

        /// <summary>
        /// Lock camera within boss arena bounds. Slight zoom out for full arena view.
        /// </summary>
        public void LockToArena(float minX, float maxX)
        {
            _arenaLocked = true;
            _arenaMinX = minX;
            _arenaMaxX = maxX;
            _targetOrthoSize = ARENA_ORTHO_SIZE;
        }

        /// <summary>
        /// Release arena camera lock (on boss death).
        /// </summary>
        public void UnlockArena()
        {
            _arenaLocked = false;
            _targetOrthoSize = NORMAL_ORTHO_SIZE;
        }

        /// <summary>
        /// Temporarily override target for cinematic pans (e.g., boss intro).
        /// Call with null to restore player tracking.
        /// </summary>
        public void SetTemporaryTarget(Transform target)
        {
            _target = target ?? _target;
        }

        /// <summary>
        /// Restore the original player target after cinematic.
        /// </summary>
        public void RestorePlayerTarget()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                _target = player.transform;
        }

        /// <summary>
        /// Start the level intro pan: camera sweeps ahead then returns to player.
        /// </summary>
        public void StartLevelIntro()
        {
            if (_introCoroutine != null) StopCoroutine(_introCoroutine);
            _introCoroutine = StartCoroutine(LevelIntroPan());
        }

        private System.Collections.IEnumerator LevelIntroPan()
        {
            // Wait one frame so Start() and physics have time to settle
            yield return null;

            if (_target == null) yield break;

            float savedSmoothX = _smoothSpeedX;
            _smoothSpeedX = 3f; // Slower pan for cinematic

            var tempGO = new GameObject("IntroPanTarget");
            tempGO.transform.position = _target.position + new Vector3(25f, 2f, 0f);

            Transform savedTarget = _target;
            _target = tempGO.transform;

            yield return new WaitForSecondsRealtime(0.6f);

            // Return to player — guard against savedTarget being destroyed during pan
            if (savedTarget != null)
            {
                _target = savedTarget;
            }
            else
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) _target = player.transform;
            }
            _smoothSpeedX = 5f;

            yield return new WaitForSecondsRealtime(0.5f);

            _smoothSpeedX = savedSmoothX;
            if (tempGO != null) Destroy(tempGO);
            _introCoroutine = null;
        }

        /// <summary>
        /// Pan camera to a world position, hold briefly, then return to player.
        /// Used for portal activation cinematic after boss death.
        /// Any player input during the pan snaps back immediately.
        /// </summary>
        public void PanToPosition(Vector3 worldPos, float panDuration = 1.0f, float holdDuration = 0.5f,
            System.Action onArrived = null)
        {
            if (_introCoroutine != null) StopCoroutine(_introCoroutine);
            _introCoroutine = StartCoroutine(PanToPositionCoroutine(worldPos, panDuration, holdDuration, onArrived));
        }

        private System.Collections.IEnumerator PanToPositionCoroutine(Vector3 worldPos, float panDuration,
            float holdDuration, System.Action onArrived)
        {
            yield return null;

            if (_target == null) yield break;

            Transform savedTarget = _target;
            float savedSmoothX = _smoothSpeedX;

            var tempGO = new GameObject("PortalPanTarget");
            tempGO.transform.position = worldPos + new Vector3(0f, _verticalOffset, 0f);
            _target = tempGO.transform;
            _smoothSpeedX = 4f;

            // Pan to position (skippable on input)
            float elapsed = 0f;
            while (elapsed < panDuration)
            {
                if (InputManager.JumpPressed || InputManager.AttackPressed || InputManager.PausePressed)
                    break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            onArrived?.Invoke();

            // Hold briefly (skippable)
            elapsed = 0f;
            while (elapsed < holdDuration)
            {
                if (InputManager.JumpPressed || InputManager.AttackPressed || InputManager.PausePressed)
                    break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Return to player
            if (savedTarget != null)
                _target = savedTarget;
            else
                RestorePlayerTarget();
            _smoothSpeedX = 5f;

            yield return new WaitForSeconds(0.5f);

            _smoothSpeedX = savedSmoothX;
            if (tempGO != null) Destroy(tempGO);
            _introCoroutine = null;
        }

        private void LateUpdate()
        {
            if (_target == null || _camera == null) return;
            // Guard against target destroyed mid-frame (Unity null check)
            if (!_target.gameObject) return;

            Vector3 currentPos = transform.position;
            Vector3 targetPos = _target.position;

            // Horizontal: smooth follow with look-ahead
            float lookAhead = 0f;
            if (Mathf.Abs(InputManager.MoveX) > 0.1f)
                lookAhead = InputManager.MoveX * _lookAheadX;

            float targetX = targetPos.x + lookAhead;
            float newX = Mathf.Lerp(currentPos.x, targetX, _smoothSpeedX * Time.deltaTime);

            // Vertical: offset target upward so player is in bottom portion of screen
            float offsetTargetY = targetPos.y + _verticalOffset;

            float newY = currentPos.y;
            float vertDiff = offsetTargetY - currentPos.y;
            if (Mathf.Abs(vertDiff) > _verticalDeadZone)
            {
                float targetY = offsetTargetY - Mathf.Sign(vertDiff) * _verticalDeadZone;
                newY = Mathf.Lerp(currentPos.y, targetY, _smoothSpeedY * Time.deltaTime);
            }

            // Smooth zoom transition
            if (Mathf.Abs(_camera.orthographicSize - _targetOrthoSize) > 0.01f)
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetOrthoSize, 3f * Time.deltaTime);

            // Clamp to level bounds (or arena bounds if locked)
            if (_tilemapRenderer != null && _tilemapRenderer.LevelWidth > 0)
            {
                float halfHeight = _camera.orthographicSize;
                float halfWidth = halfHeight * _camera.aspect;

                float minX = halfWidth;
                float maxX = _tilemapRenderer.LevelWidth - halfWidth;
                float minY = halfHeight;
                float maxY = _tilemapRenderer.LevelHeight - halfHeight;

                // Override X bounds with arena if locked
                if (_arenaLocked)
                {
                    minX = Mathf.Max(minX, _arenaMinX + halfWidth);
                    maxX = Mathf.Min(maxX, _arenaMaxX - halfWidth);
                }

                newX = Mathf.Clamp(newX, minX, Mathf.Max(minX, maxX));
                newY = Mathf.Clamp(newY, minY, Mathf.Max(minY, maxY));
            }

            // Apply trauma-based screen shake (Perlin noise for smooth motion)
            // Multiplied by AccessibilityManager.ScreenShakeIntensity (Sprint 9)
            float shakeX = 0f, shakeY = 0f;
            if (_trauma > 0f)
            {
                float shakeMultiplier = AccessibilityManager.Instance != null
                    ? AccessibilityManager.Instance.ScreenShakeIntensity : 1f;
                _shakeTime += Time.unscaledDeltaTime * 50f; // fast noise scroll
                float magnitude = _trauma * _trauma * 0.6f * shakeMultiplier; // quadratic falloff, scaled by setting
                shakeX = (Mathf.PerlinNoise(_shakeTime, 0f) - 0.5f) * 2f * magnitude;
                shakeY = (Mathf.PerlinNoise(0f, _shakeTime) - 0.5f) * 2f * magnitude;
                _trauma = Mathf.Max(0f, _trauma - TRAUMA_DECAY_SPEED * Time.unscaledDeltaTime);
            }

            transform.position = new Vector3(newX + shakeX, newY + shakeY, -10f);
        }
    }
}
