using NUnit.Framework;
using UnityEngine;
using EpochBreaker.Gameplay;
using EpochBreaker.Generative;

namespace EpochBreaker.Tests.PlayMode
{
    [TestFixture]
    [Category("WeaponSystem")]
    public class WeaponSystemTests
    {
        private GameObject _playerObj;
        private WeaponSystem _weapon;

        [SetUp]
        public void SetUp()
        {
            TestHelpers.CreateGameManager();
            _playerObj = TestHelpers.CreatePlayerObject();
            _weapon = _playerObj.GetComponent<WeaponSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObj != null)
                Object.DestroyImmediate(_playerObj);
            TestHelpers.CleanupAll();
        }

        [Test]
        public void InitialTier_IsStarting()
        {
            Assert.AreEqual(WeaponTier.Starting, _weapon.CurrentTier);
        }

        [Test]
        public void UpgradeWeapon_ToMedium()
        {
            _weapon.UpgradeWeapon(WeaponTier.Medium);
            Assert.AreEqual(WeaponTier.Medium, _weapon.CurrentTier);
        }

        [Test]
        public void UpgradeWeapon_ToHeavy()
        {
            _weapon.UpgradeWeapon(WeaponTier.Heavy);
            Assert.AreEqual(WeaponTier.Heavy, _weapon.CurrentTier);
        }

        [Test]
        public void UpgradeWeapon_SkipTier_StillUpgrades()
        {
            _weapon.UpgradeWeapon(WeaponTier.Heavy);
            Assert.AreEqual(WeaponTier.Heavy, _weapon.CurrentTier);
        }

        [Test]
        public void UpgradeWeapon_CannotDowngrade()
        {
            _weapon.UpgradeWeapon(WeaponTier.Heavy);
            _weapon.UpgradeWeapon(WeaponTier.Medium);
            Assert.AreEqual(WeaponTier.Heavy, _weapon.CurrentTier);
        }

        [Test]
        public void UpgradeWeapon_SameTier_NoChange()
        {
            _weapon.UpgradeWeapon(WeaponTier.Starting);
            Assert.AreEqual(WeaponTier.Starting, _weapon.CurrentTier);
        }

        [Test]
        public void UpgradeSequence_StartingMediumHeavy()
        {
            Assert.AreEqual(WeaponTier.Starting, _weapon.CurrentTier);
            _weapon.UpgradeWeapon(WeaponTier.Medium);
            Assert.AreEqual(WeaponTier.Medium, _weapon.CurrentTier);
            _weapon.UpgradeWeapon(WeaponTier.Heavy);
            Assert.AreEqual(WeaponTier.Heavy, _weapon.CurrentTier);
        }
    }
}
