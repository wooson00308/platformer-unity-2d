using NUnit.Framework;
using Platformer.Data;

namespace Platformer.Core.Tests
{
    public class PlayerStateTests
    {
        [Test]
        public void PlayerState_HasIdle()
            => Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerState), "Idle"));

        [Test]
        public void PlayerState_HasRunning()
            => Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerState), "Running"));

        [Test]
        public void PlayerState_HasJumping()
            => Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerState), "Jumping"));

        [Test]
        public void PlayerState_HasFalling()
            => Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerState), "Falling"));

        [Test]
        public void PlayerState_HasDead()
            => Assert.IsTrue(System.Enum.IsDefined(typeof(PlayerState), "Dead"));
    }
}
