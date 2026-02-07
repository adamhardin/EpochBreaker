using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Simple frame-based sprite animation. Cycles through an array of sprites
    /// at a configurable frame rate. Used by player, enemies, and boss.
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Sprite[] _currentFrames;
        private float _frameInterval = 0.12f;
        private float _timer;
        private int _currentFrame;
        private bool _playing = true;

        // Animation sets
        private Sprite[] _idleFrames;
        private Sprite[] _walkFrames;
        private Sprite[] _jumpFrame;   // single frame
        private Sprite[] _fallFrame;   // single frame
        private Sprite[] _wallSlideFrames;

        public enum AnimState { Idle, Walk, Jump, Fall, WallSlide }
        private AnimState _state = AnimState.Idle;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void SetAnimations(Sprite[] idle, Sprite[] walk, Sprite[] jump, Sprite[] fall, Sprite[] wallSlide)
        {
            _idleFrames = idle;
            _walkFrames = walk;
            _jumpFrame = jump;
            _fallFrame = fall;
            _wallSlideFrames = wallSlide;
            _currentFrames = idle;
        }

        public void SetState(AnimState state)
        {
            if (_state == state) return;
            _state = state;
            _currentFrame = 0;
            _timer = 0f;

            switch (state)
            {
                case AnimState.Idle:
                    _currentFrames = _idleFrames;
                    _frameInterval = 0.5f; // Slow breathing idle
                    break;
                case AnimState.Walk:
                    _currentFrames = _walkFrames;
                    _frameInterval = 0.12f;
                    break;
                case AnimState.Jump:
                    _currentFrames = _jumpFrame;
                    _frameInterval = 1f; // Static pose
                    break;
                case AnimState.Fall:
                    _currentFrames = _fallFrame;
                    _frameInterval = 1f;
                    break;
                case AnimState.WallSlide:
                    _currentFrames = _wallSlideFrames;
                    _frameInterval = 0.15f;
                    break;
            }
        }

        /// <summary>
        /// Set a custom frame set and interval for one-off animations.
        /// </summary>
        public void PlayCustom(Sprite[] frames, float interval, bool loop = true)
        {
            _currentFrames = frames;
            _frameInterval = interval;
            _currentFrame = 0;
            _timer = 0f;
            _playing = loop;
        }

        private void Update()
        {
            if (_currentFrames == null || _currentFrames.Length == 0 || _sr == null) return;

            _timer += Time.deltaTime;
            if (_timer >= _frameInterval)
            {
                _timer -= _frameInterval;
                _currentFrame++;
                if (_currentFrame >= _currentFrames.Length)
                {
                    _currentFrame = _playing ? 0 : _currentFrames.Length - 1;
                }
                _sr.sprite = _currentFrames[_currentFrame];
            }
        }

        /// <summary>
        /// Force set to a specific frame immediately.
        /// </summary>
        public void SetFrame(Sprite sprite)
        {
            if (_sr != null)
                _sr.sprite = sprite;
        }
    }
}
