using UnityEngine;

namespace PurrNet.Modules
{
    public struct Collider2DState
    {
        public Vector2 position;
        public float rotation;
        public Vector2 scale;
        public bool enabled;

        public Collider2DState(Collider2D collider)
        {
            var trs = collider.transform;
            position = trs.position;
            rotation = trs.eulerAngles.z;
            scale = trs.localScale;
            enabled = collider.enabled;
        }
    }
}