using NUnit.Framework;
using UnityEngine;
using Platformer.Data;

namespace Platformer.Data.Tests
{
    public class RespawnDataTests
    {
        private RespawnData _data;

        [SetUp]
        public void SetUp()
        {
            _data = ScriptableObject.CreateInstance<RespawnData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_data);
        }

        [Test]
        public void SetCheckpoint_SetsHasCheckpointTrue()
        {
            _data.SetCheckpoint(new Vector2(5f, 3f));
            Assert.IsTrue(_data.hasCheckpoint);
        }

        [Test]
        public void SetCheckpoint_StoresPosition()
        {
            var pos = new Vector2(5f, 3f);
            _data.SetCheckpoint(pos);
            Assert.AreEqual(pos, _data.position);
        }

        [Test]
        public void Reset_ClearsCheckpoint()
        {
            _data.SetCheckpoint(new Vector2(5f, 3f));
            _data.Reset();
            Assert.IsFalse(_data.hasCheckpoint);
            Assert.AreEqual(Vector2.zero, _data.position);
        }
    }
}
