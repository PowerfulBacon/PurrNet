using System;
using PurrNet.Packing;
using PurrNet.Pooling;

namespace PurrNet.Modules
{
    public struct SpawnPacketBatch : IPackedAuto, IDisposable
    {
        public DisposableList<SpawnPacket> spawnPackets;
        public DisposableList<DespawnPacket> despawnPackets;

        public SpawnPacketBatch(DisposableList<SpawnPacket> spawnPackets, DisposableList<DespawnPacket> despawnPackets)
        {
            this.despawnPackets = despawnPackets;
            this.spawnPackets = spawnPackets;
        }
        public void Dispose()
        {
            int c = spawnPackets.Count;
            for (var i = 0; i < c; ++i)
                spawnPackets[i].Dispose();

            spawnPackets.Dispose();
            despawnPackets.Dispose();
        }

        public override string ToString()
        {
            return $"SpawnPacketBatch: {{ spawnPackets: {spawnPackets.Count} }}";
        }
    }
}
