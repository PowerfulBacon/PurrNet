using PurrNet.Packing;

namespace PurrNet.Modules
{
    internal struct DeltaAcknowledge : IPackedAuto
    {
        public int key;
        public PackedUInt valueId;
    }
}