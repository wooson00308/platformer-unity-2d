using NUnit.Framework;
using UnityEngine;

namespace Platformer.Data.Tests
{
    public class GameEventTests
    {
        [Test]
        public void Raise_CallsRegisteredListener()
        {
            var gameEvent = ScriptableObject.CreateInstance<GameEvent>();
            bool called = false;

            gameEvent.AddListener(() => called = true);
            gameEvent.Raise();

            Assert.IsTrue(called);
            Object.DestroyImmediate(gameEvent);
        }

        [Test]
        public void RemoveListener_StopsReceivingRaise()
        {
            var gameEvent = ScriptableObject.CreateInstance<GameEvent>();
            int callCount = 0;
            void Listener() => callCount++;

            gameEvent.AddListener(Listener);
            gameEvent.Raise();
            gameEvent.RemoveListener(Listener);
            gameEvent.Raise();

            Assert.AreEqual(1, callCount);
            Object.DestroyImmediate(gameEvent);
        }

        [Test]
        public void Raise_WithNoListeners_DoesNotThrow()
        {
            var gameEvent = ScriptableObject.CreateInstance<GameEvent>();

            Assert.DoesNotThrow(() => gameEvent.Raise());
            Object.DestroyImmediate(gameEvent);
        }
    }
}
