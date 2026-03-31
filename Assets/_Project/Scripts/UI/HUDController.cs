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
                _onPlayerDied.AddListener(_HandlePlayerDied);
        }

        void Start()
        {
        }

        void OnDisable()
        {
            if (_onPlayerDied != null)
                _onPlayerDied.RemoveListener(_HandlePlayerDied);
        }

        private void _HandlePlayerDied()
        {
        }
    }
}
