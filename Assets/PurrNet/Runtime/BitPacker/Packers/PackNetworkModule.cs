using PurrNet.Modules;
using PurrNet.Packing;
using System;

namespace PurrNet
{
    public static class PackNetworkModule
    {
        [UsedByIL]
        public static void RegisterNetworkModule<T>() where T : NetworkModule
        {
            Packer<T>.RegisterWriter(WriteModule);
            Packer<T>.RegisterReader(ReadModule);
        }

        [UsedByIL]
        public static void WriteModule<T>(this BitPacker packer, T module) where T : NetworkModule
        {
            if (module is not { isSpawned: true } || !module.parent)
            {
                Packer<byte>.Write(packer, (byte)PackerModuleFlags.NotSpawned);
                return;
            }
            PackerModuleFlags flags = PackerModuleFlags.IsSpawned;
            // Pack the dynamic flag
            if (module.isDynamic)
            {
                flags |= PackerModuleFlags.IsDynamic;
            }
            // Send the stuff
            Packer<byte>.Write(packer, (byte)flags);
            Packer<byte>.Write(packer, module.index);

            Packer<NetworkIdentity>.Write(packer, module.parent);
        }

        [UsedByIL]
        public static void ReadModule<T>(this BitPacker packer, ref T module) where T : NetworkModule
        {
            PackerModuleFlags flags = PackerModuleFlags.NotSpawned;

            byte flagByte = 0;
            Packer<byte>.Read(packer, ref flagByte);
            flags = (PackerModuleFlags)flagByte;

            // Module is not spawned
            if ((flags & PackerModuleFlags.IsSpawned) == 0)
            {
                module = null;
                return;
            }

            bool isDynamic = (flags & PackerModuleFlags.IsDynamic) != 0;

            byte index = 0;
            Packer<byte>.Read(packer, ref index);

            NetworkIdentity identity = null;
            Packer<NetworkIdentity>.Read(packer, ref identity);


            if (identity && identity.TryGetModule(index, out NetworkModule nmodule) && nmodule is T result)
            {
                module = result;
                return;
            }
            // No module found
            module = null;
            // Do we need to create it?
            if (isDynamic)
            {
                // Create a new module at the index that was requested.
                T instantiated = (T)Activator.CreateInstance(typeof(T), true);
                instantiated.LateInitialize(identity, null, index);
                module = instantiated;
                return;
            }
        }

        [UsedByIL]
        public static void WriteIdentityConcrete(this BitPacker packer, NetworkModule module) =>
            WriteModule(packer, module);

        [UsedByIL]
        public static void ReadIdentityConcrete(this BitPacker packer, ref NetworkModule module) =>
            ReadModule(packer, ref module);
    }
}