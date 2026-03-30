using UnityEngine;
using Platformer.Data;

namespace Platformer.Core
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.1f;
        [SerializeField] private LayerMask _groundLayer;

        private Rigidbody2D _rb;
        private PlayerState _state = PlayerState.Idle;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        void OnEnable()
        {
        }

        void Start()
        {
        }

        void Update()
        {
        }

        void FixedUpdate()
        {
        }

        void OnDisable()
        {
        }

        private bool IsGrounded()
            => Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        private void SetState(PlayerState newState)
        {
            if (_state == newState) return;
            _state = newState;
        }
    }
}
