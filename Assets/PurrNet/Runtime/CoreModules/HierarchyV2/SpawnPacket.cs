using System;
using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Pooling;

namespace PurrNet.Modules
{
    public readonly struct SpawnPacketBatch : IPackedAuto, IDisposable
    {
        public readonly List<SpawnPacket> spawnPackets;
        public readonly List<DespawnPacket> despawnPackets;

        public SpawnPacketBatch(List<SpawnPacket> spawnPackets, List<DespawnPacket> despawnPackets)
        {
            this.despawnPackets = despawnPackets;
            this.spawnPackets = spawnPackets;
        }
        public void Dispose()
        {
            int c = spawnPackets.Count;
            for (var i = 0; i < c; ++i)
                spawnPackets[i].Dispose();

            ListPool<SpawnPacket>.Destroy(spawnPackets);
            ListPool<DespawnPacket>.Destroy(despawnPackets);
        }

        public override string ToString()
        {
            return $"SpawnPacketBatch: {{ spawnPackets: {spawnPackets.Count} }}";
        }
    }

    public struct SpawnPacket : IPackedSimple, IDisposable
    {
        public SceneID sceneId;
        public SpawnID packetIdx;
        public GameObjectPrototype prototype;

        public List<NetworkIdentity> localcache;

        public override string ToString()
        {
            return $"SpawnPacket: {{ sceneId: {sceneId}, packetIdx: {packetIdx}, prototype: {prototype} }}";
        }

        public void Serialize(BitPacker packer)
        {
            Packer<SceneID>.Serialize(packer, ref sceneId);
            Packer<SpawnID>.Serialize(packer, ref packetIdx);
            Packer<GameObjectPrototype>.Serialize(packer, ref prototype);
        }

        public void Dispose()
        {
            prototype.Dispose();
            if (localcache != null)
            {
                ListPool<NetworkIdentity>.Destroy(localcache);
                localcache = null;
            }
        }
    }
}
