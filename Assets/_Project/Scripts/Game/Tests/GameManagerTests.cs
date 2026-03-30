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
            _go.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void Instance_IsNotNull_AfterAwake()
        {
            Assert.IsNotNull(GameManager.Instance);
        }

        [Test]
        public void Instance_IsSameObject()
        {
            Assert.AreEqual(_go.GetComponent<GameManager>(), GameManager.Instance);
        }
    }
}
