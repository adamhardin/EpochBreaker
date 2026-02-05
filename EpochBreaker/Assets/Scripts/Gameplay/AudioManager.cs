using UnityEngine;
using System.Collections.Generic;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Manages audio playback. Lives on the GameManager GameObject (DontDestroyOnLoad).
    /// One looping AudioSource for music, pooled AudioSources for SFX.
    /// Uses PlayOneShot to prevent sounds cutting each other off.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _musicSource;
        private AudioSource[] _sfxSources;
        private int _sfxIndex;
        private const int SFX_POOL_SIZE = 8;

        // Prevent same clip from rapid-fire stacking
        private Dictionary<int, float> _lastPlayTime = new Dictionary<int, float>();
        private const float MIN_REPEAT_INTERVAL = 0.05f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = 0.5f;
            _musicSource.playOnAwake = false;

            _sfxSources = new AudioSource[SFX_POOL_SIZE];
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                _sfxSources[i] = gameObject.AddComponent<AudioSource>();
                _sfxSources[i].volume = 0.7f;
                _sfxSources[i].playOnAwake = false;
            }
        }

        public static void PlaySFX(AudioClip clip)
        {
            if (Instance == null || clip == null) return;

            // Prevent same clip from stacking within a short window
            int clipId = clip.GetInstanceID();
            float now = Time.unscaledTime;
            if (Instance._lastPlayTime.TryGetValue(clipId, out float last) &&
                now - last < MIN_REPEAT_INTERVAL)
                return;
            Instance._lastPlayTime[clipId] = now;

            // PlayOneShot doesn't interrupt other sounds on the same source
            var src = Instance._sfxSources[Instance._sfxIndex];
            Instance._sfxIndex = (Instance._sfxIndex + 1) % SFX_POOL_SIZE;
            src.PlayOneShot(clip);
        }

        public static void PlayMusic(AudioClip clip)
        {
            if (Instance == null || clip == null) return;
            if (Instance._musicSource.clip == clip && Instance._musicSource.isPlaying) return;
            Instance._musicSource.clip = clip;
            Instance._musicSource.Play();
        }

        public static void StopMusic()
        {
            if (Instance == null) return;
            Instance._musicSource.Stop();
        }
    }
}
