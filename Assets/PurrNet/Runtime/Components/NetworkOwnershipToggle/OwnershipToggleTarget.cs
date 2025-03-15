using UnityEngine;

namespace PurrNet
{
    [System.Serializable]
    public struct OwnershipToggleTarget
    {
        public Component target;
        public bool activeAsOwner;
    }
}