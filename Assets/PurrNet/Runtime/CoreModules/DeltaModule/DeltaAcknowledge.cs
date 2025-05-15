using PurrNet.Packing;

namespace PurrNet.Modules
{
    internal struct DeltaAcknowledge : IPackedAuto
    {
        public uint key;
        public PackedUInt valueId;
    }
    
    public interface IStableHashable
    {
        uint GetStableHash();
    }
}