using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Platformer.Game.Tests
{
    public class MovingPlatformTests
    {
        [Test]
        public void MovingPlatform_CanBeAddedWithRigidbody2D()
        {
            var go = new GameObject("Platform");
            go.AddComponent<Rigidbody2D>();
            var mp = go.AddComponent<MovingPlatform>();

            Assert.NotNull(mp);

            Object.DestroyImmediate(go);
        }

#if UNITY_EDITOR
        [Test]
        public void Prefab_MovingPlatform_HasSettingsAssigned()
        {
            const string path = "Assets/_Project/Prefabs/MovingPlatform.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Assert.NotNull(prefab, "MovingPlatform.prefab 이 경로에 있어야 함: " + path);

            var mp = prefab.GetComponent<MovingPlatform>();
            Assert.NotNull(mp);

            var so = new SerializedObject(mp);
            var settingsProp = so.FindProperty("_settings");
            Assert.NotNull(settingsProp);
            Assert.NotNull(settingsProp.objectReferenceValue, "Inspector에서 MovingPlatformSettings 연결 필요.");
        }

        [Test]
        public void Prefab_HasTilemapAndCollider_ForSameLookAsGround()
        {
            const string path = "Assets/_Project/Prefabs/MovingPlatform.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.NotNull(prefab);

            var tm = prefab.GetComponentInChildren<Tilemap>(true);
            Assert.NotNull(tm, "Tilemap 이 있어야 메인 지면이랑 같은 렌더링.");

            var tcol = prefab.GetComponentInChildren<TilemapCollider2D>(true);
            Assert.NotNull(tcol);
        }
#endif
    }
}
