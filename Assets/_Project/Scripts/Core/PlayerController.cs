using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Platformer.Data;

namespace Platformer.Core
{
    public class PlayerController : MonoBehaviour, IDamageable
    {
        #region 변수
        [SerializeField] private RespawnData _respawnData;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private int _maxAirJumps = 1;
        [SerializeField] private float _fallGravityMultiplier = 2.5f;
        [SerializeField] private float _fallDeathThreshold = -10f;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private int _groundOverlapCapacity = 8;
        [SerializeField] private float _deathReloadDelay = 1f;
        [SerializeField] private string _deathTriggerName = "Die";

        private Rigidbody2D _rb;
        private Animator _animator;
        private InputSystem_Actions _input;
        private PlayerState _state = PlayerState.Idle;
        private float _moveInput;
        private bool _isJumpRequested;
        private bool _isAirJumpPending;
        private int _airJumpsRemaining;
        private float _defaultGravityScale;
        private bool _isDead;
        private Collider2D[] _groundOverlapResults;
        #endregion

        #region 유니티 라이프사이클
        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _input = new InputSystem_Actions();
            _defaultGravityScale = _rb.gravityScale;
            _groundOverlapResults = new Collider2D[Mathf.Max(1, _groundOverlapCapacity)];
        }

        void OnEnable()
        {
            _input.Player.Enable();
        }

        void Start()
        {
            if (_respawnData != null && _respawnData.hasCheckpoint)
                transform.position = _respawnData.position;
        }

        void Update()
        {
            if (_isDead)
                return;

            if (transform.position.y < _fallDeathThreshold)
            {
                _Die();
                return;
            }

            var raw = _input.Player.Move.ReadValue<Vector2>();
            _moveInput = raw.x != 0f ? Mathf.Sign(raw.x) : 0f;

            if (_IsGrounded())
                _airJumpsRemaining = _maxAirJumps;

            if (_input.Player.Jump.WasPressedThisFrame())
            {
                if (_IsGrounded())
                {
                    _isJumpRequested = true;
                    _isAirJumpPending = false;
                }
                else if (_airJumpsRemaining > 0)
                {
                    _isJumpRequested = true;
                    _isAirJumpPending = true;
                }
            }

            _UpdateState();
            _UpdateFacing();
        }

        void FixedUpdate()
        {
            if (_isDead)
                return;

            var isGrounded = _IsGrounded();
            var platformVel = Vector2.zero;
            if (isGrounded && _TryGetMovingGroundVelocity(out var pv))
                platformVel = pv;

            if (_isJumpRequested)
            {
                _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed + platformVel.x, 0f);
                _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _isJumpRequested = false;
                if (_isAirJumpPending)
                {
                    _airJumpsRemaining--;
                    _isAirJumpPending = false;
                }
            }
            else if (isGrounded && platformVel.sqrMagnitude > 0f)
            {
                _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed + platformVel.x, platformVel.y);
            }
            else
            {
                _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed, _rb.linearVelocity.y);
            }

            _rb.gravityScale = _rb.linearVelocity.y < 0f
                ? _defaultGravityScale * _fallGravityMultiplier
                : _defaultGravityScale;
        }

        void OnDisable()
        {
            _input.Player.Disable();
        }
        #endregion

        #region Public 메서드
        public void TakeDamage(int amount)
        {
            if (_isDead || amount <= 0)
                return;

            _Die();
        }
        #endregion

        #region Private 메서드
        private bool _IsGrounded()
        {
            if (_groundCheck == null)
                return false;
            return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        private bool _TryGetMovingGroundVelocity(out Vector2 velocity)
        {
            velocity = Vector2.zero;
            if (_groundCheck == null || _groundOverlapResults == null)
                return false;

            var n = Physics2D.OverlapCircleNonAlloc(
                _groundCheck.position,
                _groundCheckRadius,
                _groundOverlapResults,
                _groundLayer);

            for (var i = 0; i < n; i++)
            {
                var col = _groundOverlapResults[i];
                if (col == null)
                    continue;

                var rb = col.attachedRigidbody;
                if (rb == null)
                    rb = col.GetComponentInParent<Rigidbody2D>();
                if (rb == null)
                    continue;

                if (rb.TryGetComponent<IMovingGround>(out var moving))
                {
                    velocity = moving.GetPointVelocity(_groundCheck.position);
                    return true;
                }
            }

            return false;
        }

        private void _UpdateState()
        {
            if (!_IsGrounded())
            {
                _SetState(_rb.linearVelocity.y > 0f ? PlayerState.Jumping : PlayerState.Falling);
                return;
            }
            _SetState(Mathf.Abs(_moveInput) > 0.01f ? PlayerState.Running : PlayerState.Idle);
        }

        private void _SetState(PlayerState newState)
        {
            if (_state == newState) return;
            _state = newState;
            _UpdateAnimator();
        }

        private void _UpdateAnimator()
        {
            if (_animator == null) return;
            _animator.SetFloat("Speed", Mathf.Abs(_moveInput));
            _animator.SetBool("IsGrounded", _IsGrounded());
        }

        private void _UpdateFacing()
        {
            if (Mathf.Abs(_moveInput) < 0.01f) return;
            var scale = transform.localScale;
            scale.x = _moveInput > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        private void _Die()
        {
            _isDead = true;
            _moveInput = 0f;
            _isJumpRequested = false;
            _isAirJumpPending = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezePositionX
                | RigidbodyConstraints2D.FreezePositionY
                | RigidbodyConstraints2D.FreezeRotation;
            _SetState(PlayerState.Dead);

            if (_animator != null)
            {
                _animator.SetBool("IsGrounded", true);
                _animator.SetFloat("Speed", 0f);

                if (_HasAnimatorParameter(_deathTriggerName, AnimatorControllerParameterType.Trigger))
                    _animator.SetTrigger(_deathTriggerName);

                var deathStateHash = Animator.StringToHash("Death");
                if (_animator.HasState(0, deathStateHash))
                    _animator.Play(deathStateHash, 0, 0f);
            }

            Invoke(nameof(_ReloadCurrentScene), _deathReloadDelay);
        }

        private void _ReloadCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private bool _HasAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType)
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
        #endregion
    }
}
