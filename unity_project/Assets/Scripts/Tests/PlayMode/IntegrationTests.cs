using NUnit.Framework;
using UnityEngine;
using SixteenBit.Gameplay;
using SixteenBit.Generative;

namespace SixteenBit.Tests.PlayMode
{
    [TestFixture]
    [Category("Integration")]
    public class IntegrationTests
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

        [Test]
        public void LevelGeneration_ProducesValidData()
        {
            var generator = new LevelGenerator();
            var id = LevelID.Create(3, 99999UL);
            var level = generator.Generate(id);

            Assert.IsNotNull(level);
            Assert.IsNotNull(level.Layout.Tiles);
            Assert.Greater(level.Layout.WidthTiles, 0);
            Assert.Greater(level.Layout.HeightTiles, 0);
            Assert.AreEqual(
                level.Layout.WidthTiles * level.Layout.HeightTiles,
                level.Layout.Tiles.Length);
        }

        [Test]
        public void LevelGeneration_DeterministicAcrossRuns()
        {
            var generator = new LevelGenerator();
            var id = LevelID.Create(5, 42UL);

            var level1 = generator.Generate(id);
            var level2 = generator.Generate(id);

            Assert.AreEqual(level1.ComputeHash(), level2.ComputeHash());
        }

        [Test]
        public void LevelGeneration_DifferentSeeds_DifferentLevels()
        {
            var generator = new LevelGenerator();
            var id1 = LevelID.Create(0, 111UL);
            var id2 = LevelID.Create(0, 222UL);

            var level1 = generator.Generate(id1);
            var level2 = generator.Generate(id2);

            Assert.AreNotEqual(level1.ComputeHash(), level2.ComputeHash());
        }

        [Test]
        public void LevelGeneration_HasEnemies()
        {
            var generator = new LevelGenerator();
            var id = LevelID.Create(4, 55555UL);
            var level = generator.Generate(id);

            Assert.IsNotNull(level.Enemies);
            Assert.Greater(level.Enemies.Count, 0);
        }

        [Test]
        public void LevelGeneration_HasCheckpoints()
        {
            var generator = new LevelGenerator();
            var id = LevelID.Create(2, 77777UL);
            var level = generator.Generate(id);

            Assert.IsNotNull(level.Checkpoints);
            Assert.Greater(level.Checkpoints.Count, 0);
        }

        [Test]
        public void GameManager_FullLifecycle()
        {
            var gm = GameManager.Instance;
            Assert.IsNotNull(gm);

            // Start at title
            gm.TransitionTo(GameState.TitleScreen);
            Assert.AreEqual(GameState.TitleScreen, gm.CurrentState);

            // Transition to Playing
            gm.TransitionTo(GameState.Playing);
            Assert.AreEqual(GameState.Playing, gm.CurrentState);

            // Pause and resume
            gm.PauseGame();
            Assert.AreEqual(GameState.Paused, gm.CurrentState);
            gm.ResumeGame();
            Assert.AreEqual(GameState.Playing, gm.CurrentState);

            // Complete level
            gm.CompleteLevel();
            Assert.AreEqual(GameState.LevelComplete, gm.CurrentState);

            // Return to title
            gm.ReturnToTitle();
            Assert.AreEqual(GameState.TitleScreen, gm.CurrentState);
        }
    }
}
