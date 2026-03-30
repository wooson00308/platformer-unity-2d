using UnityEngine;
using UnityEngine.Events;

namespace Platformer.Data
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "Platformer/Events/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private event UnityAction _onRaised;

        public void Raise() => _onRaised?.Invoke();

        public void AddListener(UnityAction listener)    => _onRaised += listener;
        public void RemoveListener(UnityAction listener) => _onRaised -= listener;
    }
}
