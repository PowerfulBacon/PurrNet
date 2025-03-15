using JetBrains.Annotations;
using UnityEngine;

namespace PurrNet
{
    public sealed class NetworkOwnershipToggle : NetworkIdentity
    {
        [SerializeField] private OwnershipToggleTarget[] _toggleTargets;

        private bool _lastOwner;

        private void Awake()
        {
            Setup(false);
        }

        [UsedImplicitly]
        public void Setup(bool asOwner)
        {
            _lastOwner = asOwner;

            for (var i = 0; i < _toggleTargets.Length; i++)
            {
                var target = _toggleTargets[i].target;
                if (!target) continue;

                bool targetState = _toggleTargets[i].activeAsOwner == asOwner;
                SetComponentState(target, targetState);
            }
        }

        private void SetComponentState(Component target, bool targetState)
        {
            if (target is Transform go)
            {
                go.gameObject.SetActive(targetState);
            }
            else if (target is Behaviour behaviour)
            {
                behaviour.enabled = targetState;
            }
            else if (target is Collider collider)
            {
                collider.enabled = targetState;
            }
            else if (target is Collider2D collider2D)
            {
                collider2D.enabled = targetState;
            }
            else if (target is Renderer renderer)
            {
                renderer.enabled = targetState;
            }
        }

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            if (isOwner != _lastOwner)
                Setup(isOwner);
        }
    }
}