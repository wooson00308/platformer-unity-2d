using NUnit.Framework;
using UnityEngine;

namespace Platformer.Game.Tests
{
    public class EnemyMeleeChaserTests
    {
        [Test]
        public void EnemyMeleeChaser_CanBeAddedAsComponent()
        {
            var go = new GameObject("Enemy");
            go.AddComponent<Rigidbody2D>();

            var enemy = go.AddComponent<EnemyMeleeChaser>();

            Assert.IsNotNull(enemy);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnemyMeleeChaser_TakeDamage_OneHitDestroysEnemy()
        {
            var go = new GameObject("Enemy");
            go.AddComponent<Rigidbody2D>();
            var enemy = go.AddComponent<EnemyMeleeChaser>();

            enemy.TakeDamage(1);

            Assert.IsTrue(enemy == null);
        }
    }
}
