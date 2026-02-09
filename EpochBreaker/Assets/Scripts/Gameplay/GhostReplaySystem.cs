using System.Collections.Generic;
using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Records player position samples during gameplay and plays them back as a ghost overlay.
    /// Attach to the player GameObject during level loading.
    /// Ghost data is stored in PlayerPrefs as a compact string: "x1,y1;x2,y2;..."
    /// Only the best run's ghost is kept per level code.
    /// </summary>
    public class GhostReplaySystem : MonoBehaviour
    {
        /// <summary>
        /// Master toggle for ghost replay. Disabled for Build 036 as an audio debugging
        /// measure — ghost I/O and object spawning may contribute to boss Phase 3 squeal.
        /// Flip to true to re-enable the entire system.
        /// </summary>
        public const bool ENABLED = false;

        private const float SAMPLE_INTERVAL = 0.2f;
        private const string GHOST_PREFS_PREFIX = "Ghost_";
        private const string GHOST_INDEX_KEY = "EpochBreaker_GhostIndex";
        private const int MAX_GHOST_ENTRIES = 50;
        private const float GHOST_ALPHA = 0.5f;

        // Recording state
        private bool _recording;
        private float _recordTimer;
        private List<Vector2> _samples = new List<Vector2>();

        // Playback state
        private bool _playing;
        private float _playbackTime;
        private List<Vector2> _ghostSamples;
        private GameObject _ghostObj;
        private SpriteRenderer _ghostRenderer;

        /// <summary>
        /// True if currently recording player positions.
        /// </summary>
        public bool IsRecording => _recording;

        /// <summary>
        /// True if ghost playback is active.
        /// </summary>
        public bool IsPlaying => _playing;

        /// <summary>
        /// The recorded samples from the current run.
        /// </summary>
        public List<Vector2> RecordedSamples => _samples;

        /// <summary>
        /// Start recording player position samples.
        /// Called automatically when attached to player at level start.
        /// </summary>
        public void StartRecording()
        {
            if (!ENABLED) return;
            _recording = true;
            _recordTimer = 0f;
            _samples.Clear();
            // Record initial position
            _samples.Add((Vector2)transform.position);
        }

        /// <summary>
        /// Stop recording and return the sample data.
        /// </summary>
        public List<Vector2> StopRecording()
        {
            _recording = false;
            return _samples;
        }

        /// <summary>
        /// Save the current recording as the ghost for a level code,
        /// but only if the given score beats the previously stored best.
        /// </summary>
        public void SaveGhost(string levelCode, int score)
        {
            if (!ENABLED) return;
            string key = GHOST_PREFS_PREFIX + levelCode;

            // Check existing best score
            int existingBest = PlayerPrefs.GetInt(key + "_score", 0);
            if (score <= existingBest && existingBest > 0)
                return; // Don't overwrite a better ghost

            // Serialize samples as compact string
            string data = SerializeSamples(_samples);
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.SetInt(key + "_score", score);

            // Track stored ghost codes and prune oldest when over cap
            UpdateGhostIndex(levelCode);

            SafePrefs.Save();
        }

        /// <summary>
        /// Maintain an index of stored ghost level codes.
        /// When the index exceeds MAX_GHOST_ENTRIES, delete the oldest entries.
        /// </summary>
        private static void UpdateGhostIndex(string levelCode)
        {
            string indexStr = PlayerPrefs.GetString(GHOST_INDEX_KEY, "");
            var codes = new List<string>();
            if (!string.IsNullOrEmpty(indexStr))
            {
                string[] parts = indexStr.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                        codes.Add(parts[i]);
                }
            }

            // Remove existing entry so it moves to the end (most recent)
            codes.Remove(levelCode);
            codes.Add(levelCode);

            // Prune oldest entries if over cap
            while (codes.Count > MAX_GHOST_ENTRIES)
            {
                string oldest = codes[0];
                codes.RemoveAt(0);
                PlayerPrefs.DeleteKey(GHOST_PREFS_PREFIX + oldest);
                PlayerPrefs.DeleteKey(GHOST_PREFS_PREFIX + oldest + "_score");
            }

            PlayerPrefs.SetString(GHOST_INDEX_KEY, string.Join(";", codes));
        }

        /// <summary>
        /// Check if a ghost exists for a given level code.
        /// </summary>
        public static bool HasGhost(string levelCode)
        {
            if (!ENABLED) return false;
            string key = GHOST_PREFS_PREFIX + levelCode;
            return !string.IsNullOrEmpty(PlayerPrefs.GetString(key, ""));
        }

        /// <summary>
        /// Get the stored ghost score for a level.
        /// </summary>
        public static int GetGhostScore(string levelCode)
        {
            return PlayerPrefs.GetInt(GHOST_PREFS_PREFIX + levelCode + "_score", 0);
        }

        /// <summary>
        /// Load ghost data for a level code and start playback.
        /// Creates a semi-transparent ghost sprite that follows the recorded path.
        /// </summary>
        public void StartPlayback(string levelCode)
        {
            if (!ENABLED) return;
            string key = GHOST_PREFS_PREFIX + levelCode;
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data))
                return;

            _ghostSamples = DeserializeSamples(data);
            if (_ghostSamples == null || _ghostSamples.Count < 2)
                return;

            _playbackTime = 0f;
            _playing = true;

            CreateGhostSprite();
        }

        /// <summary>
        /// Stop ghost playback and destroy the ghost sprite.
        /// </summary>
        public void StopPlayback()
        {
            _playing = false;
            if (_ghostObj != null)
            {
                Destroy(_ghostObj);
                _ghostObj = null;
                _ghostRenderer = null;
            }
        }

        private void Update()
        {
            if (_recording)
            {
                _recordTimer += Time.deltaTime;
                if (_recordTimer >= SAMPLE_INTERVAL)
                {
                    _recordTimer -= SAMPLE_INTERVAL;
                    _samples.Add((Vector2)transform.position);
                }
            }

            if (_playing && _ghostSamples != null && _ghostObj != null)
            {
                _playbackTime += Time.deltaTime;
                float sampleIndex = _playbackTime / SAMPLE_INTERVAL;
                int idx = (int)sampleIndex;
                float t = sampleIndex - idx;

                if (idx >= _ghostSamples.Count - 1)
                {
                    // Playback complete
                    StopPlayback();
                    return;
                }

                // Interpolate between samples for smooth movement
                Vector2 pos = Vector2.Lerp(_ghostSamples[idx], _ghostSamples[idx + 1], t);
                _ghostObj.transform.position = new Vector3(pos.x, pos.y, 0f);
            }
        }

        private void OnDestroy()
        {
            StopPlayback();
        }

        private void CreateGhostSprite()
        {
            if (_ghostObj != null)
                Destroy(_ghostObj);

            _ghostObj = new GameObject("GhostReplay");
            _ghostRenderer = _ghostObj.AddComponent<SpriteRenderer>();
            _ghostRenderer.sprite = PlaceholderAssets.GetPlayerSprite();
            _ghostRenderer.sortingOrder = 9; // Behind player (10)
            _ghostRenderer.color = new Color(1f, 1f, 1f, GHOST_ALPHA);

            if (_ghostSamples.Count > 0)
                _ghostObj.transform.position = new Vector3(_ghostSamples[0].x, _ghostSamples[0].y, 0f);
        }

        /// <summary>
        /// Serialize position samples to compact string format.
        /// Format: "x1,y1;x2,y2;..." with 2 decimal places.
        /// </summary>
        private static string SerializeSamples(List<Vector2> samples)
        {
            var sb = new System.Text.StringBuilder(samples.Count * 12);
            for (int i = 0; i < samples.Count; i++)
            {
                if (i > 0) sb.Append(';');
                sb.Append(samples[i].x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(samples[i].y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Deserialize position samples from compact string format.
        /// </summary>
        private static List<Vector2> DeserializeSamples(string data)
        {
            var result = new List<Vector2>();
            string[] pairs = data.Split(';');
            for (int i = 0; i < pairs.Length; i++)
            {
                string[] parts = pairs[i].Split(',');
                if (parts.Length != 2) continue;

                if (float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float y))
                {
                    result.Add(new Vector2(x, y));
                }
            }
            return result;
        }
    }
}
