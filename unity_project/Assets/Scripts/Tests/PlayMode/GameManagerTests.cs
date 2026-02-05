using NUnit.Framework;
using UnityEngine;
using SixteenBit.Gameplay;

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
        public void NextLevel_AdvancesDifficulty()
        {
            GameManager.Instance.CurrentDifficulty = 0;
            GameManager.Instance.CurrentEra = 0;

            // NextLevel calls TransitionTo(Loading) which triggers StartLevel.
            // We test the difficulty/era math by checking properties after call.
            int prevDiff = GameManager.Instance.CurrentDifficulty;
            GameManager.Instance.NextLevel();
            Assert.AreEqual(prevDiff + 1, GameManager.Instance.CurrentDifficulty);
        }

        [Test]
        public void NextLevel_CyclesDifficultyAndAdvancesEra()
        {
            GameManager.Instance.CurrentDifficulty = 3;
            GameManager.Instance.CurrentEra = 0;

            GameManager.Instance.NextLevel();
            // Difficulty wraps to 0, era advances to 1
            Assert.AreEqual(0, GameManager.Instance.CurrentDifficulty);
            Assert.AreEqual(1, GameManager.Instance.CurrentEra);
        }

        [Test]
        public void NextLevel_EraClampedAtMax()
        {
            GameManager.Instance.CurrentDifficulty = 3;
            GameManager.Instance.CurrentEra = 9;

            GameManager.Instance.NextLevel();
            Assert.AreEqual(0, GameManager.Instance.CurrentDifficulty);
            Assert.AreEqual(9, GameManager.Instance.CurrentEra); // Clamped at 9
        }
    }
}
