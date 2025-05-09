using UnityEngine;

namespace PurrNet
{
    public struct Vector3WithParent
    {
        readonly NetworkIdentity identity;
        public readonly Vector3 position;
        readonly bool isLocalPos;

        public Vector3WithParent(NetworkIdentity identity, bool isLocalPos, Vector3 position)
        {
            this.identity = identity;
            this.position = position;
            this.isLocalPos = isLocalPos;
        }

        public static Vector3WithParent Lerp(Vector3WithParent a, Vector3WithParent b, float t)
        {
            if (!b.isLocalPos)
                return new Vector3WithParent(b.identity, false, Vector3.Lerp(a.position, b.position, t));

            var aWorldPos = a.identity ? a.identity.transform.TransformPoint(a.position) : a.position;
            var bWorldPos = b.identity ? b.identity.transform.TransformPoint(b.position) : b.position;
            var lerpedWorldPos = Vector3.Lerp(aWorldPos, bWorldPos, t);
            return new Vector3WithParent(b.identity, true, lerpedWorldPos);
        }

        public static Vector3WithParent NoLerp(Vector3WithParent a, Vector3WithParent b, float t)
        {
            if (!b.isLocalPos)
                return new Vector3WithParent(b.identity, false, b.position);
            var worldPos = b.identity ? b.identity.transform.TransformPoint(b.position) : b.position;
            return new Vector3WithParent(b.identity, true, worldPos);
        }
    }
}
