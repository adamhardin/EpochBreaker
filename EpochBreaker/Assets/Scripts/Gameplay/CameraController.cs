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

        // Screen shake
        private float _shakeTimer;
        private float _shakeDuration;
        private float _shakeIntensity;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
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
        /// Trigger screen shake. Intensity decays linearly over duration.
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            // Only override if stronger than current shake
            if (intensity > _shakeIntensity || _shakeTimer <= 0f)
            {
                _shakeIntensity = intensity;
                _shakeTimer = duration;
                _shakeDuration = duration;
            }
        }

        private void LateUpdate()
        {
            if (_target == null || _camera == null) return;

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

            // Clamp to level bounds
            if (_tilemapRenderer != null && _tilemapRenderer.LevelWidth > 0)
            {
                float halfHeight = _camera.orthographicSize;
                float halfWidth = halfHeight * _camera.aspect;

                float minX = halfWidth;
                float maxX = _tilemapRenderer.LevelWidth - halfWidth;
                float minY = halfHeight;
                float maxY = _tilemapRenderer.LevelHeight - halfHeight;

                newX = Mathf.Clamp(newX, minX, Mathf.Max(minX, maxX));
                newY = Mathf.Clamp(newY, minY, Mathf.Max(minY, maxY));
            }

            // Apply screen shake offset
            float shakeX = 0f, shakeY = 0f;
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float decay = Mathf.Clamp01(_shakeTimer / _shakeDuration);
                float magnitude = _shakeIntensity * decay;
                shakeX = Random.Range(-magnitude, magnitude);
                shakeY = Random.Range(-magnitude, magnitude);

                if (_shakeTimer <= 0f)
                    _shakeIntensity = 0f;
            }

            transform.position = new Vector3(newX + shakeX, newY + shakeY, -10f);
        }
    }
}
