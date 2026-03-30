using UnityEngine;
using UnityEngine.Events;

namespace Platformer.Data
{
    /// <summary>
    /// Inspector에서 GameEvent → UnityEvent 연결할 때 사용하는 어댑터 컴포넌트.
    /// 코드에서 직접 구독할 때는 GameEvent.AddListener/RemoveListener 사용.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent _event;
        [SerializeField] private UnityEvent _response;

        void OnEnable()  => _event.AddListener(_response.Invoke);
        void OnDisable() => _event.RemoveListener(_response.Invoke);
    }
}
