using UnityEngine;

namespace Platformer.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, -10f);
        [SerializeField] private float _smoothTime = 0.3f;

        private Vector3 _velocity = Vector3.zero;

        void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + _offset;
            transform.position = Vector3.SmoothDamp(
                transform.position, targetPos, ref _velocity, _smoothTime);
        }
    }
}
