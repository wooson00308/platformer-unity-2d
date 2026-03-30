using NUnit.Framework;
using UnityEngine;

namespace Platformer.Game.Tests
{
    public class GameManagerTests
    {
        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("GameManager");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void GameManager_CanBeAddedAsComponent()
        {
            var manager = _go.AddComponent<GameManager>();
            Assert.IsNotNull(manager);
        }

        [Test]
        public void GameManager_IsMonoBehaviour()
        {
            var manager = _go.AddComponent<GameManager>();
            Assert.IsInstanceOf<MonoBehaviour>(manager);
        }
    }
}
