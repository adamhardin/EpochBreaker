using NUnit.Framework;
using UnityEngine;
using EpochBreaker.Gameplay;
using EpochBreaker.Generative;

namespace EpochBreaker.Tests.PlayMode
{
    [TestFixture]
    [Category("EnemyBase")]
    public class EnemyBaseTests
    {
        private GameObject _enemyObj;
        private EnemyBase _enemy;

        [SetUp]
        public void SetUp()
        {
            TestHelpers.CreateGameManager();
            GameManager.Instance.TransitionTo(GameState.Playing);

            _enemyObj = TestHelpers.CreateEnemyObject();
            _enemy = _enemyObj.GetComponent<EnemyBase>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyObj != null)
                Object.DestroyImmediate(_enemyObj);
            TestHelpers.CleanupAll();
        }

        [Test]
        public void InitialState_IsNotDead()
        {
            Assert.IsFalse(_enemy.IsDead);
            Assert.AreEqual(3, _enemy.Health);
        }

        [Test]
        public void Initialize_SetsTypeAndBehavior()
        {
            var data = new EnemyData
            {
                Type = EnemyType.Knight,
                Behavior = EnemyBehavior.Patrol,
                PatrolMinX = 0,
                PatrolMaxX = 10,
                DifficultyWeight = 3f
            };
            _enemy.Initialize(data, null);

            Assert.AreEqual(EnemyType.Knight, _enemy.Type);
            Assert.AreEqual(EnemyBehavior.Patrol, _enemy.Behavior);
        }

        [Test]
        public void Initialize_ScalesHealthFromDifficultyWeight()
        {
            var data = new EnemyData
            {
                Type = EnemyType.Soldier,
                Behavior = EnemyBehavior.Chase,
                DifficultyWeight = 5f
            };
            _enemy.Initialize(data, null);
            Assert.AreEqual(5, _enemy.Health);
        }

        [Test]
        public void Initialize_MinHealthIsOne()
        {
            var data = new EnemyData
            {
                Type = EnemyType.Caveman,
                Behavior = EnemyBehavior.Patrol,
                DifficultyWeight = 0.1f
            };
            _enemy.Initialize(data, null);
            Assert.GreaterOrEqual(_enemy.Health, 1);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            _enemy.TakeDamage(1);
            Assert.AreEqual(2, _enemy.Health);
        }

        [Test]
        public void TakeDamage_MultipleTimes()
        {
            _enemy.TakeDamage(1);
            _enemy.TakeDamage(1);
            Assert.AreEqual(1, _enemy.Health);
        }

        [Test]
        public void TakeDamage_ToZero_Dies()
        {
            _enemy.TakeDamage(3);
            Assert.IsTrue(_enemy.IsDead);
        }

        [Test]
        public void TakeDamage_WhileDead_NoEffect()
        {
            _enemy.TakeDamage(3);
            Assert.IsTrue(_enemy.IsDead);

            _enemy.TakeDamage(1);
            Assert.AreEqual(0, _enemy.Health);
        }

        [Test]
        public void Die_AddsScoreToGameManager()
        {
            GameManager.Instance.Score = 0;
            _enemy.TakeDamage(3);
            Assert.AreEqual(100, GameManager.Instance.Score);
        }
    }
}
