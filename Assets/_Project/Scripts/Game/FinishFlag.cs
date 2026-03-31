using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer.Data;

namespace Platformer.Game
{
    public class FinishFlag : MonoBehaviour
    {
        [SerializeField] private RespawnData _respawnData;

        private bool _isTriggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isTriggered) return;
            if (!other.CompareTag("Player")) return;

            _isTriggered = true;

            if (_respawnData != null)
                _respawnData.Reset();

#if UNITY_EDITOR
            Debug.Log("[FinishFlag] Level Clear!");
#endif

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
