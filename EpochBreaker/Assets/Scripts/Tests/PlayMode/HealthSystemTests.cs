using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EpochBreaker.Gameplay;

namespace EpochBreaker.Tests.PlayMode
{
    [TestFixture]
    [Category("HealthSystem")]
    public class HealthSystemTests
    {
        private GameObject _playerObj;
        private HealthSystem _health;

        [SetUp]
        public void SetUp()
        {
            TestHelpers.CreateGameManager();
            TestHelpers.CreateCheckpointManager();
            _playerObj = TestHelpers.CreatePlayerObject();
            _health = _playerObj.GetComponent<HealthSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObj != null)
                Object.DestroyImmediate(_playerObj);
            TestHelpers.CleanupAll();
            Time.timeScale = 1f;
        }

        [Test]
        public void InitialHealth_IsMaxHealth()
        {
            Assert.AreEqual(5, _health.MaxHealth);
            Assert.AreEqual(5, _health.CurrentHealth);
            Assert.IsFalse(_health.IsDead);
        }

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            _health.TakeDamage(1, Vector2.zero);
            Assert.AreEqual(4, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_MultipleTimes_ReducesCorrectly()
        {
            _health.TakeDamage(2, Vector2.zero);
            Assert.AreEqual(3, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ToZero_TriggersDeath()
        {
            bool deathTriggered = false;
            _health.OnDeath += () => deathTriggered = true;

            _health.TakeDamage(5, Vector2.zero);
            Assert.AreEqual(0, _health.CurrentHealth);
            Assert.IsTrue(_health.IsDead);
            Assert.IsTrue(deathTriggered);
        }

        [Test]
        public void TakeDamage_ClampsToZero()
        {
            _health.TakeDamage(10, Vector2.zero);
            Assert.AreEqual(0, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_ActivatesInvulnerability()
        {
            _health.TakeDamage(1, Vector2.zero);
            Assert.IsTrue(_health.IsInvulnerable);
        }

        [Test]
        public void TakeDamage_WhileInvulnerable_NoDamage()
        {
            _health.TakeDamage(1, Vector2.zero);
            Assert.IsTrue(_health.IsInvulnerable);
            Assert.AreEqual(4, _health.CurrentHealth);

            // Second hit during i-frames should do nothing
            _health.TakeDamage(1, Vector2.zero);
            Assert.AreEqual(4, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_WhileDead_NoDamage()
        {
            _health.TakeDamage(5, Vector2.zero);
            Assert.IsTrue(_health.IsDead);

            _health.TakeDamage(1, Vector2.zero);
            Assert.AreEqual(0, _health.CurrentHealth);
        }

        [Test]
        public void Heal_RestoresHealth()
        {
            _health.TakeDamage(3, Vector2.zero);
            _health.Heal(2);
            Assert.AreEqual(4, _health.CurrentHealth);
        }

        [Test]
        public void Heal_ClampsToMax()
        {
            _health.TakeDamage(1, Vector2.zero);
            _health.Heal(10);
            Assert.AreEqual(5, _health.CurrentHealth);
        }

        [Test]
        public void Heal_WhileDead_NoEffect()
        {
            _health.TakeDamage(5, Vector2.zero);
            Assert.IsTrue(_health.IsDead);

            _health.Heal(3);
            Assert.AreEqual(0, _health.CurrentHealth);
        }

        [Test]
        public void ResetHealth_RestoresToFull()
        {
            _health.TakeDamage(3, Vector2.zero);
            _health.ResetHealth();
            Assert.AreEqual(5, _health.CurrentHealth);
            Assert.IsFalse(_health.IsInvulnerable);
        }

        [Test]
        public void OnHealthChanged_FiresOnDamage()
        {
            int reportedCurrent = -1;
            int reportedMax = -1;
            _health.OnHealthChanged += (c, m) => { reportedCurrent = c; reportedMax = m; };

            _health.TakeDamage(1, Vector2.zero);
            Assert.AreEqual(4, reportedCurrent);
            Assert.AreEqual(5, reportedMax);
        }

        [Test]
        public void OnDamage_FiresOnHit()
        {
            bool damageEventFired = false;
            _health.OnDamage += () => damageEventFired = true;

            _health.TakeDamage(1, Vector2.zero);
            Assert.IsTrue(damageEventFired);
        }

        [UnityTest]
        public IEnumerator Invulnerability_ExpiresAfterDuration()
        {
            _health.TakeDamage(1, Vector2.zero);
            Assert.IsTrue(_health.IsInvulnerable);

            // Invulnerability lasts 1 second. Wait slightly longer.
            yield return new WaitForSeconds(1.1f);

            Assert.IsFalse(_health.IsInvulnerable);
        }
    }
}
