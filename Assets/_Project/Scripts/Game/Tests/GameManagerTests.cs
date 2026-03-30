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
            // EditMode에서는 Awake가 자동 호출되지 않으므로 수동 트리거
            _go.AddComponent<GameManager>().SendMessage("Awake");
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
