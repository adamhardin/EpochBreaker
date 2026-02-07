using UnityEngine;
using System.Collections.Generic;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages audio playback. Lives on the GameManager GameObject (DontDestroyOnLoad).
    /// One looping AudioSource for music, pooled AudioSources for SFX.
    /// Uses PlayOneShot to prevent sounds cutting each other off.
    /// Volume settings persisted to PlayerPrefs.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _musicSource;
        private AudioSource[] _sfxSources;
        private int _sfxIndex;
        private const int SFX_POOL_SIZE = 8;

        // Volume settings (0-1 range)
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private float _weaponVolume = 1f;

        private const float BASE_MUSIC_VOLUME = 0.15f;
        private const float BASE_SFX_VOLUME = 0.25f;

        private const string PREF_MUSIC_VOL = "EpochBreaker_MusicVolume";
        private const string PREF_SFX_VOL = "EpochBreaker_SFXVolume";
        private const string PREF_WEAPON_VOL = "EpochBreaker_WeaponVolume";

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                _musicSource.volume = BASE_MUSIC_VOLUME * _musicVolume;
                PlayerPrefs.SetFloat(PREF_MUSIC_VOL, _musicVolume);
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SFX_VOL, _sfxVolume);
            }
        }

        public float WeaponVolume
        {
            get => _weaponVolume;
            set
            {
                _weaponVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_WEAPON_VOL, _weaponVolume);
            }
        }

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

            // Load saved volume settings
            _musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOL, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 1f);
            _weaponVolume = PlayerPrefs.GetFloat(PREF_WEAPON_VOL, 1f);

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = BASE_MUSIC_VOLUME * _musicVolume;
            _musicSource.playOnAwake = false;

            _sfxSources = new AudioSource[SFX_POOL_SIZE];
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                _sfxSources[i] = gameObject.AddComponent<AudioSource>();
                _sfxSources[i].volume = BASE_SFX_VOLUME;
                _sfxSources[i].playOnAwake = false;
            }
        }

        public static void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (Instance == null || clip == null) return;

            // Prevent same clip from stacking within a short window
            int clipId = clip.GetInstanceID();
            float now = Time.unscaledTime;
            if (Instance._lastPlayTime.TryGetValue(clipId, out float last) &&
                now - last < MIN_REPEAT_INTERVAL)
                return;
            Instance._lastPlayTime[clipId] = now;

            var src = Instance._sfxSources[Instance._sfxIndex];
            Instance._sfxIndex = (Instance._sfxIndex + 1) % SFX_POOL_SIZE;
            src.PlayOneShot(clip, volume * Instance._sfxVolume);
        }

        public static void PlayWeaponSFX(AudioClip clip, float volume = 1f)
        {
            if (Instance == null || clip == null) return;

            int clipId = clip.GetInstanceID();
            float now = Time.unscaledTime;
            if (Instance._lastPlayTime.TryGetValue(clipId, out float last) &&
                now - last < MIN_REPEAT_INTERVAL)
                return;
            Instance._lastPlayTime[clipId] = now;

            var src = Instance._sfxSources[Instance._sfxIndex];
            Instance._sfxIndex = (Instance._sfxIndex + 1) % SFX_POOL_SIZE;
            // ±10% pitch randomization for weapon fire variety
            src.pitch = Random.Range(0.9f, 1.1f);
            src.PlayOneShot(clip, volume * Instance._weaponVolume);
            src.pitch = 1f; // Reset for next use
        }

        /// <summary>
        /// Play SFX with pitch variation. Used for combat hits to add variety.
        /// </summary>
        public static void PlaySFXPitched(AudioClip clip, float pitchMin = 0.9f, float pitchMax = 1.1f, float volume = 1f)
        {
            if (Instance == null || clip == null) return;

            int clipId = clip.GetInstanceID();
            float now = Time.unscaledTime;
            if (Instance._lastPlayTime.TryGetValue(clipId, out float last) &&
                now - last < MIN_REPEAT_INTERVAL)
                return;
            Instance._lastPlayTime[clipId] = now;

            var src = Instance._sfxSources[Instance._sfxIndex];
            Instance._sfxIndex = (Instance._sfxIndex + 1) % SFX_POOL_SIZE;
            src.pitch = Random.Range(pitchMin, pitchMax);
            src.PlayOneShot(clip, volume * Instance._sfxVolume);
            src.pitch = 1f;
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
