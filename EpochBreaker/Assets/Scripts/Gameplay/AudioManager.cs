using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages audio playback. Lives on the GameManager GameObject (DontDestroyOnLoad).
    /// One looping AudioSource for music, one for ambient, pooled AudioSources for SFX.
    /// Uses PlayOneShot to prevent sounds cutting each other off.
    /// Volume settings persisted to PlayerPrefs.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _musicSource;
        private AudioSource _ambientSource;
        private AudioSource[] _sfxSources;
        private int _sfxIndex;
        private const int SFX_POOL_SIZE = 8;

        // Dedicated weapon audio sources — separate pool so boss/environment SFX
        // can't starve weapon fire sounds (fixes perceived fire rate slowdown)
        private AudioSource[] _weaponSources;
        private int _weaponIndex;
        private const int WEAPON_POOL_SIZE = 3;

        // Volume settings (0-1 range)
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private float _weaponVolume = 1f;

        private const float BASE_MUSIC_VOLUME = 0.15f;
        private const float BASE_SFX_VOLUME = 0.18f;
        private const float BASE_AMBIENT_VOLUME = 0.08f;

        // Current music variant (Sprint 9: 3 variants per era group)
        private int _currentMusicVariant = 0;

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
                _ambientSource.volume = BASE_AMBIENT_VOLUME * _musicVolume;
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

        // Limit concurrent SFX to prevent audio overload / high-pitched artifacts
        private const int MAX_CONCURRENT_SFX = 3;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] AudioManager duplicate detected — destroying new instance.");
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

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.volume = BASE_AMBIENT_VOLUME * _musicVolume; // Ambient follows music volume
            _ambientSource.playOnAwake = false;

            _sfxSources = new AudioSource[SFX_POOL_SIZE];
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                _sfxSources[i] = gameObject.AddComponent<AudioSource>();
                _sfxSources[i].volume = BASE_SFX_VOLUME;
                _sfxSources[i].playOnAwake = false;
            }

            _weaponSources = new AudioSource[WEAPON_POOL_SIZE];
            for (int i = 0; i < WEAPON_POOL_SIZE; i++)
            {
                _weaponSources[i] = gameObject.AddComponent<AudioSource>();
                _weaponSources[i].volume = BASE_SFX_VOLUME;
                _weaponSources[i].playOnAwake = false;
            }

            // Runtime low-pass filter — attenuates harsh high-frequency content from
            // procedural synthesis (unfiltered noise broadband, harmonic beating, Square SFX).
            // WebGL guard: AudioLowPassFilter can throw on some browsers (build 025 fix).
            try
            {
                var lpf = gameObject.AddComponent<AudioLowPassFilter>();
                lpf.cutoffFrequency = 3000f;
            }
            catch (System.Exception)
            {
                // Silently skip on platforms that don't support audio filters
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

            // Limit concurrent SFX to prevent audio overload
            int playing = 0;
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                if (Instance._sfxSources[i].isPlaying) playing++;
            }
            if (playing >= MAX_CONCURRENT_SFX) return;

            var src = Instance._sfxSources[Instance._sfxIndex];
            Instance._sfxIndex = (Instance._sfxIndex + 1) % SFX_POOL_SIZE;
            src.pitch = 1f; // Ensure normal pitch for non-pitched SFX
            src.PlayOneShot(clip, Mathf.Min(volume * Instance._sfxVolume, 0.8f));
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

            // Use dedicated weapon pool — never starved by boss/environment SFX
            var src = Instance._weaponSources[Instance._weaponIndex];
            Instance._weaponIndex = (Instance._weaponIndex + 1) % WEAPON_POOL_SIZE;
            // Fixed pitch — pitch randomization removed to prevent beating/interference between overlapping shots
            src.pitch = 1f;
            src.PlayOneShot(clip, Mathf.Min(volume * Instance._weaponVolume, 0.8f));
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

            // Limit concurrent SFX
            int playing = 0;
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                if (Instance._sfxSources[i].isPlaying) playing++;
            }
            if (playing >= MAX_CONCURRENT_SFX) return;

            var src = Instance._sfxSources[Instance._sfxIndex];
            Instance._sfxIndex = (Instance._sfxIndex + 1) % SFX_POOL_SIZE;
            src.pitch = Random.Range(pitchMin, pitchMax);
            src.PlayOneShot(clip, Mathf.Min(volume * Instance._sfxVolume, 0.8f));
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
            Instance._ambientSource.Stop();
        }

        /// <summary>
        /// Play a randomly selected music variant for the given epoch.
        /// Sprint 9: 3 variants per era group, randomly selected each level.
        /// Also starts the era-appropriate ambient audio loop.
        /// Fades in music over 200ms and staggers ambient by 200ms to avoid
        /// the harsh transient of both starting simultaneously at full volume.
        /// </summary>
        public static void PlayGameplayMusicWithVariant(int epoch)
        {
            if (Instance == null) return;
            Instance._currentMusicVariant = Random.Range(0, 3);
            var clip = PlaceholderAudio.GetGameplayMusicVariant(epoch, Instance._currentMusicVariant);
            var ambientClip = PlaceholderAudio.GetAmbientLoop(epoch);

            // Start music at zero volume, fade in
            Instance._musicSource.clip = clip;
            Instance._musicSource.volume = 0f;
            Instance._musicSource.Play();

            Instance.StopAllCoroutines();
            Instance.StartCoroutine(Instance.FadeInGameplayAudio(ambientClip));
        }

        private IEnumerator FadeInGameplayAudio(AudioClip ambientClip)
        {
            float targetMusicVol = BASE_MUSIC_VOLUME * _musicVolume;
            float targetAmbientVol = BASE_AMBIENT_VOLUME * _musicVolume;
            float fadeDuration = 0.2f; // 200ms fade-in

            // Fade music in over 200ms
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                _musicSource.volume = targetMusicVol * t;
                yield return null;
            }
            _musicSource.volume = targetMusicVol;

            // Start ambient after music fade completes (~200ms stagger)
            if (ambientClip != null)
            {
                _ambientSource.clip = ambientClip;
                _ambientSource.volume = 0f;
                _ambientSource.Play();

                elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / fadeDuration);
                    _ambientSource.volume = targetAmbientVol * t;
                    yield return null;
                }
                _ambientSource.volume = targetAmbientVol;
            }
        }

        /// <summary>
        /// Play ambient audio loop (e.g., wind, machinery, digital hum).
        /// </summary>
        public static void PlayAmbient(AudioClip clip)
        {
            if (Instance == null || clip == null) return;
            if (Instance._ambientSource.clip == clip && Instance._ambientSource.isPlaying) return;
            Instance._ambientSource.clip = clip;
            Instance._ambientSource.Play();
        }

        /// <summary>
        /// Stop only ambient audio (music continues).
        /// </summary>
        public static void StopAmbient()
        {
            if (Instance == null) return;
            Instance._ambientSource.Stop();
        }
    }
}
