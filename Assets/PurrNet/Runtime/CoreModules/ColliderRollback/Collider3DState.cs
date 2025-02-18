using UnityEngine;

namespace PurrNet.Modules
{
    public struct Collider3DState
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public bool enabled;

        public Collider3DState(Collider collider)
        {
            var trs = collider.transform;
            position = trs.position;
            rotation = trs.rotation;
            scale = trs.localScale;
            enabled = collider.enabled;
        }
    }
}