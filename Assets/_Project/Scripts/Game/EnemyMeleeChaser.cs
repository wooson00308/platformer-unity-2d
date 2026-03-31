using Platformer.Core;
using UnityEngine;

namespace Platformer.Game
{
    public class EnemyMeleeChaser : MonoBehaviour, IDamageable
    {
        #region 상수
        private const string IS_CHASING_ANIM_PARAM = "IsChasing";
        #endregion

        #region 변수
        [SerializeField] private Transform _target;
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _detectRadius = 5f;
        [SerializeField] private float _stopDistanceX = 0.2f;
        [SerializeField] private int _contactDamage = 1;
        [SerializeField] private int _maxHealth = 1;
        [SerializeField] private float _stompVelocityThreshold = -0.1f;
        [SerializeField] private float _stompContactTolerance = 0.2f;
        [SerializeField] private float _stompBounceImpulse = 5f;
        [Tooltip("스프라이트 기본이 오른쪽을 보면 true, 왼쪽을 보면 false(대부분 팩은 false).")]
        [SerializeField] private bool _isFacingRightByDefault;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private int _currentHealth;
        private bool _isDead;
        #endregion

        #region 유니티 라이프사이클
        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _currentHealth = _maxHealth;

            var s = transform.localScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }

        void FixedUpdate()
        {
            if (_isDead)
                return;

            if (_target == null)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            var toTarget = _target.position - transform.position;
            var isInRange = toTarget.sqrMagnitude <= _detectRadius * _detectRadius;
            if (!isInRange)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                _UpdateFacingAndAnim(0f, false);
                return;
            }

            var absDistanceX = Mathf.Abs(toTarget.x);
            if (absDistanceX <= _stopDistanceX)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                _UpdateFacingAndAnim(0f, false);
                return;
            }

            var directionX = Mathf.Sign(toTarget.x);
            _rb.linearVelocity = new Vector2(directionX * _moveSpeed, _rb.linearVelocity.y);

            _UpdateFacingAndAnim(directionX, true);
        }
        #endregion

        #region Public 메서드
        public void TakeDamage(int amount)
        {
            if (_isDead || amount <= 0)
                return;

            _currentHealth -= amount;
            if (_currentHealth <= 0)
                _Die();
        }

        public bool TryStompByPlayerCollider(Collider2D playerCollider)
        {
            if (_isDead || playerCollider == null)
                return false;

            if (!playerCollider.TryGetComponent<PlayerController>(out _))
                return false;

            var playerRb = playerCollider.attachedRigidbody;
            var isFalling = playerRb != null && playerRb.linearVelocity.y <= _stompVelocityThreshold;
            if (!isFalling)
                return false;

            if (playerRb != null)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
                playerRb.AddForce(Vector2.up * _stompBounceImpulse, ForceMode2D.Impulse);
            }

            _Die();
            return true;
        }
        #endregion

        #region Private 메서드
        private void _UpdateFacingAndAnim(float directionX, bool isChasing)
        {
            if (_spriteRenderer != null)
            {
                if (Mathf.Abs(directionX) > 0.01f)
                    _spriteRenderer.flipX = _isFacingRightByDefault ? directionX < 0f : directionX > 0f;
            }

            if (_animator != null && _HasAnimatorParameter(IS_CHASING_ANIM_PARAM, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IS_CHASING_ANIM_PARAM, isChasing && Mathf.Abs(directionX) > 0.01f);
        }

        private bool _HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return false;

            foreach (var p in _animator.parameters)
            {
                if (p.type == type && p.name == parameterName)
                    return true;
            }

            return false;
        }

        private void _ResolveCollision(Collision2D collision)
        {
            var otherCollider = collision.collider;
            if (_isDead || otherCollider == null)
                return;

            if (_TryStompedByPlayer(collision))
                return;

            if (otherCollider.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(_contactDamage);
        }

        private bool _TryStompedByPlayer(Collision2D collision)
        {
            var otherCollider = collision.collider;
            if (!otherCollider.TryGetComponent<PlayerController>(out _))
                return false;

            var otherRigidbody = otherCollider.attachedRigidbody;
            var isFalling = otherRigidbody != null && otherRigidbody.linearVelocity.y <= _stompVelocityThreshold;
            if (!isFalling || _collider == null)
                return false;

            var isAbove = _IsStompContact(collision, otherCollider);
            if (!isAbove)
                return false;

            if (otherRigidbody != null)
            {
                otherRigidbody.linearVelocity = new Vector2(otherRigidbody.linearVelocity.x, 0f);
                otherRigidbody.AddForce(Vector2.up * _stompBounceImpulse, ForceMode2D.Impulse);
            }

            _Die();
            return true;
        }

        private bool _IsStompContact(Collision2D collision, Collider2D otherCollider)
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y >= 0.5f)
                    return true;
            }

            var playerBottom = otherCollider.bounds.min.y;
            var enemyTop = _collider.bounds.max.y;
            return playerBottom >= enemyTop - _stompContactTolerance;
        }

        private void _Die()
        {
            _isDead = true;

            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }
        #endregion

        #region 엔진 콜백
        private void OnCollisionEnter2D(Collision2D collision)
        {
            _ResolveCollision(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            _ResolveCollision(collision);
        }
        #endregion
    }
}
