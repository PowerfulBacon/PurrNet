using System;
using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Pooling;

namespace PurrNet.Modules
{
    public readonly struct SpawnPacketBatch : IPackedAuto, IDisposable
    {
        public readonly List<SpawnPacket> spawnPackets;

        public SpawnPacketBatch(List<SpawnPacket> spawnPackets)
        {
            this.spawnPackets = spawnPackets;
        }

        public void Dispose()
        {
            int c = spawnPackets.Count;
            for (var i = 0; i < c; ++i)
                spawnPackets[i].Dispose();

            ListPool<SpawnPacket>.Destroy(spawnPackets);
        }

        public override string ToString()
        {
            return $"SpawnPacketBatch: {{ spawnPackets: {spawnPackets.Count} }}";
        }
    }

    public struct SpawnPacket : IPackedAuto, IDisposable
    {
        public SceneID sceneId;
        public SpawnID packetIdx;
        public GameObjectPrototype prototype;

        public override string ToString()
        {
            return $"SpawnPacket: {{ sceneId: {sceneId}, packetIdx: {packetIdx}, prototype: {prototype} }}";
        }

        public void Dispose()
        {
            prototype.Dispose();
        }
    }
}
