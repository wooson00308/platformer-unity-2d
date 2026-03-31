using UnityEngine;
using Platformer.Data;

namespace Platformer.Game
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private RespawnData _respawnData;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (_respawnData == null) return;

            _respawnData.SetCheckpoint(transform.position);
        }
    }
}
