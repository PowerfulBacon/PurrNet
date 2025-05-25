using PurrNet.Packing;

namespace PurrNet.Modules
{
    internal struct DeltaAcknowledge : IPackedAuto
    {
        public PackedUInt key;
        public PackedUInt valueId;
    }

    public interface IStableHashable
    {
        uint GetStableHash();
    }
}
