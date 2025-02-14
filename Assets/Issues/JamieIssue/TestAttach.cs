using PurrNet;
using UnityEngine;

public class TestAttach : NetworkBehaviour
{
    private Rigidbody _rb = null;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
    {
        base.OnOwnerChanged(oldOwner, newOwner, asServer);

        if (newOwner == null)
        {
            _rb.isKinematic = false;
            _rb.detectCollisions = true;

            if (isController)
            {
                transform.SetParent(null);
            }
        }
        else
        {
            _rb.isKinematic = true;
            _rb.detectCollisions = false;

            if (isController)
            {
                PlayerController player = PlayerController.Get(newOwner.Value);
                Transform rHand = player.transform.Find("AttachPoint");

                transform.SetParent(rHand);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }
        }
    }

    [ContextMenu("Set As Owner")]
    private void SetAsOwnerContext()
    {
        ServerSetPlayerIDAsOwner(localPlayer);
    }

    [ContextMenu("Remove Ownership")]
    private void RemoveOwnershipContext()
    {
        ServerSetPlayerIDAsOwner(null);
    }

    [ServerRpc(requireOwnership: false)]
    private void ServerSetPlayerIDAsOwner(PlayerID? playerID)
    {
        if (playerID == null)
        {
            RemoveOwnership();
        }
        else
        {
            GiveOwnership(playerID);
        }

    }
}
