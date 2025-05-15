using PurrNet;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Transports;
using UnityEngine;

public class Test : NetworkIdentity
{
    [SerializeField] private int _packKey = 123;
    [SerializeField] private CompressedVector3 _testData;

    private void Update()
    {
        if (!isController)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            SendData(_testData);
    }

    [ServerRpc(Channel.Unreliable)]
    private void SendData(CompressedVector3 testData)
    {
        foreach (var player in networkManager.players)
        {
            if (!networkManager.TryGetModule(out DeltaModule deltaModule, isServer))
                continue;

            using var packer = BitPackerPool.Get();
            var key = new TestKey() { key = _packKey };
            deltaModule.Write(packer, player, key, testData);
            ReceiveData(player, packer);
        }
    }

    [TargetRpc(requireServer:false, channel:Channel.Unreliable)]
    private void ReceiveData(PlayerID target, BitPacker packer, RPCInfo info = default)
    {
        CompressedVector3 receivedData = default;

        if (!networkManager.TryGetModule(out DeltaModule deltaModule, false))
        {
            packer.Dispose();
            return;
        }

        var startPos = packer.positionInBits;
        var key = new TestKey() { key = _packKey };
        deltaModule.Read(packer, key, ref receivedData);

        PurrLogger.Log($"Received data ({key.GetType().Name}: {key.ToString()}): {receivedData}, length: {packer.positionInBits - startPos} bits");

        packer.Dispose();
    }

    private struct TestKey : IStableHashable
    {
        public int key;

        public override string ToString()
        {
            return $"{key}";
        }

        public uint GetStableHash()
        {
            return (uint)key;
        }
    }
}
