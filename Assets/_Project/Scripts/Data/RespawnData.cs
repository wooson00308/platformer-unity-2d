using UnityEngine;

namespace Platformer.Data
{
    [CreateAssetMenu(fileName = "RespawnData", menuName = "Platformer/Data/RespawnData")]
    public class RespawnData : ScriptableObject
    {
        public bool hasCheckpoint;
        public Vector2 position;

        public void SetCheckpoint(Vector2 pos)
        {
            hasCheckpoint = true;
            position = pos;
        }

        public void Reset()
        {
            hasCheckpoint = false;
            position = Vector2.zero;
        }
    }
}
