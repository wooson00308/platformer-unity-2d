using UnityEngine;

namespace Platformer.Data
{
    [CreateAssetMenu(fileName = "MovingPlatformSettings", menuName = "Platformer/Settings/Moving Platform")]
    public class MovingPlatformSettings : ScriptableObject
    {
        [Tooltip("초당 이동 속도 (월드 단위)")]
        public float moveSpeed = 2f;

        [Tooltip("끝점에 도달했을 때 멈추는 시간(초)")]
        public float pauseAtEndpoints = 0.35f;

        [Tooltip("시작 위치 기준, 로컬 공간에서 이동 방향 (정규화 권장)")]
        public Vector2 localTravelDirection = Vector2.right;

        [Tooltip("한쪽 끝까지의 이동 거리")]
        public float travelDistance = 3f;
    }
}
