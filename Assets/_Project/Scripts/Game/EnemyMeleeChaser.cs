using Platformer.Core;
using UnityEngine;

namespace Platformer.Game
{
    public class EnemyMeleeChaser : MonoBehaviour, IDamageable
    {
        private const string IsChasingAnimatorParam = "IsChasing";
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
        [SerializeField] private bool _spriteFacesRightByDefault;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private int _currentHealth;
        private bool _isDead;

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

        void OnEnable()
        {
        }

        void Start()
        {
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
            var shouldChase = toTarget.sqrMagnitude <= _detectRadius * _detectRadius;
            if (!shouldChase)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                UpdateFacingAndAnim(0f, false);
                return;
            }

            var absDistanceX = Mathf.Abs(toTarget.x);
            if (absDistanceX <= _stopDistanceX)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                UpdateFacingAndAnim(0f, false);
                return;
            }

            var directionX = Mathf.Sign(toTarget.x);
            _rb.linearVelocity = new Vector2(directionX * _moveSpeed, _rb.linearVelocity.y);

            UpdateFacingAndAnim(directionX, true);
        }

        private void UpdateFacingAndAnim(float directionX, bool isChasing)
        {
            if (_spriteRenderer != null)
            {
                if (Mathf.Abs(directionX) > 0.01f)
                    _spriteRenderer.flipX = _spriteFacesRightByDefault ? directionX < 0f : directionX > 0f;
            }

            if (_animator != null && HasAnimatorParameter(IsChasingAnimatorParam, AnimatorControllerParameterType.Bool))
                _animator.SetBool(IsChasingAnimatorParam, isChasing && Mathf.Abs(directionX) > 0.01f);
        }

        private bool HasAnimatorParameter(string parameterName, AnimatorControllerParameterType type)
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

        void OnDisable()
        {
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ResolveCollision(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            ResolveCollision(collision);
        }

        public void TakeDamage(int amount)
        {
            if (_isDead || amount <= 0)
                return;

            _currentHealth -= amount;
            if (_currentHealth <= 0)
                Die();
        }

        private void ResolveCollision(Collision2D collision)
        {
            var otherCollider = collision.collider;
            if (_isDead || otherCollider == null)
                return;

            if (TryStompedByPlayer(collision))
                return;

            if (otherCollider.TryGetComponent<IDamageable>(out var damageable))
                damageable.TakeDamage(_contactDamage);
        }

        private bool TryStompedByPlayer(Collision2D collision)
        {
            var otherCollider = collision.collider;
            if (!otherCollider.TryGetComponent<Platformer.Core.PlayerController>(out _))
                return false;

            var otherRigidbody = otherCollider.attachedRigidbody;
            var isFalling = otherRigidbody != null && otherRigidbody.linearVelocity.y <= _stompVelocityThreshold;
            if (!isFalling || _collider == null)
                return false;

            var isAbove = IsStompContact(collision, otherCollider);
            if (!isAbove)
                return false;

            if (otherRigidbody != null)
            {
                otherRigidbody.linearVelocity = new Vector2(otherRigidbody.linearVelocity.x, 0f);
                otherRigidbody.AddForce(Vector2.up * _stompBounceImpulse, ForceMode2D.Impulse);
            }

            Die();
            return true;
        }

        public bool TryStompByPlayerCollider(Collider2D playerCollider)
        {
            if (_isDead || playerCollider == null)
                return false;

            if (!playerCollider.TryGetComponent<Platformer.Core.PlayerController>(out _))
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

            Die();
            return true;
        }

        private bool IsStompContact(Collision2D collision, Collider2D otherCollider)
        {
            // Enemy 기준 충돌 노멀이 위(+Y)를 가리키면 플레이어가 위에서 밟은 접촉으로 본다.
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y >= 0.5f)
                    return true;
            }

            // 노멀이 불안정한 프레임 대비: 플레이어 하단이 적 상단 근처 이상이면 위 접촉으로 허용.
            var playerBottom = otherCollider.bounds.min.y;
            var enemyTop = _collider.bounds.max.y;
            return playerBottom >= enemyTop - _stompContactTolerance;
        }

        private void Die()
        {
            _isDead = true;

            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }
    }
}
