using PurrNet.Packing;

namespace PurrNet.Modules
{
    internal struct DeltaAcknowledge : IPackedAuto
    {
        public PackedUInt key;
        public PackedUInt valueId;
    }

    internal struct DeltaCleanup : IPackedAuto
    {
        public PackedUInt upToId;
    }

    public interface IStableHashable
    {
        uint GetStableHash();
    }
}
