using UnityEngine;

namespace Platformer.Game
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance => _instance;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        void OnEnable()
        {
        }

        void Start()
        {
        }

        void OnDisable()
        {
        }

        void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
