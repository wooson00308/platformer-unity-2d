using UnityEngine;
using Platformer.Core;
using Platformer.Data;

namespace Platformer.Game
{
    /// <summary>
    /// 키네마틱 Rigidbody2D를 왕복 이동시키고, 올라탄 플레이어에게 속도를 넘긴다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour, IMovingGround
    {
        [SerializeField] private MovingPlatformSettings _settings;

        private Rigidbody2D _rb;
        private Vector2 _origin;
        private Vector2 _targetWorld;
        private bool _towardEnd = true;
        private float _pauseRemaining;
        private Vector2 _computedVelocity;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _origin = _rb.position;
            RebuildEndpoints();
        }

        void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            var prev = _rb.position;

            if (_settings == null)
            {
                _computedVelocity = Vector2.zero;
                return;
            }

            if (_pauseRemaining > 0f)
            {
                _pauseRemaining -= dt;
                _computedVelocity = Vector2.zero;
                return;
            }

            var dest = _towardEnd ? _targetWorld : _origin;
            var next = Vector2.MoveTowards(prev, dest, _settings.moveSpeed * dt);

            if ((next - dest).sqrMagnitude < 0.0001f)
            {
                next = dest;
                _towardEnd = !_towardEnd;
                _pauseRemaining = Mathf.Max(0f, _settings.pauseAtEndpoints);
            }

            _rb.MovePosition(next);
            _computedVelocity = dt > 0f ? (next - prev) / dt : Vector2.zero;
        }

        private void RebuildEndpoints()
        {
            if (_settings == null)
            {
                _targetWorld = _origin;
                return;
            }

            var dir = _settings.localTravelDirection;
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector2.right;
            else
                dir.Normalize();

            var worldDelta = (Vector2)transform.TransformDirection(new Vector3(dir.x, dir.y, 0f));
            _targetWorld = _origin + worldDelta * Mathf.Max(0f, _settings.travelDistance);
        }

        public Vector2 GetPointVelocity(Vector2 _)
        {
            if (_rb == null)
                return Vector2.zero;

            return _computedVelocity;
        }
    }
}
