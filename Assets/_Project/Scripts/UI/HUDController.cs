using UnityEngine;
using Platformer.Data;

namespace Platformer.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private GameEvent _onPlayerDied;

        void OnEnable()
        {
            if (_onPlayerDied != null)
                _onPlayerDied.AddListener(HandlePlayerDied);
        }

        void Start()
        {
        }

        void OnDisable()
        {
            if (_onPlayerDied != null)
                _onPlayerDied.RemoveListener(HandlePlayerDied);
        }

        private void HandlePlayerDied()
        {
        }
    }
}
