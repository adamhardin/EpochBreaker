using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EpochBreaker.Gameplay;
using EpochBreaker.Generative;

namespace EpochBreaker.Tests.PlayMode
{
    /// <summary>
    /// Smoke tests that exercise major state transitions multiple times
    /// to catch lifecycle bugs (stale singletons, deferred Destroy races,
    /// leaked objects, camera nulls, etc.).
    ///
    /// These are the automated equivalent of "play 3 levels in a row"
    /// manual playtests. They should be run before every deployment.
    /// </summary>
    [TestFixture]
    [Category("Smoke")]
    public class SmokeTests
    {
        [SetUp]
        public void SetUp()
        {
            TestHelpers.CreateGameManager();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.CleanupAll();
            Time.timeScale = 1f;
        }

        // ── Test 1: Multi-level lifecycle ──
        // The bug that prompted this test suite: CameraController.Instance
        // was null on 2nd level load due to a singleton Awake/Destroy race.
        [Test]
        public void MultiLevel_StateTransitions_NoNullSingletons()
        {
            var gm = GameManager.Instance;
            Assert.IsNotNull(gm, "GameManager should exist");

            // Simulate 3 full level cycles: Title → Loading → Playing → Complete → repeat
            for (int i = 0; i < 3; i++)
            {
                gm.TransitionTo(GameState.TitleScreen);
                Assert.AreEqual(GameState.TitleScreen, gm.CurrentState,
                    $"Cycle {i}: should be at TitleScreen");

                gm.TransitionTo(GameState.Loading);
                // Loading immediately calls StartLevel() which transitions to Playing
                Assert.AreEqual(GameState.Playing, gm.CurrentState,
                    $"Cycle {i}: Loading should auto-transition to Playing");

                gm.CompleteLevel();
                Assert.AreEqual(GameState.LevelComplete, gm.CurrentState,
                    $"Cycle {i}: should be LevelComplete");

                // Verify GameManager singleton survived the cycle
                Assert.IsNotNull(GameManager.Instance,
                    $"Cycle {i}: GameManager.Instance should not be null after level complete");
            }
        }

        // ── Test 2: Death and respawn cycle ──
        [Test]
        public void DeathRespawn_MultipleDeaths_StateRemainsConsistent()
        {
            var gm = GameManager.Instance;

            gm.TransitionTo(GameState.Loading);
            Assert.AreEqual(GameState.Playing, gm.CurrentState);

            // Simulate multiple deaths without exhausting lives
            for (int i = 0; i < 2; i++)
            {
                var result = gm.LoseLife();
                Assert.AreEqual(DeathResult.Respawn, result,
                    $"Death {i}: should respawn (lives remaining)");
                Assert.AreEqual(GameState.Playing, gm.CurrentState,
                    $"Death {i}: state should still be Playing after respawn");
            }

            // Verify we can still complete the level after deaths
            gm.CompleteLevel();
            Assert.AreEqual(GameState.LevelComplete, gm.CurrentState);
        }

        // ── Test 3: Pause/Resume cycle ──
        [Test]
        public void PauseResume_MultipleCycles_TimeScaleRestored()
        {
            var gm = GameManager.Instance;
            gm.TransitionTo(GameState.Loading);

            for (int i = 0; i < 5; i++)
            {
                gm.PauseGame();
                Assert.AreEqual(GameState.Paused, gm.CurrentState,
                    $"Pause cycle {i}: should be Paused");
                Assert.AreEqual(0f, Time.timeScale,
                    $"Pause cycle {i}: timeScale should be 0");

                gm.ResumeGame();
                Assert.AreEqual(GameState.Playing, gm.CurrentState,
                    $"Pause cycle {i}: should be Playing after resume");
                Assert.AreEqual(1f, Time.timeScale,
                    $"Pause cycle {i}: timeScale should be 1");
            }
        }

        // ── Test 4: Title → Play → Title → Play (re-entry) ──
        [Test]
        public void TitleReentry_SecondPlaySession_Works()
        {
            var gm = GameManager.Instance;

            // First session
            gm.TransitionTo(GameState.Loading);
            Assert.AreEqual(GameState.Playing, gm.CurrentState);
            gm.CompleteLevel();
            gm.ReturnToTitle();
            Assert.AreEqual(GameState.TitleScreen, gm.CurrentState);

            // Second session (this is where the camera bug manifested)
            gm.TransitionTo(GameState.Loading);
            Assert.AreEqual(GameState.Playing, gm.CurrentState,
                "Second play session should transition to Playing");

            // Third session for good measure
            gm.CompleteLevel();
            gm.ReturnToTitle();
            gm.TransitionTo(GameState.Loading);
            Assert.AreEqual(GameState.Playing, gm.CurrentState,
                "Third play session should also work");
        }

        // ── Test 5: Full death cycle → GameOver → Title → Play ──
        [Test]
        public void FullDeathCycle_GameOver_CanStartNewGame()
        {
            var gm = GameManager.Instance;

            gm.TransitionTo(GameState.Loading);
            Assert.AreEqual(GameState.Playing, gm.CurrentState);

            // Exhaust all lives
            DeathResult result = DeathResult.Respawn;
            int safety = 0;
            while (result != DeathResult.GameOver && result != DeathResult.LevelFailed && safety < 20)
            {
                result = gm.LoseLife();
                safety++;
            }
            // Should eventually run out of lives
            Assert.IsTrue(result == DeathResult.GameOver || result == DeathResult.LevelFailed,
                "Should eventually exhaust lives");

            // Return to title and start fresh
            gm.ReturnToTitle();
            Assert.AreEqual(GameState.TitleScreen, gm.CurrentState);

            gm.TransitionTo(GameState.Loading);
            Assert.AreEqual(GameState.Playing, gm.CurrentState,
                "Should be able to start a new game after GameOver");
        }

