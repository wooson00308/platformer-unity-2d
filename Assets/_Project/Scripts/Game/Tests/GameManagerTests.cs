using System.Reflection;
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
            var manager = _go.AddComponent<GameManager>();
            // EditMode에서 Awake 자동 호출 안 됨 — 리플렉션으로 직접 호출
            typeof(GameManager)
                .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(manager, null);
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
