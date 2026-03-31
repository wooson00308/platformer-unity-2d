using NUnit.Framework;
using UnityEngine;

namespace Platformer.Data.Tests
{
    public class MovingPlatformSettingsTests
    {
        [Test]
        public void CreateInstance_HasPositiveDefaults()
        {
            var s = ScriptableObject.CreateInstance<MovingPlatformSettings>();

            Assert.Greater(s.moveSpeed, 0f);
            Assert.GreaterOrEqual(s.pauseAtEndpoints, 0f);
            Assert.Greater(s.travelDistance, 0f);
            Assert.Greater(s.localTravelDirection.sqrMagnitude, 0f);

            Object.DestroyImmediate(s);
        }
    }
}
