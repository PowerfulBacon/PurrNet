#if UNITY_PHYSICS_3D

using UnityEngine;

namespace PurrNet.Examples.TopDownShooter
{
    public class PlayerShoot : NetworkIdentity
    {
        [SerializeField] private Bullet bulletPrefab;

        protected override void OnSpawned(bool asServer)
        {
            enabled = isOwner;
        }

        protected override void OnSpawned()
        {
            Debug.Log($"New Player Spawned for player: {owner}");
        }
        
        
        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            if (!asServer)
                return;
            
            Debug.Log($"Ownership changed: {newOwner}");
        }

        protected override void OnOwnerDisconnected(PlayerID ownerId)
        {
            Debug.Log($"Owner disconnected: {ownerId}");
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            var trs = transform;

            UnityProxy.Instantiate(bulletPrefab, trs.position + trs.forward * 0.5f + Vector3.up * 0.7f, trs.rotation);
        }
    }
}

#endif
