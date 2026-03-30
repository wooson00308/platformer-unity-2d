using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Platformer.Data;

namespace Platformer.Core
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private float _fallGravityMultiplier = 2.5f;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _deathReloadDelay = 1f;
        [SerializeField] private string _deathTriggerName = "Die";

        private Rigidbody2D _rb;
        private Animator _animator;
        private InputSystem_Actions _input;
        private PlayerState _state = PlayerState.Idle;
        private float _moveInput;
        private bool _jumpRequested;
        private float _defaultGravityScale;
        private bool _isDead;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
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
            if (_isDead)
                return;

            var raw = _input.Player.Move.ReadValue<Vector2>();
            _moveInput = raw.x != 0f ? Mathf.Sign(raw.x) : 0f;

            if (_input.Player.Jump.WasPressedThisFrame() && IsGrounded())
                _jumpRequested = true;

            UpdateState();
            UpdateFacing();
        }

        void FixedUpdate()
        {
            if (_isDead)
                return;

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
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;
            _animator.SetFloat("Speed", Mathf.Abs(_moveInput));
            _animator.SetBool("IsGrounded", IsGrounded());
        }

        private void UpdateFacing()
        {
            if (Mathf.Abs(_moveInput) < 0.01f) return;
            var scale = transform.localScale;
            scale.x = _moveInput > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || amount <= 0)
                return;

            Die();
        }

        private void Die()
        {
            _isDead = true;
            _moveInput = 0f;
            _jumpRequested = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX
                | RigidbodyConstraints2D.FreezePositionY
                | RigidbodyConstraints2D.FreezeRotation;
            SetState(PlayerState.Dead);

            if (_animator != null)
            {
                if (HasAnimatorParameter(_deathTriggerName, AnimatorControllerParameterType.Trigger))
                    _animator.SetTrigger(_deathTriggerName);

                var deathStateHash = Animator.StringToHash("Death");
                if (_animator.HasState(0, deathStateHash))
                    _animator.Play(deathStateHash, 0, 0f);
            }

            Invoke(nameof(ReloadCurrentScene), _deathReloadDelay);
        }

        private void ReloadCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return false;

            foreach (var parameter in _animator.parameters)
            {
                if (parameter.type == parameterType && parameter.name == parameterName)
                    return true;
            }

            return false;
        }
    }
}
