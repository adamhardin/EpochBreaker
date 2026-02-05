using UnityEngine;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Follows the player horizontally with smooth tracking.
    /// Clamped to level bounds. Vertical tracking with dead zone.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        private Transform _target;
        private LevelRenderer _tilemapRenderer;
        private Camera _camera;

        private float _smoothSpeedX = 8f;
        private float _smoothSpeedY = 4f;
        private float _verticalDeadZone = 1.5f;
        private float _lookAheadX = 1.5f;

        public void Initialize(Transform target, LevelRenderer tilemapRenderer)
        {
            _target = target;
            _tilemapRenderer = tilemapRenderer;
            _camera = GetComponent<Camera>();
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

            // Vertical: follow with dead zone
            float newY = currentPos.y;
            float vertDiff = targetPos.y - currentPos.y;
            if (Mathf.Abs(vertDiff) > _verticalDeadZone)
            {
                float targetY = targetPos.y - Mathf.Sign(vertDiff) * _verticalDeadZone;
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

            transform.position = new Vector3(newX, newY, -10f);
        }
    }
}
