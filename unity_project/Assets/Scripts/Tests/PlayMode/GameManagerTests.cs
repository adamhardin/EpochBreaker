using NUnit.Framework;
using UnityEngine;
using SixteenBit.Gameplay;
using SixteenBit.Generative;

namespace SixteenBit.Tests.PlayMode
{
    [TestFixture]
    [Category("GameManager")]
    public class GameManagerTests
    {
        private GameObject _gmObj;

        [SetUp]
        public void SetUp()
        {
            _gmObj = TestHelpers.CreateGameManager();
        }

        [TearDown]
        public void TearDown()
        {
            TestHelpers.CleanupAll();
            Time.timeScale = 1f;
        }

        [Test]
        public void Singleton_IsCreated()
        {
            Assert.IsNotNull(GameManager.Instance);
        }

        [Test]
        public void InitialState_IsTitleScreen()
        {
            Assert.AreEqual(GameState.TitleScreen, GameManager.Instance.CurrentState);
        }

        [Test]
        public void TransitionTo_Playing_SetsState()
        {
            GameManager.Instance.TransitionTo(GameState.Playing);
            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState);
        }

        [Test]
        public void PauseGame_FromPlaying_SetsPaused()
        {
            GameManager.Instance.TransitionTo(GameState.Playing);
            GameManager.Instance.PauseGame();
            Assert.AreEqual(GameState.Paused, GameManager.Instance.CurrentState);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void PauseGame_FromNonPlaying_NoEffect()
        {
            GameManager.Instance.TransitionTo(GameState.TitleScreen);
            GameManager.Instance.PauseGame();
            Assert.AreEqual(GameState.TitleScreen, GameManager.Instance.CurrentState);
        }

        [Test]
        public void ResumeGame_FromPaused_SetsPlaying()
        {
            GameManager.Instance.TransitionTo(GameState.Playing);
            GameManager.Instance.PauseGame();
            Assert.AreEqual(GameState.Paused, GameManager.Instance.CurrentState);

            GameManager.Instance.ResumeGame();
            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void ResumeGame_FromNonPaused_NoEffect()
        {
            GameManager.Instance.TransitionTo(GameState.Playing);
            GameManager.Instance.ResumeGame();
            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState);
        }

        [Test]
        public void CompleteLevel_FromPlaying_SetsLevelComplete()
        {
            GameManager.Instance.TransitionTo(GameState.Playing);
            GameManager.Instance.CompleteLevel();
            Assert.AreEqual(GameState.LevelComplete, GameManager.Instance.CurrentState);
        }

        [Test]
        public void CompleteLevel_FromNonPlaying_NoEffect()
        {
            GameManager.Instance.TransitionTo(GameState.TitleScreen);
            GameManager.Instance.CompleteLevel();
            Assert.AreEqual(GameState.TitleScreen, GameManager.Instance.CurrentState);
        }

        [Test]
        public void Score_CanBeModified()
        {
            GameManager.Instance.Score = 0;
            GameManager.Instance.Score += 100;
            Assert.AreEqual(100, GameManager.Instance.Score);
        }

        [Test]
        public void Score_AccumulatesCorrectly()
        {
            GameManager.Instance.Score = 0;
            GameManager.Instance.Score += 100;
            GameManager.Instance.Score += 250;
            Assert.AreEqual(350, GameManager.Instance.Score);
        }

        [Test]
        public void ReturnToTitle_SetsCorrectState()
        {
            GameManager.Instance.TransitionTo(GameState.Playing);
            GameManager.Instance.ReturnToTitle();
            Assert.AreEqual(GameState.TitleScreen, GameManager.Instance.CurrentState);
        }

        [Test]
        public void CurrentLevelID_IsValidAfterStartGame()
        {
            // StartGame generates a random LevelID
            GameManager.Instance.StartGame();
            var id = GameManager.Instance.CurrentLevelID;

            Assert.IsTrue(id.Epoch >= 0 && id.Epoch <= LevelID.MAX_EPOCH,
                "Epoch should be in valid range");
            Assert.IsTrue(id.Seed > 0, "Seed should be non-zero");
        }

        [Test]
        public void NextLevel_AdvancesEpochAndChangesSeed()
        {
            // Start with a known level
            GameManager.Instance.StartGame();
            var originalID = GameManager.Instance.CurrentLevelID;
            int originalEpoch = originalID.Epoch;

            // Call NextLevel
            GameManager.Instance.NextLevel();
            var newID = GameManager.Instance.CurrentLevelID;

            // Epoch should advance (with wrap)
            int expectedEpoch = (originalEpoch + 1) % (LevelID.MAX_EPOCH + 1);
            Assert.AreEqual(expectedEpoch, newID.Epoch, "Epoch should advance by 1");
            Assert.AreNotEqual(originalID.Seed, newID.Seed, "Seed should change");
        }

        [Test]
        public void NextLevel_WrapsEpochAtMax()
        {
            // Create a level at epoch 9 and verify wrap
            var epoch9ID = LevelID.Create(9, 12345UL);
            GameManager.Instance.StartLevelFromCode(epoch9ID.ToCode());

            Assert.AreEqual(9, GameManager.Instance.CurrentLevelID.Epoch);

            GameManager.Instance.NextLevel();

            Assert.AreEqual(0, GameManager.Instance.CurrentLevelID.Epoch,
                "Epoch should wrap from 9 to 0");
        }

        [Test]
        public void StartLevelFromCode_SetsCorrectLevelID()
        {
            var testID = LevelID.Create(5, 0xABCDE12345UL);
            string code = testID.ToCode();

            GameManager.Instance.StartLevelFromCode(code);

            var currentID = GameManager.Instance.CurrentLevelID;
            Assert.AreEqual(testID.Epoch, currentID.Epoch);
            Assert.AreEqual(testID.Seed, currentID.Seed);
        }

        [Test]
        public void LevelHistory_CanBeSavedAndLoaded()
        {
            // Clear any existing history
            GameManager.ClearLevelHistory();

            // Load should return empty
            var history = GameManager.LoadLevelHistory();
            Assert.AreEqual(0, history.Entries.Count, "History should be empty after clear");
        }

        [Test]
        public void CopyToClipboard_SetsSystemBuffer()
        {
            string testText = "3-TESTCODE";
            GameManager.CopyToClipboard(testText);
            Assert.AreEqual(testText, GUIUtility.systemCopyBuffer);
        }
    }
}
