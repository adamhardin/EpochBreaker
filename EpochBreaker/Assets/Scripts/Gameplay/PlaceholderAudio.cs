using UnityEngine;
using System.Collections.Generic;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Generates retro 16-bit style audio clips procedurally.
    /// All sounds are synthesized at runtime — no imported audio files needed.
    /// Mirrors the PlaceholderAssets pattern for visual content.
    /// </summary>
    public static class PlaceholderAudio
    {
        private const int SAMPLE_RATE = 44100;
        private static readonly Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();

        // ─── Waveform Types ───

        private enum Wave { Square, Triangle, Sawtooth, Noise }

        // ─── Note Frequencies (Hz) ───

        const float _F2 = 87.31f, _G2 = 98.00f, _A2 = 110.00f, _B2 = 123.47f;
        const float _C3 = 130.81f, _D3 = 146.83f, _E3 = 164.81f, _F3 = 174.61f;
        const float _G3 = 196.00f, _A3 = 220.00f, _B3 = 246.94f;
        const float _C4 = 261.63f, _D4 = 293.66f, _E4 = 329.63f, _F4 = 349.23f;
        const float _G4 = 392.00f, _A4 = 440.00f, _B4 = 493.88f;
        const float _C5 = 523.25f, _D5 = 587.33f, _E5 = 659.26f, _F5 = 698.46f;
        const float _G5 = 783.99f, _A5 = 880.00f;
        const float _C6 = 1046.50f;

        // ─── Core Synthesis ───

        private static float SampleWave(Wave type, float phase, int sampleIdx)
        {
            switch (type)
            {
                case Wave.Square:
                    return (phase % 1f) < 0.5f ? 0.8f : -0.8f;
                case Wave.Triangle:
                    return 4f * Mathf.Abs((phase % 1f) - 0.5f) - 1f;
                case Wave.Sawtooth:
                    return 2f * (phase % 1f) - 1f;
                case Wave.Noise:
                    int x = sampleIdx * 1103515245 + 12345;
                    return ((x >> 16) & 0x7FFF) / 16384f - 1f;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Additive tone generator with frequency sweep and attack/release envelope.
        /// Accumulates phase for clean sweeps. Mixes into existing buffer data.
        /// </summary>
        private static void AddTone(float[] buf, float startTime, float duration,
            float startFreq, float endFreq, Wave type, float volume,
            float attack = 0.005f, float release = 0.01f)
        {
            int startSample = Mathf.Max(0, (int)(startTime * SAMPLE_RATE));
            int sampleCount = (int)(duration * SAMPLE_RATE);
            if (sampleCount <= 0) return;

            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                int idx = startSample + i;
                if (idx >= buf.Length) break;

                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, progress);
                phase += freq / SAMPLE_RATE;

                // Attack/release envelope
                float env = 1f;
                if (attack > 0f && t < attack)
                    env = t / attack;
                if (release > 0f && t > duration - release)
                    env = Mathf.Min(env, (duration - t) / release);

                buf[idx] += SampleWave(type, phase, idx) * volume * Mathf.Clamp01(env);
            }
        }

        private static AudioClip MakeClip(string name, float[] data)
        {
            // 4-pass single-pole low-pass filter (-24 dB/octave effective rolloff)
            // Cutoff ~1.6 kHz at 44100 Hz sample rate (alpha ≈ 0.18, applied 4x)
            // Aggressively rolls off harsh upper harmonics from procedural waveforms,
            // especially on WebGL where the runtime AudioLowPassFilter is unavailable.
            const float lpAlpha = 0.18f;
            for (int pass = 0; pass < 4; pass++)
            {
                float prev = data[0];
                for (int i = 1; i < data.Length; i++)
                {
                    data[i] = prev + lpAlpha * (data[i] - prev);
                    prev = data[i];
                }
            }

            // Normalize to consistent peak level so all clips have equal loudness
            float peak = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float abs = data[i] > 0f ? data[i] : -data[i];
                if (abs > peak) peak = abs;
            }
            if (peak > 0.001f)
            {
                float gain = 0.20f / peak; // more headroom for simultaneous playback
                for (int i = 0; i < data.Length; i++)
                {
                    float s = data[i] * gain;
                    data[i] = s / (1f + Mathf.Abs(s)); // Soft saturation, asymptotes at ±1
                }
            }

            var clip = AudioClip.Create(name, data.Length, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float[] MakeBuffer(float duration)
        {
            return new float[(int)(SAMPLE_RATE * duration)];
        }

        // ═══════════════════════════════════════════════════════════════
        //  SOUND EFFECTS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>Rising chirp — classic platformer jump.</summary>
        public static AudioClip GetJumpSFX()
        {
            if (_cache.TryGetValue("sfx_jump", out var c)) return c;
            var buf = MakeBuffer(0.12f);
            AddTone(buf, 0f, 0.12f, 400f, 800f, Wave.Square, 0.25f, 0.005f, 0.02f);
            var clip = MakeClip("SFX_Jump", buf);
            _cache["sfx_jump"] = clip;
            return clip;
        }

        /// <summary>Heavy slam — ground pound impact.</summary>
        public static AudioClip GetStompSFX()
        {
            if (_cache.TryGetValue("sfx_stomp", out var c)) return c;
            var buf = MakeBuffer(0.18f);
            AddTone(buf, 0f, 0.12f, 100f, 30f, Wave.Triangle, 0.3f, 0.001f, 0.03f);
            AddTone(buf, 0f, 0.06f, 500f, 500f, Wave.Noise, 0.15f, 0.001f, 0.03f);
            AddTone(buf, 0.02f, 0.08f, 80f, 40f, Wave.Square, 0.15f, 0.001f, 0.02f);
            var clip = MakeClip("SFX_Stomp", buf);
            _cache["sfx_stomp"] = clip;
            return clip;
        }

        /// <summary>Descending pew — projectile fire.</summary>
        public static AudioClip GetShootSFX()
        {
            if (_cache.TryGetValue("sfx_shoot", out var c)) return c;
            var buf = MakeBuffer(0.08f);
            AddTone(buf, 0f, 0.08f, 1200f, 300f, Wave.Square, 0.02f, 0.002f, 0.01f);
            var clip = MakeClip("SFX_Shoot", buf);
            _cache["sfx_shoot"] = clip;
            return clip;
        }

        /// <summary>Short impact — enemy takes damage.</summary>
        public static AudioClip GetEnemyHitSFX()
        {
            if (_cache.TryGetValue("sfx_enemy_hit", out var c)) return c;
            var buf = MakeBuffer(0.08f);
            AddTone(buf, 0f, 0.04f, 300f, 300f, Wave.Square, 0.2f, 0.002f, 0.01f);
            AddTone(buf, 0f, 0.06f, 500f, 500f, Wave.Noise, 0.12f, 0.001f, 0.03f);
            var clip = MakeClip("SFX_EnemyHit", buf);
            _cache["sfx_enemy_hit"] = clip;
            return clip;
        }

        /// <summary>Descending warble — enemy destroyed.</summary>
        public static AudioClip GetEnemyDieSFX()
        {
            if (_cache.TryGetValue("sfx_enemy_die", out var c)) return c;
            var buf = MakeBuffer(0.35f);
            AddTone(buf, 0f, 0.3f, 500f, 80f, Wave.Square, 0.2f, 0.005f, 0.05f);
            AddTone(buf, 0f, 0.1f, 500f, 500f, Wave.Noise, 0.1f, 0.001f, 0.05f);
            var clip = MakeClip("SFX_EnemyDie", buf);
            _cache["sfx_enemy_die"] = clip;
            return clip;
        }

        /// <summary>Harsh buzz — player takes damage.</summary>
        public static AudioClip GetPlayerHurtSFX()
        {
            if (_cache.TryGetValue("sfx_hurt", out var c)) return c;
            var buf = MakeBuffer(0.2f);
            AddTone(buf, 0f, 0.15f, 500f, 500f, Wave.Noise, 0.15f, 0.001f, 0.05f);
            AddTone(buf, 0.02f, 0.15f, 250f, 100f, Wave.Square, 0.2f, 0.005f, 0.03f);
            var clip = MakeClip("SFX_Hurt", buf);
            _cache["sfx_hurt"] = clip;
            return clip;
        }

        /// <summary>Ascending bling — reward/item collected.</summary>
        public static AudioClip GetRewardPickupSFX()
        {
            if (_cache.TryGetValue("sfx_reward", out var c)) return c;
            var buf = MakeBuffer(0.15f);
            AddTone(buf, 0f, 0.06f, _E5, _E5, Wave.Square, 0.2f, 0.003f, 0.01f);
            AddTone(buf, 0.07f, 0.06f, _A5, _A5, Wave.Square, 0.2f, 0.003f, 0.02f);
            var clip = MakeClip("SFX_Reward", buf);
            _cache["sfx_reward"] = clip;
            return clip;
        }

        /// <summary>Power-up chord — weapon upgrade collected.</summary>
        public static AudioClip GetWeaponPickupSFX()
        {
            if (_cache.TryGetValue("sfx_weapon", out var c)) return c;
            var buf = MakeBuffer(0.3f);
            AddTone(buf, 0f, 0.08f, _C4, _C4, Wave.Square, 0.18f, 0.003f, 0.01f);
            AddTone(buf, 0.08f, 0.08f, _E4, _E4, Wave.Square, 0.18f, 0.003f, 0.01f);
            AddTone(buf, 0.16f, 0.12f, _G4, _G4, Wave.Square, 0.2f, 0.003f, 0.04f);
            AddTone(buf, 0.16f, 0.12f, _C5, _C5, Wave.Square, 0.1f, 0.003f, 0.04f);
            var clip = MakeClip("SFX_Weapon", buf);
            _cache["sfx_weapon"] = clip;
            return clip;
        }

        /// <summary>Save jingle — checkpoint activated.</summary>
        public static AudioClip GetCheckpointSFX()
        {
            if (_cache.TryGetValue("sfx_checkpoint", out var c)) return c;
            var buf = MakeBuffer(0.5f);
            AddTone(buf, 0f, 0.1f, _C5, _C5, Wave.Square, 0.15f, 0.003f, 0.02f);
            AddTone(buf, 0.1f, 0.1f, _E5, _E5, Wave.Square, 0.15f, 0.003f, 0.02f);
            AddTone(buf, 0.2f, 0.1f, _G5, _G5, Wave.Square, 0.15f, 0.003f, 0.02f);
            AddTone(buf, 0.3f, 0.18f, _C6, _C6, Wave.Square, 0.18f, 0.003f, 0.06f);
            AddTone(buf, 0.3f, 0.18f, _E5, _E5, Wave.Triangle, 0.1f, 0.003f, 0.06f);
            var clip = MakeClip("SFX_Checkpoint", buf);
            _cache["sfx_checkpoint"] = clip;
            return clip;
        }

        /// <summary>Victory fanfare — level completed.</summary>
        public static AudioClip GetLevelCompleteSFX()
        {
            if (_cache.TryGetValue("sfx_level_complete", out var c)) return c;
            var buf = MakeBuffer(1.5f);
            // Ascending arpeggio
            float n = 0.09f;
            AddTone(buf, 0f, n, _C4, _C4, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n, n, _E4, _E4, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n * 2, n, _G4, _G4, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n * 3, n, _C5, _C5, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n * 4, n, _E5, _E5, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n * 5, n, _G5, _G5, Wave.Square, 0.15f, 0.003f, 0.01f);
            // Sustained C major chord
            float chordStart = n * 6;
            float chordDur = 1.5f - chordStart - 0.1f;
            AddTone(buf, chordStart, chordDur, _C5, _C5, Wave.Square, 0.15f, 0.01f, 0.15f);
            AddTone(buf, chordStart, chordDur, _E5, _E5, Wave.Square, 0.1f, 0.01f, 0.15f);
            AddTone(buf, chordStart, chordDur, _G5, _G5, Wave.Square, 0.08f, 0.01f, 0.15f);
            AddTone(buf, chordStart, chordDur, _C4, _C4, Wave.Triangle, 0.12f, 0.01f, 0.15f);
            var clip = MakeClip("SFX_LevelComplete", buf);
            _cache["sfx_level_complete"] = clip;
            return clip;
        }

        /// <summary>Get weapon-type-specific fire SFX.</summary>
        public static AudioClip GetWeaponFireSFX(WeaponType type)
        {
            string key = $"sfx_fire_{type}";
            if (_cache.TryGetValue(key, out var c)) return c;

            AudioClip clip;
            switch (type)
            {
                case WeaponType.Piercer:
                    clip = MakePiercerSFX();
                    break;
                case WeaponType.Spreader:
                    clip = MakeSpreaderSFX();
                    break;
                case WeaponType.Chainer:
                    clip = MakeChainerSFX();
                    break;
                case WeaponType.Slower:
                    clip = MakeSlowerSFX();
                    break;
                case WeaponType.Cannon:
                    clip = MakeCannonSFX();
                    break;
                default:
                    return GetShootSFX(); // Bolt uses default
            }
            _cache[key] = clip;
            return clip;
        }

        private static AudioClip MakePiercerSFX()
        {
            // Sharp whistle — piercing bolt (capped at 1500 Hz to avoid harmonic squeals)
            var buf = MakeBuffer(0.10f);
            AddTone(buf, 0f, 0.10f, 1500f, 500f, Wave.Sawtooth, 0.02f, 0.001f, 0.02f);
            AddTone(buf, 0f, 0.05f, 1400f, 800f, Wave.Square, 0.01f, 0.001f, 0.01f);
            return MakeClip("SFX_Fire_Piercer", buf);
        }

        private static AudioClip MakeSpreaderSFX()
        {
            // Short burst — shotgun-like spread
            var buf = MakeBuffer(0.10f);
            AddTone(buf, 0f, 0.06f, 800f, 200f, Wave.Noise, 0.12f, 0.001f, 0.02f);
            AddTone(buf, 0f, 0.04f, 600f, 300f, Wave.Square, 0.08f, 0.001f, 0.01f);
            return MakeClip("SFX_Fire_Spreader", buf);
        }

        private static AudioClip MakeChainerSFX()
        {
            // Electric zap — chain lightning (capped frequencies to avoid squeals)
            var buf = MakeBuffer(0.12f);
            AddTone(buf, 0f, 0.08f, 1200f, 600f, Wave.Square, 0.02f, 0.001f, 0.015f);
            AddTone(buf, 0.02f, 0.06f, 1400f, 800f, Wave.Noise, 0.06f, 0.001f, 0.02f);
            AddTone(buf, 0.06f, 0.06f, 600f, 400f, Wave.Triangle, 0.04f, 0.001f, 0.02f);
            return MakeClip("SFX_Fire_Chainer", buf);
        }

        private static AudioClip MakeSlowerSFX()
        {
            // Low warble — slow/freeze effect
            var buf = MakeBuffer(0.15f);
            AddTone(buf, 0f, 0.15f, 400f, 200f, Wave.Triangle, 0.15f, 0.01f, 0.04f);
            AddTone(buf, 0.02f, 0.10f, 300f, 150f, Wave.Square, 0.05f, 0.005f, 0.03f);
            return MakeClip("SFX_Fire_Slower", buf);
        }

        private static AudioClip MakeCannonSFX()
        {
            // Heavy boom — cannon blast
            var buf = MakeBuffer(0.25f);
            AddTone(buf, 0f, 0.15f, 120f, 40f, Wave.Triangle, 0.30f, 0.001f, 0.05f);
            AddTone(buf, 0f, 0.08f, 500f, 200f, Wave.Noise, 0.20f, 0.001f, 0.03f);
            AddTone(buf, 0.02f, 0.10f, 100f, 50f, Wave.Square, 0.15f, 0.005f, 0.04f);
            return MakeClip("SFX_Fire_Cannon", buf);
        }

        /// <summary>Overheat warning — cannon overheated.</summary>
        public static AudioClip GetOverheatSFX()
        {
            if (_cache.TryGetValue("sfx_overheat", out var c)) return c;
            var buf = MakeBuffer(0.4f);
            // Hissing steam
            AddTone(buf, 0f, 0.3f, 800f, 400f, Wave.Noise, 0.15f, 0.01f, 0.1f);
            // Warning descending tone
            AddTone(buf, 0.05f, 0.25f, 600f, 200f, Wave.Square, 0.1f, 0.01f, 0.05f);
            var clip = MakeClip("SFX_Overheat", buf);
            _cache["sfx_overheat"] = clip;
            return clip;
        }

        /// <summary>Chain arc zap — chain lightning jump.</summary>
        public static AudioClip GetChainArcSFX()
        {
            if (_cache.TryGetValue("sfx_chain_arc", out var c)) return c;
            var buf = MakeBuffer(0.08f);
            AddTone(buf, 0f, 0.06f, 1400f, 600f, Wave.Noise, 0.08f, 0.001f, 0.02f);
            AddTone(buf, 0.01f, 0.05f, 1000f, 500f, Wave.Square, 0.04f, 0.001f, 0.015f);
            var clip = MakeClip("SFX_ChainArc", buf);
            _cache["sfx_chain_arc"] = clip;
            return clip;
        }

        /// <summary>Debris crash — falling blocks impact.</summary>
        public static AudioClip GetDebrisSFX()
        {
            if (_cache.TryGetValue("sfx_debris", out var c)) return c;
            var buf = MakeBuffer(0.3f);
            AddTone(buf, 0f, 0.15f, 150f, 40f, Wave.Triangle, 0.25f, 0.001f, 0.05f);
            AddTone(buf, 0f, 0.1f, 400f, 200f, Wave.Noise, 0.20f, 0.001f, 0.04f);
            AddTone(buf, 0.05f, 0.2f, 100f, 30f, Wave.Square, 0.10f, 0.01f, 0.05f);
            var clip = MakeClip("SFX_Debris", buf);
            _cache["sfx_debris"] = clip;
            return clip;
        }

        /// <summary>Gas hiss — poison cloud released.</summary>
        public static AudioClip GetGasHissSFX()
        {
            if (_cache.TryGetValue("sfx_gas", out var c)) return c;
            var buf = MakeBuffer(0.4f);
            AddTone(buf, 0f, 0.4f, 600f, 300f, Wave.Noise, 0.12f, 0.02f, 0.1f);
            AddTone(buf, 0.05f, 0.3f, 200f, 100f, Wave.Triangle, 0.05f, 0.01f, 0.05f);
            var clip = MakeClip("SFX_GasHiss", buf);
            _cache["sfx_gas"] = clip;
            return clip;
        }

        /// <summary>Fire whoosh — fire hazard released.</summary>
        public static AudioClip GetFireSFX()
        {
            if (_cache.TryGetValue("sfx_fire", out var c)) return c;
            var buf = MakeBuffer(0.3f);
            AddTone(buf, 0f, 0.2f, 800f, 200f, Wave.Noise, 0.15f, 0.005f, 0.06f);
            AddTone(buf, 0f, 0.15f, 300f, 100f, Wave.Sawtooth, 0.10f, 0.005f, 0.04f);
            var clip = MakeClip("SFX_Fire", buf);
            _cache["sfx_fire"] = clip;
            return clip;
        }

        /// <summary>Spike scrape — metal spikes emerge.</summary>
        public static AudioClip GetSpikeSFX()
        {
            if (_cache.TryGetValue("sfx_spike", out var c)) return c;
            var buf = MakeBuffer(0.2f);
            AddTone(buf, 0f, 0.1f, 1200f, 600f, Wave.Sawtooth, 0.08f, 0.001f, 0.02f);
            AddTone(buf, 0.03f, 0.15f, 400f, 200f, Wave.Noise, 0.10f, 0.005f, 0.04f);
            var clip = MakeClip("SFX_Spike", buf);
            _cache["sfx_spike"] = clip;
            return clip;
        }

        /// <summary>Short crunch — tile destroyed.</summary>
        public static AudioClip GetTileCrunchSFX()
        {
            if (_cache.TryGetValue("sfx_crunch", out var c)) return c;
            var buf = MakeBuffer(0.12f);
            // Crunchy noise burst with low-frequency thud
            AddTone(buf, 0f, 0.06f, 600f, 300f, Wave.Noise, 0.15f, 0.001f, 0.02f);
            AddTone(buf, 0f, 0.08f, 120f, 50f, Wave.Triangle, 0.2f, 0.001f, 0.03f);
            AddTone(buf, 0.02f, 0.06f, 400f, 150f, Wave.Square, 0.08f, 0.001f, 0.02f);
            var clip = MakeClip("SFX_Crunch", buf);
            _cache["sfx_crunch"] = clip;
            return clip;
        }

        /// <summary>Soft scraping loop — wall-slide contact.</summary>
        public static AudioClip GetWallSlideSFX()
        {
            if (_cache.TryGetValue("sfx_wall_slide", out var c)) return c;
            var buf = MakeBuffer(0.3f);
            // Gritty scraping: filtered noise with low-frequency modulation
            AddTone(buf, 0f, 0.3f, 200f, 250f, Wave.Noise, 0.08f, 0.01f, 0.01f);
            AddTone(buf, 0f, 0.3f, 80f, 100f, Wave.Triangle, 0.06f, 0.01f, 0.01f);
            // Crossfade for seamless loop
            ApplyLoopFade(buf);
            var clip = MakeClip("SFX_WallSlide", buf);
            _cache["sfx_wall_slide"] = clip;
            return clip;
        }

        /// <summary>Quick blip — menu button pressed.</summary>
        public static AudioClip GetMenuSelectSFX()
        {
            if (_cache.TryGetValue("sfx_select", out var c)) return c;
            var buf = MakeBuffer(0.05f);
            AddTone(buf, 0f, 0.05f, 880f, 880f, Wave.Square, 0.15f, 0.002f, 0.01f);
            var clip = MakeClip("SFX_Select", buf);
            _cache["sfx_select"] = clip;
            return clip;
        }

        /// <summary>Dramatic death sound — player died.</summary>
        public static AudioClip GetPlayerDeathSFX()
        {
            if (_cache.TryGetValue("sfx_death", out var c)) return c;
            var buf = MakeBuffer(0.8f);
            // Dramatic descending sweep
            AddTone(buf, 0f, 0.4f, 800f, 100f, Wave.Square, 0.25f, 0.01f, 0.1f);
            // Heavy impact
            AddTone(buf, 0f, 0.15f, 150f, 40f, Wave.Triangle, 0.3f, 0.001f, 0.05f);
            // Noise burst for impact
            AddTone(buf, 0f, 0.2f, 500f, 200f, Wave.Noise, 0.2f, 0.001f, 0.08f);
            // Sad trailing tone
            AddTone(buf, 0.3f, 0.5f, 200f, 80f, Wave.Triangle, 0.15f, 0.05f, 0.2f);
            var clip = MakeClip("SFX_Death", buf);
            _cache["sfx_death"] = clip;
            return clip;
        }

        /// <summary>Epic boss death explosion — massive multi-layered impact.</summary>
        public static AudioClip GetBossDeathSFX()
        {
            if (_cache.TryGetValue("sfx_boss_death", out var c)) return c;
            var buf = MakeBuffer(1.5f);
            // Initial massive impact
            AddTone(buf, 0f, 0.3f, 100f, 30f, Wave.Triangle, 0.4f, 0.001f, 0.1f);
            AddTone(buf, 0f, 0.4f, 400f, 50f, Wave.Noise, 0.35f, 0.001f, 0.15f);
            // Explosion rumble
            AddTone(buf, 0.1f, 0.5f, 80f, 20f, Wave.Square, 0.3f, 0.02f, 0.2f);
            // Rising pitch - energy release
            AddTone(buf, 0.2f, 0.4f, 200f, 800f, Wave.Sawtooth, 0.2f, 0.05f, 0.1f);
            // Secondary explosions
            AddTone(buf, 0.4f, 0.3f, 150f, 40f, Wave.Noise, 0.25f, 0.01f, 0.1f);
            AddTone(buf, 0.6f, 0.25f, 120f, 35f, Wave.Noise, 0.2f, 0.01f, 0.1f);
            // Triumphant chime
            AddTone(buf, 0.8f, 0.4f, 523f, 523f, Wave.Square, 0.15f, 0.02f, 0.2f); // C5
            AddTone(buf, 0.9f, 0.4f, 659f, 659f, Wave.Square, 0.15f, 0.02f, 0.2f); // E5
            AddTone(buf, 1.0f, 0.5f, 784f, 784f, Wave.Square, 0.15f, 0.02f, 0.3f); // G5
            var clip = MakeClip("SFX_BossDeath", buf);
            _cache["sfx_boss_death"] = clip;
            return clip;
        }

        /// <summary>Deep growl — boss roar on activation and phase change.</summary>
        public static AudioClip GetBossRoarSFX()
        {
            if (_cache.TryGetValue("sfx_boss_roar", out var c)) return c;
            var buf = MakeBuffer(0.6f);
            // Low rumbling growl
            AddTone(buf, 0f, 0.5f, 80f, 50f, Wave.Sawtooth, 0.3f, 0.02f, 0.15f);
            AddTone(buf, 0.05f, 0.4f, 120f, 60f, Wave.Square, 0.2f, 0.02f, 0.1f);
            // Noise texture for grit
            AddTone(buf, 0f, 0.3f, 300f, 100f, Wave.Noise, 0.15f, 0.01f, 0.1f);
            var clip = MakeClip("SFX_BossRoar", buf);
            _cache["sfx_boss_roar"] = clip;
            return clip;
        }

        /// <summary>Rising growl — charge wind-up telegraph.</summary>
        public static AudioClip GetChargeWindupSFX()
        {
            if (_cache.TryGetValue("sfx_charge_windup", out var c)) return c;
            var buf = MakeBuffer(0.5f);
            // Rising threat
            AddTone(buf, 0f, 0.45f, 60f, 200f, Wave.Sawtooth, 0.25f, 0.03f, 0.05f);
            AddTone(buf, 0.1f, 0.35f, 100f, 300f, Wave.Noise, 0.12f, 0.02f, 0.05f);
            AddTone(buf, 0.2f, 0.25f, 80f, 250f, Wave.Square, 0.1f, 0.02f, 0.05f);
            var clip = MakeClip("SFX_ChargeWindup", buf);
            _cache["sfx_charge_windup"] = clip;
            return clip;
        }

        // ═══════════════════════════════════════════════════════════════
        //  MUSIC
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Majestic chiptune loop — C→Am→F→G progression, 8 bars at 110 BPM.
        /// Three channels: triangle bass, square lead arpeggios, sawtooth pad.
        /// </summary>
        public static AudioClip GetTitleMusic()
        {
            if (_cache.TryGetValue("music_title", out var c)) return c;

            float bpm = 110f;
            float beat = 60f / bpm;
            int bars = 8;
            float totalDur = bars * 4 * beat;
            var buf = MakeBuffer(totalDur);
            float eighth = beat / 2f;

            // Chord progression: C | Am | F | G (repeated twice)
            float[] bassRoots = { _C3, _A2, _F2, _G2, _C3, _A2, _F2, _G2 };

            // ── Bass channel (triangle, half notes) ──
            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                float root = bassRoots[bar];
                AddTone(buf, barStart, 2f * beat * 0.95f, root, root,
                    Wave.Triangle, 0.18f, 0.01f, 0.05f);
                AddTone(buf, barStart + 2 * beat, 2f * beat * 0.95f, root, root,
                    Wave.Triangle, 0.18f, 0.01f, 0.05f);
            }

            // ── Lead channel (square, arpeggiated eighth notes) ──
            float[][] leadPatterns = {
                new[] { _E4, _G4, _C5, _E5, _C5, _G4, _E4, _G4 },  // C
                new[] { _C4, _E4, _A4, _C5, _A4, _E4, _C4, _E4 },  // Am
                new[] { _F4, _A4, _C5, _F5, _C5, _A4, _F4, _A4 },  // F
                new[] { _G4, _B4, _D5, _G5, _D5, _B4, _G4, _B4 },  // G
            };

            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                float[] pattern = leadPatterns[bar % 4];
                for (int n = 0; n < pattern.Length; n++)
                {
                    AddTone(buf, barStart + n * eighth, eighth * 0.8f,
                        pattern[n], pattern[n], Wave.Square, 0.1f, 0.005f, 0.02f);
                }
            }

            // ── Pad channel (sawtooth, sustained chords, quiet) ──
            float[][] chordNotes = {
                new[] { _C4, _E4, _G4 },  // C major
                new[] { _A3, _C4, _E4 },  // A minor
                new[] { _F3, _A3, _C4 },  // F major
                new[] { _G3, _B3, _D4 },  // G major
            };

            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                float[] chord = chordNotes[bar % 4];
                foreach (float note in chord)
                {
                    AddTone(buf, barStart, 4 * beat * 0.95f,
                        note, note, Wave.Triangle, 0.04f, 0.05f, 0.1f);
                }
            }

            // Crossfade edges for seamless loop
            ApplyLoopFade(buf);

            var clip = MakeClip("Music_Title", buf);
            _cache["music_title"] = clip;
            return clip;
        }

        /// <summary>
        /// Era-varied chiptune loop — Am→F→C→G progression, 8 bars.
        /// Era 0-2: 110 BPM, triangle lead (ancient, gentle).
        /// Era 3-5: 128 BPM, square lead (driving, medieval-industrial).
        /// Era 6-8: 140 BPM, sawtooth lead (aggressive, modern-digital).
        /// Era 9: 120 BPM, ethereal triangle+square with delay effect.
        /// </summary>
        public static AudioClip GetGameplayMusic(int epoch = -1)
        {
            string key = epoch >= 0 ? $"music_gameplay_{epoch}" : "music_gameplay";
            if (_cache.TryGetValue(key, out var c)) return c;

            // Era-based tempo and instrument
            float bpm;
            Wave leadWave;
            float leadVol;
            if (epoch >= 0 && epoch <= 2)
            {
                bpm = 110f; leadWave = Wave.Triangle; leadVol = 0.12f;
            }
            else if (epoch >= 6 && epoch <= 8)
            {
                bpm = 140f; leadWave = Wave.Triangle; leadVol = 0.08f;
            }
            else if (epoch == 9)
            {
                bpm = 120f; leadWave = Wave.Triangle; leadVol = 0.1f;
            }
            else
            {
                bpm = 128f; leadWave = Wave.Square; leadVol = 0.1f;
            }
            float beat = 60f / bpm;
            int bars = 8;
            float totalDur = bars * 4 * beat;
            var buf = MakeBuffer(totalDur);
            float eighth = beat / 2f;

            // Progression: Am | F | C | G (repeated twice, minor feel)
            float[] bassRoots = { _A2, _F2, _C3, _G2, _A2, _F2, _C3, _G2 };

            // ── Bass channel (triangle, driving eighth-note pattern) ──
            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                float root = bassRoots[bar];
                float octUp = root * 2f;
                // Pattern: root, rest, octUp, root, rest, octUp, root, octUp
                float[] bassPattern = { root, 0, octUp, root, 0, octUp, root, octUp };
                for (int n = 0; n < bassPattern.Length; n++)
                {
                    if (bassPattern[n] <= 0f) continue;
                    AddTone(buf, barStart + n * eighth, eighth * 0.7f,
                        bassPattern[n], bassPattern[n], Wave.Triangle, 0.2f, 0.005f, 0.02f);
                }
            }

            // ── Lead channel (era-varied waveform, syncopated melody) ──
            // First 4 bars: ascending pattern
            float[][] leadA = {
                new[] { _A4, 0f, _C5, _A4, _E5, 0f, _C5, _E5 },  // Am
                new[] { _F4, 0f, _A4, _F4, _C5, 0f, _A4, _C5 },  // F
                new[] { _E4, 0f, _G4, _E4, _C5, 0f, _G4, _C5 },  // C
                new[] { _D4, 0f, _G4, _D4, _B4, 0f, _G4, _B4 },  // G
            };
            // Second 4 bars: variation (descending then ascending)
            float[][] leadB = {
                new[] { _E5, 0f, _A4, _C5, _E5, 0f, _A5, _E5 },  // Am
                new[] { _C5, 0f, _F4, _A4, _C5, 0f, _F5, _C5 },  // F
                new[] { _C5, 0f, _E4, _G4, _C5, 0f, _E5, _C5 },  // C
                new[] { _B4, 0f, _D4, _G4, _B4, 0f, _D5, _B4 },  // G
            };

            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                float[] pattern = bar < 4 ? leadA[bar % 4] : leadB[bar % 4];
                for (int n = 0; n < pattern.Length; n++)
                {
                    if (pattern[n] <= 0f) continue;
                    AddTone(buf, barStart + n * eighth, eighth * 0.75f,
                        pattern[n], pattern[n], leadWave, leadVol, 0.005f, 0.015f);
                }
            }

            // ── Percussion ──
            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                for (int b = 0; b < 4; b++)
                {
                    float beatStart = barStart + b * beat;
                    // Kick on every beat (triangle sweep down)
                    AddTone(buf, beatStart, 0.06f, 150f, 40f, Wave.Triangle, 0.15f, 0.001f, 0.02f);
                    // Snare on beats 2 and 4 (noise burst)
                    if (b == 1 || b == 3)
                        AddTone(buf, beatStart, 0.06f, 500f, 500f, Wave.Noise, 0.1f, 0.001f, 0.03f);
                    // Hi-hat on every eighth note (very short noise)
                    AddTone(buf, beatStart, 0.02f, 1000f, 1000f, Wave.Noise, 0.04f, 0.001f, 0.008f);
                    AddTone(buf, beatStart + eighth, 0.02f, 1000f, 1000f, Wave.Noise, 0.03f, 0.001f, 0.008f);
                }
            }

            // Era 9: add echo/delay effect for ethereal feel
            if (epoch == 9)
            {
                int delaySamples = (int)(0.15f * SAMPLE_RATE);
                for (int i = delaySamples; i < buf.Length; i++)
                    buf[i] += buf[i - delaySamples] * 0.3f;
            }

            ApplyLoopFade(buf);

            var clip = MakeClip($"Music_Gameplay_{epoch}", buf);
            _cache[key] = clip;
            return clip;
        }

        // ═══════════════════════════════════════════════════════════════
        //  MUSIC VARIANTS (Sprint 9: 3 per era group, random selection)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Get a gameplay music variant for the current era group.
        /// Era groups: 0-2 (ancient), 3-5 (medieval-industrial), 6-8 (modern-digital), 9 (transcendent).
        /// Each group has 3 variants with distinct progressions, rhythms, and lead patterns.
        /// variant should be 0, 1, or 2.
        /// </summary>
        public static AudioClip GetGameplayMusicVariant(int epoch, int variant)
        {
            // Variant 0 returns the original track
            if (variant == 0) return GetGameplayMusic(epoch);

            string key = $"music_gameplay_{epoch}_v{variant}";
            if (_cache.TryGetValue(key, out var c)) return c;

            float bpm;
            Wave leadWave;
            float leadVol;

            if (epoch >= 0 && epoch <= 2)
            {
                bpm = variant == 1 ? 100f : 118f;
                leadWave = variant == 1 ? Wave.Square : Wave.Triangle;
                leadVol = 0.11f;
            }
            else if (epoch >= 6 && epoch <= 8)
            {
                bpm = variant == 1 ? 135f : 148f;
                leadWave = variant == 1 ? Wave.Square : Wave.Triangle;
                leadVol = variant == 1 ? 0.09f : 0.07f;
            }
            else if (epoch == 9)
            {
                bpm = variant == 1 ? 115f : 126f;
                leadWave = variant == 1 ? Wave.Square : Wave.Triangle;
                leadVol = 0.09f;
            }
            else // 3-5
            {
                bpm = variant == 1 ? 122f : 136f;
                leadWave = variant == 1 ? Wave.Triangle : Wave.Square;
                leadVol = 0.1f;
            }

            float beat = 60f / bpm;
            int bars = 8;
            float totalDur = bars * 4 * beat;
            var buf = MakeBuffer(totalDur);
            float eighth = beat / 2f;

            // Variant 1: Dm | Bb | F | C (darker progression)
            // Variant 2: Em | C | G | D (brighter progression)
            float[] bassRoots;
            float[][] leadA, leadB;
            if (variant == 1)
            {
                bassRoots = new[] { _D3, _B2, _F2, _C3, _D3, _B2, _F2, _C3 };
                leadA = new[] {
                    new[] { _D4, 0f, _F4, _D4, _A4, 0f, _F4, _A4 },
                    new[] { _B3, 0f, _D4, _B3, _F4, 0f, _D4, _F4 },
                    new[] { _F4, 0f, _A4, _F4, _C5, 0f, _A4, _C5 },
                    new[] { _E4, 0f, _G4, _E4, _C5, 0f, _G4, _C5 },
                };
                leadB = new[] {
                    new[] { _A4, 0f, _D4, _F4, _A4, 0f, _D5, _A4 },
                    new[] { _F4, 0f, _B3, _D4, _F4, 0f, _B4, _F4 },
                    new[] { _C5, 0f, _F4, _A4, _C5, 0f, _F5, _C5 },
                    new[] { _C5, 0f, _E4, _G4, _C5, 0f, _E5, _C5 },
                };
            }
            else
            {
                bassRoots = new[] { _E3, _C3, _G2, _D3, _E3, _C3, _G2, _D3 };
                leadA = new[] {
                    new[] { _E4, 0f, _G4, _E4, _B4, 0f, _G4, _B4 },
                    new[] { _C4, 0f, _E4, _C4, _G4, 0f, _E4, _G4 },
                    new[] { _G4, 0f, _B4, _G4, _D5, 0f, _B4, _D5 },
                    new[] { _D4, 0f, _A4, _D4, _A4, 0f, _F4, _A4 },
                };
                leadB = new[] {
                    new[] { _B4, 0f, _E4, _G4, _B4, 0f, _E5, _B4 },
                    new[] { _G4, 0f, _C4, _E4, _G4, 0f, _C5, _G4 },
                    new[] { _D5, 0f, _G4, _B4, _D5, 0f, _G5, _D5 },
                    new[] { _A4, 0f, _D4, _F4, _A4, 0f, _D5, _A4 },
                };
            }

            // Bass channel
            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                float root = bassRoots[bar];
                float octUp = root * 2f;
                float[] bassPattern = { root, 0, octUp, root, 0, octUp, root, octUp };
                for (int n = 0; n < bassPattern.Length; n++)
                {
                    if (bassPattern[n] <= 0f) continue;
                    AddTone(buf, barStart + n * eighth, eighth * 0.7f,
                        bassPattern[n], bassPattern[n], Wave.Triangle, 0.2f, 0.005f, 0.02f);
                }
            }

            // Lead channel
            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                float[] pattern = bar < 4 ? leadA[bar % 4] : leadB[bar % 4];
                for (int n = 0; n < pattern.Length; n++)
                {
                    if (pattern[n] <= 0f) continue;
                    AddTone(buf, barStart + n * eighth, eighth * 0.75f,
                        pattern[n], pattern[n], leadWave, leadVol, 0.005f, 0.015f);
                }
            }

            // Percussion
            for (int bar = 0; bar < bars; bar++)
            {
                float barStart = bar * 4 * beat;
                for (int b = 0; b < 4; b++)
                {
                    float beatStart = barStart + b * beat;
                    AddTone(buf, beatStart, 0.06f, 150f, 40f, Wave.Triangle, 0.15f, 0.001f, 0.02f);
                    if (variant == 1 ? (b == 1 || b == 3) : (b == 0 || b == 2))
                        AddTone(buf, beatStart, 0.06f, 500f, 500f, Wave.Noise, 0.1f, 0.001f, 0.03f);
                    AddTone(buf, beatStart, 0.02f, 1000f, 1000f, Wave.Noise, 0.04f, 0.001f, 0.008f);
                    AddTone(buf, beatStart + eighth, 0.02f, 1000f, 1000f, Wave.Noise, 0.03f, 0.001f, 0.008f);
                }
            }

            // Era 9: echo effect
            if (epoch == 9)
            {
                int delaySamples = (int)(0.15f * SAMPLE_RATE);
                for (int i = delaySamples; i < buf.Length; i++)
                    buf[i] += buf[i - delaySamples] * 0.3f;
            }

            ApplyLoopFade(buf);
            var clip = MakeClip($"Music_Gameplay_{epoch}_v{variant}", buf);
            _cache[key] = clip;
            return clip;
        }

        // ═══════════════════════════════════════════════════════════════
        //  VICTORY JINGLES (Sprint 9: varies by star rating)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Victory jingle that varies by star rating.
        /// 1-star: short ascending notes. 2-star: medium fanfare. 3-star: triumphant full fanfare.
        /// </summary>
        public static AudioClip GetVictoryJingle(int stars)
        {
            stars = Mathf.Clamp(stars, 1, 3);
            string key = $"sfx_victory_{stars}star";
            if (_cache.TryGetValue(key, out var c)) return c;

            AudioClip clip;
            switch (stars)
            {
                case 1:
                    clip = MakeVictory1Star();
                    break;
                case 2:
                    clip = MakeVictory2Star();
                    break;
                default:
                    clip = MakeVictory3Star();
                    break;
            }
            _cache[key] = clip;
            return clip;
        }

        private static AudioClip MakeVictory1Star()
        {
            // Short, modest ascending notes — level cleared but not great
            var buf = MakeBuffer(0.8f);
            float n = 0.12f;
            AddTone(buf, 0f, n, _C4, _C4, Wave.Square, 0.15f, 0.003f, 0.02f);
            AddTone(buf, n, n, _E4, _E4, Wave.Square, 0.15f, 0.003f, 0.02f);
            AddTone(buf, n * 2, 0.4f, _G4, _G4, Wave.Square, 0.18f, 0.005f, 0.15f);
            AddTone(buf, n * 2, 0.4f, _C4, _C4, Wave.Triangle, 0.1f, 0.01f, 0.15f);
            return MakeClip("SFX_Victory_1Star", buf);
        }

        private static AudioClip MakeVictory2Star()
        {
            // Medium fanfare — solid performance
            var buf = MakeBuffer(1.2f);
            float n = 0.1f;
            AddTone(buf, 0f, n, _C4, _C4, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n, n, _E4, _E4, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n * 2, n, _G4, _G4, Wave.Square, 0.15f, 0.003f, 0.01f);
            AddTone(buf, n * 3, n, _C5, _C5, Wave.Square, 0.18f, 0.003f, 0.01f);
            // Sustained chord
            float chordStart = n * 4;
            AddTone(buf, chordStart, 0.6f, _C5, _C5, Wave.Square, 0.14f, 0.01f, 0.15f);
            AddTone(buf, chordStart, 0.6f, _E4, _E4, Wave.Triangle, 0.10f, 0.01f, 0.15f);
            AddTone(buf, chordStart, 0.6f, _G4, _G4, Wave.Triangle, 0.08f, 0.01f, 0.15f);
            return MakeClip("SFX_Victory_2Star", buf);
        }

        private static AudioClip MakeVictory3Star()
        {
            // Triumphant full fanfare — perfect run!
            var buf = MakeBuffer(1.8f);
            float n = 0.08f;
            // Rapid ascending arpeggio
            AddTone(buf, 0f, n, _C4, _C4, Wave.Square, 0.14f, 0.003f, 0.01f);
            AddTone(buf, n, n, _E4, _E4, Wave.Square, 0.14f, 0.003f, 0.01f);
            AddTone(buf, n * 2, n, _G4, _G4, Wave.Square, 0.14f, 0.003f, 0.01f);
            AddTone(buf, n * 3, n, _C5, _C5, Wave.Square, 0.14f, 0.003f, 0.01f);
            AddTone(buf, n * 4, n, _E5, _E5, Wave.Square, 0.14f, 0.003f, 0.01f);
            AddTone(buf, n * 5, n, _G5, _G5, Wave.Square, 0.14f, 0.003f, 0.01f);
            // Grand sustained chord with bass
            float chordStart = n * 6;
            float chordDur = 1.8f - chordStart - 0.15f;
            AddTone(buf, chordStart, chordDur, _C5, _C5, Wave.Square, 0.15f, 0.01f, 0.2f);
            AddTone(buf, chordStart, chordDur, _E5, _E5, Wave.Square, 0.12f, 0.01f, 0.2f);
            AddTone(buf, chordStart, chordDur, _G5, _G5, Wave.Square, 0.10f, 0.01f, 0.2f);
            AddTone(buf, chordStart, chordDur, _C4, _C4, Wave.Triangle, 0.15f, 0.01f, 0.2f);
            AddTone(buf, chordStart, chordDur, _G3, _G3, Wave.Triangle, 0.10f, 0.01f, 0.2f);
            // Shimmer effect
            AddTone(buf, chordStart + 0.1f, chordDur - 0.1f, _C6, _C6, Wave.Square, 0.04f, 0.1f, 0.3f);
            return MakeClip("SFX_Victory_3Star", buf);
        }

        // ═══════════════════════════════════════════════════════════════
        //  AMBIENT AUDIO (Sprint 9: environmental loops per era)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Get era-appropriate ambient audio loop.
        /// Era 0-2: wind (low white noise). Era 3-4: medieval ambiance. Era 5: machinery (rhythmic clank).
        /// Era 6: urban hum. Era 7-8: digital hum. Era 9: cosmic resonance.
        /// </summary>
        public static AudioClip GetAmbientLoop(int epoch)
        {
            string key = $"ambient_{epoch}";
            if (_cache.TryGetValue(key, out var c)) return c;

            AudioClip clip;
            if (epoch <= 2)
                clip = MakeWindAmbient(epoch);
            else if (epoch <= 4)
                clip = MakeMedievalAmbient(epoch);
            else if (epoch == 5)
                clip = MakeMachineryAmbient();
            else if (epoch == 6)
                clip = MakeUrbanAmbient();
            else if (epoch <= 8)
                clip = MakeDigitalAmbient(epoch);
            else
                clip = MakeCosmicAmbient();

            _cache[key] = clip;
            return clip;
        }

        private static AudioClip MakeWindAmbient(int epoch)
        {
            // Gentle wind: low-frequency filtered noise
            var buf = MakeBuffer(3.0f);
            float baseFreq = 60f + epoch * 15f; // Slightly different per era
            AddTone(buf, 0f, 3.0f, baseFreq, baseFreq + 30f, Wave.Noise, 0.06f, 0.2f, 0.2f);
            AddTone(buf, 0.5f, 2.0f, baseFreq * 0.5f, baseFreq * 0.7f, Wave.Noise, 0.04f, 0.3f, 0.3f);
            // Soft rustling
            AddTone(buf, 1.0f, 1.5f, 200f, 150f, Wave.Noise, 0.02f, 0.2f, 0.3f);
            ApplyLoopFade(buf);
            return MakeClip($"Ambient_Wind_{epoch}", buf);
        }

        private static AudioClip MakeMedievalAmbient(int epoch)
        {
            // Medieval: distant bells, crackling fire
            var buf = MakeBuffer(4.0f);
            // Background crackle
            AddTone(buf, 0f, 4.0f, 150f, 100f, Wave.Noise, 0.03f, 0.3f, 0.3f);
            // Distant bell tones
            AddTone(buf, 0.5f, 0.8f, 400f, 395f, Wave.Triangle, 0.04f, 0.01f, 0.5f);
            AddTone(buf, 2.2f, 0.8f, 350f, 345f, Wave.Triangle, 0.03f, 0.01f, 0.5f);
            // Wind undertone
            AddTone(buf, 0f, 4.0f, 80f, 90f, Wave.Noise, 0.02f, 0.3f, 0.3f);
            ApplyLoopFade(buf);
            return MakeClip($"Ambient_Medieval_{epoch}", buf);
        }

        private static AudioClip MakeMachineryAmbient()
        {
            // Industrial: rhythmic clanking, steam, gears
            var buf = MakeBuffer(3.0f);
            // Steady mechanical hum
            AddTone(buf, 0f, 3.0f, 60f, 62f, Wave.Triangle, 0.04f, 0.2f, 0.2f);
            // Rhythmic clank (every 0.5s)
            for (int i = 0; i < 6; i++)
            {
                float t = i * 0.5f;
                AddTone(buf, t, 0.05f, 800f, 200f, Wave.Noise, 0.06f, 0.001f, 0.02f);
                AddTone(buf, t, 0.08f, 100f, 50f, Wave.Triangle, 0.05f, 0.001f, 0.03f);
            }
            // Steam hiss
            AddTone(buf, 1.0f, 0.6f, 500f, 300f, Wave.Noise, 0.03f, 0.05f, 0.2f);
            ApplyLoopFade(buf);
            return MakeClip("Ambient_Machinery", buf);
        }

        private static AudioClip MakeUrbanAmbient()
        {
            // Urban: city hum, distant traffic
            var buf = MakeBuffer(3.0f);
            AddTone(buf, 0f, 3.0f, 80f, 85f, Wave.Triangle, 0.03f, 0.3f, 0.3f);
            AddTone(buf, 0f, 3.0f, 120f, 125f, Wave.Triangle, 0.03f, 0.3f, 0.3f);
            // Distant traffic swooshes
            AddTone(buf, 0.3f, 0.8f, 200f, 100f, Wave.Noise, 0.03f, 0.1f, 0.3f);
            AddTone(buf, 1.8f, 0.7f, 180f, 90f, Wave.Noise, 0.025f, 0.1f, 0.3f);
            ApplyLoopFade(buf);
            return MakeClip("Ambient_Urban", buf);
        }

        private static AudioClip MakeDigitalAmbient(int epoch)
        {
            // Digital: low hum with soft data-processing texture
            var buf = MakeBuffer(3.0f);
            float baseHum = epoch == 7 ? 220f : 260f;
            // Soft triangle hum (not square — avoids harsh buzz)
            AddTone(buf, 0f, 3.0f, baseHum, baseHum + 3f, Wave.Triangle, 0.015f, 0.3f, 0.3f);
            AddTone(buf, 0f, 3.0f, baseHum * 0.5f, baseHum * 0.5f + 2f, Wave.Triangle, 0.01f, 0.3f, 0.3f);
            // Data processing blips (triangle instead of square, lower volume)
            for (int i = 0; i < 8; i++)
            {
                float t = i * 0.37f;
                float freq = 600f + (i % 3) * 150f;
                AddTone(buf, t, 0.03f, freq, freq, Wave.Triangle, 0.012f, 0.002f, 0.01f);
            }
            // Low digital drone
            AddTone(buf, 0f, 3.0f, 100f, 102f, Wave.Triangle, 0.02f, 0.3f, 0.3f);
            ApplyLoopFade(buf);
            return MakeClip($"Ambient_Digital_{epoch}", buf);
        }

        private static AudioClip MakeCosmicAmbient()
        {
            // Transcendent: ethereal tones, cosmic resonance
            var buf = MakeBuffer(4.0f);
            // Deep cosmic drone
            AddTone(buf, 0f, 4.0f, 55f, 58f, Wave.Triangle, 0.06f, 0.5f, 0.5f);
            // Ethereal high tones
            AddTone(buf, 0f, 4.0f, 660f, 665f, Wave.Triangle, 0.02f, 0.5f, 0.5f);
            AddTone(buf, 0.5f, 3.0f, 880f, 878f, Wave.Triangle, 0.015f, 0.5f, 0.5f);
            // Shimmer (triangle to avoid harsh buzz)
            AddTone(buf, 1.0f, 2.0f, 1320f, 1325f, Wave.Triangle, 0.008f, 0.3f, 0.5f);
            // Soft noise texture
            AddTone(buf, 0f, 4.0f, 200f, 180f, Wave.Noise, 0.015f, 0.5f, 0.5f);
            ApplyLoopFade(buf);
            return MakeClip("Ambient_Cosmic", buf);
        }

        // ═══════════════════════════════════════════════════════════════
        //  BOSS PHASE-CHANGE ROAR (Sprint 9)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Intense boss phase-change roar — deeper and more distorted than regular roar.
        /// Low rumble with heavy distortion and rising overtones.
        /// </summary>
        public static AudioClip GetBossPhaseChangeSFX()
        {
            if (_cache.TryGetValue("sfx_boss_phase_change", out var c)) return c;
            var buf = MakeBuffer(0.9f);
            // Deep rumble foundation
            AddTone(buf, 0f, 0.7f, 50f, 35f, Wave.Sawtooth, 0.35f, 0.01f, 0.2f);
            AddTone(buf, 0f, 0.6f, 70f, 40f, Wave.Square, 0.25f, 0.01f, 0.15f);
            // Noise burst for distortion
            AddTone(buf, 0f, 0.4f, 400f, 100f, Wave.Noise, 0.25f, 0.005f, 0.15f);
            // Rising overtone — signals danger escalation
            AddTone(buf, 0.1f, 0.5f, 100f, 400f, Wave.Sawtooth, 0.15f, 0.03f, 0.1f);
            // Secondary rumble
            AddTone(buf, 0.3f, 0.4f, 60f, 30f, Wave.Triangle, 0.2f, 0.02f, 0.15f);
            // Noise tail
            AddTone(buf, 0.5f, 0.3f, 200f, 80f, Wave.Noise, 0.12f, 0.02f, 0.15f);
            var clip = MakeClip("SFX_BossPhaseChange", buf);
            _cache["sfx_boss_phase_change"] = clip;
            return clip;
        }

        /// <summary>Crossfade edges for seamless looping (50ms fade in/out).</summary>
        private static void ApplyLoopFade(float[] buf)
        {
            int fadeLen = (int)(0.10f * SAMPLE_RATE);
            fadeLen = Mathf.Min(fadeLen, buf.Length / 4);
            for (int i = 0; i < fadeLen; i++)
            {
                float fade = (float)i / fadeLen;
                buf[i] *= fade;
                buf[buf.Length - 1 - i] *= fade;
            }
        }

        // Note frequency helpers (flats/sharps for variant progressions)
        const float _Bb2 = 116.54f, _B2_flat = 116.54f;
        const float _Eb3 = 155.56f, _Bb3 = 233.08f;
        const float _D2 = 73.42f, _E2 = 82.41f;
    }
}
