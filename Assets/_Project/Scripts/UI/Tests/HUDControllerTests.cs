using NUnit.Framework;
using UnityEngine;

namespace Platformer.UI.Tests
{
    public class HUDControllerTests
    {
        private GameObject _go;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("HUDController");
            _go.AddComponent<HUDController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        [Test]
        public void HUDController_CanBeCreated()
        {
            Assert.IsNotNull(_go.GetComponent<HUDController>());
        }

        [Test]
        public void HUDController_EnableDisable_DoesNotThrow()
        {
            var hud = _go.GetComponent<HUDController>();
            Assert.DoesNotThrow(() =>
            {
                hud.enabled = false;
                hud.enabled = true;
            });
        }
    }
}
