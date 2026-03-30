using UnityEngine;

namespace Platformer.Game
{
    public class EnemyStompZone : MonoBehaviour
    {
        [SerializeField] private EnemyMeleeChaser _owner;

        void Awake()
        {
            if (_owner == null)
                _owner = GetComponentInParent<EnemyMeleeChaser>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_owner == null)
                return;

            _owner.TryStompByPlayerCollider(other);
        }
    }
}
