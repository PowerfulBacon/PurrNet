using UnityEngine;

namespace PurrNet
{
    public struct ScaleWithParent
    {
        readonly NetworkIdentity parent;
        public readonly Vector3 scale;

        public ScaleWithParent(NetworkIdentity parent, Vector3 scale)
        {
            this.parent = parent;
            this.scale = scale;
        }

        public static ScaleWithParent Lerp(ScaleWithParent a, ScaleWithParent b, float t)
        {
            var aTrs = a.parent ? a.parent.transform : default;
            var bTrs = b.parent ? b.parent.transform : default;

            var aWorldScale = aTrs ? aTrs.GetWorldScale(a.scale) : a.scale;
            var bWorldScale = bTrs ? bTrs.GetWorldScale(b.scale) : b.scale;

            return new ScaleWithParent(null, Vector3.Lerp(aWorldScale, bWorldScale, t));
        }

        public static ScaleWithParent NoLerp(ScaleWithParent a, ScaleWithParent b, float t)
        {
            if (!b.parent)
                return new ScaleWithParent(default, b.scale);
            var worldScale = b.parent ? b.parent.transform.GetWorldScale(b.scale) : b.scale;
            return new ScaleWithParent(default, worldScale);
        }
    }
}
