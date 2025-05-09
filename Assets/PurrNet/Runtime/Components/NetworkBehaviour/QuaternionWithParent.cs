using UnityEngine;

namespace PurrNet
{
    public struct QuaternionWithParent
    {
        readonly NetworkIdentity parent;
        public readonly Quaternion rotation;
        readonly bool isLocalPos;

        public QuaternionWithParent(NetworkIdentity parent, bool isLocalPos, Quaternion rotation)
        {
            this.parent = parent;
            this.rotation = rotation;
            this.isLocalPos = isLocalPos;
        }

        public static QuaternionWithParent Lerp(QuaternionWithParent a, QuaternionWithParent b, float t)
        {
            if (!b.isLocalPos)
                return new QuaternionWithParent(default, default, Quaternion.Lerp(a.rotation, b.rotation, t));

            var aWorldRot = a.parent ? a.parent.transform.rotation * a.rotation : a.rotation;
            var bWorldRot = b.parent ? b.parent.transform.rotation * b.rotation : b.rotation;

            var lerpedWorldRot = Quaternion.Lerp(aWorldRot, bWorldRot, t);
            return new QuaternionWithParent(default, default, lerpedWorldRot);
        }

        public static QuaternionWithParent NoLerp(QuaternionWithParent a, QuaternionWithParent b, float t)
        {
            if (!b.isLocalPos)
                return new QuaternionWithParent(default, default, b.rotation);

            var bWorldRot = b.parent ? b.parent.transform.rotation * b.rotation : b.rotation;
            return new QuaternionWithParent(default, default, bWorldRot);
        }
    }
}
