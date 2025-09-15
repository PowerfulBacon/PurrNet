
namespace PurrNet
{

    internal enum PackerModuleFlags : byte
    {
        NotSpawned = 0,
        IsSpawned = 1 << 0,
        IsDynamic = 1 << 1,
    }

}