        // ── Test 6: Restart level mid-play ──
        [Test]
        public void RestartLevel_MidPlay_TransitionsCleanly()
        {
            var gm = GameManager.Instance;

            gm.TransitionTo(GameState.Loading);
            Assert.AreEqual(GameState.Playing, gm.CurrentState);

            // Restart mid-play
            gm.RestartLevel();
            Assert.AreEqual(GameState.Playing, gm.CurrentState,
                "RestartLevel should re-enter Playing state");

            // Should still be functional
            gm.CompleteLevel();
            Assert.AreEqual(GameState.LevelComplete, gm.CurrentState);
        }

        // ── Test 7: Ghost replay path ──
        [Test]
        public void GhostReplay_ThenNormalPlay_NoStateCorruption()
        {
            var gm = GameManager.Instance;

            // Start a normal level first to have a valid level code
            var testID = LevelID.Create(3, 42UL);
            gm.StartLevelFromCode(testID.ToCode());
            Assert.AreEqual(GameState.Playing, gm.CurrentState);

            gm.CompleteLevel();
            gm.ReturnToTitle();

            // Start ghost replay
            gm.StartGhostReplay(testID.ToCode());
            Assert.AreEqual(GameState.Playing, gm.CurrentState);
            Assert.IsTrue(gm.IsGhostReplay, "Should be in ghost replay mode");

            gm.CompleteLevel();
            gm.ReturnToTitle();

            // Normal play after ghost should work
            gm.StartGame();
            Assert.AreEqual(GameState.Playing, gm.CurrentState,
                "Normal play after ghost replay should work");
            Assert.IsFalse(gm.IsGhostReplay,
                "Ghost replay flag should be cleared for normal play");
        }

        // ── Test 8: Level code entry → Complete → Title → Code entry again ──
        [Test]
        public void LevelFromCode_MultipleEntries_Works()
        {
            var gm = GameManager.Instance;

            var codes = new[]
            {
                LevelID.Create(0, 111UL).ToCode(),
                LevelID.Create(5, 222UL).ToCode(),
                LevelID.Create(9, 333UL).ToCode()
            };

            foreach (var code in codes)
            {
                gm.StartLevelFromCode(code);
                Assert.AreEqual(GameState.Playing, gm.CurrentState,
                    $"Code {code}: should be Playing");

                gm.CompleteLevel();
                Assert.AreEqual(GameState.LevelComplete, gm.CurrentState);

                gm.ReturnToTitle();
                Assert.AreEqual(GameState.TitleScreen, gm.CurrentState);
            }
        }

        // ── Test 9: Score reset between levels ──
        [Test]
        public void ScoreState_ResetsBetweenLevels()
        {
            var gm = GameManager.Instance;

            gm.TransitionTo(GameState.Loading);
            // Score should be reset for new level
            Assert.AreEqual(0, gm.EnemiesKilled, "Enemies killed should reset on new level");
            Assert.AreEqual(0, gm.RewardsCollected, "Rewards should reset on new level");
            Assert.AreEqual(0f, gm.LevelElapsedTime, "Timer should reset on new level");
        }

        // ── Test 10: Rapid state transitions (stress test) ──
        [Test]
        public void RapidTransitions_NoExceptions()
        {
            var gm = GameManager.Instance;

            // Rapidly cycle through states — should not throw
            for (int i = 0; i < 10; i++)
            {
                gm.TransitionTo(GameState.TitleScreen);
                gm.TransitionTo(GameState.Loading);
                gm.PauseGame();
                gm.ResumeGame();
                gm.CompleteLevel();
                gm.ReturnToTitle();
            }

            Assert.AreEqual(GameState.TitleScreen, gm.CurrentState,
                "Should end at TitleScreen after rapid transitions");
            Assert.IsNotNull(GameManager.Instance,
                "GameManager should survive rapid state cycling");
        }

        // ── Test 11: Campaign progression across multiple levels ──
        [Test]
        public void CampaignMode_AdvancesEpoch()
        {
            var gm = GameManager.Instance;
            gm.StartGame(); // Starts at epoch 0

            int startEpoch = gm.CampaignEpoch;

            gm.CompleteLevel();
            gm.NextLevel();

            // NextLevel should advance CampaignEpoch and transition to Loading→Playing
            Assert.AreEqual(GameState.Playing, gm.CurrentState,
                "Should be playing after NextLevel");
            Assert.AreEqual(startEpoch + 1, gm.CampaignEpoch,
                "CampaignEpoch should advance by 1");
        }

        // ── Test 12: AudioManager survives level transitions ──
        [Test]
        public void AudioManager_PersistsAcrossLevels()
        {
            var gm = GameManager.Instance;

            // AudioManager is added as a component on the GameManager GO in Awake
            Assert.IsNotNull(AudioManager.Instance, "AudioManager should exist");

            gm.TransitionTo(GameState.Loading);
            Assert.IsNotNull(AudioManager.Instance,
                "AudioManager should persist through Loading transition");

            gm.CompleteLevel();
            gm.ReturnToTitle();
            Assert.IsNotNull(AudioManager.Instance,
                "AudioManager should persist after returning to title");

            gm.TransitionTo(GameState.Loading);
            Assert.IsNotNull(AudioManager.Instance,
                "AudioManager should persist through second level load");
        }
    }
}
