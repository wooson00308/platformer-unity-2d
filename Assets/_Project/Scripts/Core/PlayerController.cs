using UnityEngine;
using UnityEngine.InputSystem;
using Platformer.Data;

namespace Platformer.Core
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private float _fallGravityMultiplier = 2.5f;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask _groundLayer;

        private Rigidbody2D _rb;
        private InputSystem_Actions _input;
        private PlayerState _state = PlayerState.Idle;
        private float _moveInput;
        private bool _jumpRequested;
        private float _defaultGravityScale;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _input = new InputSystem_Actions();
            _defaultGravityScale = _rb.gravityScale;
        }

        void OnEnable()
        {
            _input.Player.Enable();
        }

        void Start()
        {
        }

        void Update()
        {
            _moveInput = _input.Player.Move.ReadValue<Vector2>().x;

            if (_input.Player.Jump.WasPressedThisFrame() && IsGrounded())
                _jumpRequested = true;

            UpdateState();
        }

        void FixedUpdate()
        {
            _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed, _rb.linearVelocity.y);

            if (_jumpRequested)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _jumpRequested = false;
            }

            // 떨어질 때 중력 강화 — 더 묵직한 점프감
            _rb.gravityScale = _rb.linearVelocity.y < 0f
                ? _defaultGravityScale * _fallGravityMultiplier
                : _defaultGravityScale;
        }

        void OnDisable()
        {
            _input.Player.Disable();
        }

        private bool IsGrounded()
            => Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);

        private void UpdateState()
        {
            if (!IsGrounded())
            {
                SetState(_rb.linearVelocity.y > 0f ? PlayerState.Jumping : PlayerState.Falling);
                return;
            }
            SetState(Mathf.Abs(_moveInput) > 0.01f ? PlayerState.Running : PlayerState.Idle);
        }

        private void SetState(PlayerState newState)
        {
            if (_state == newState) return;
            _state = newState;
        }
    }
}
