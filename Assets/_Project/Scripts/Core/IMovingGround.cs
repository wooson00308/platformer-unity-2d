using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// 플레이어가 올라탔을 때 Rigidbody2D 속도에 합쳐 줄 지면(이동 플랫폼 등).
    /// </summary>
    public interface IMovingGround
    {
        Vector2 GetPointVelocity(Vector2 worldPoint);
    }
}
