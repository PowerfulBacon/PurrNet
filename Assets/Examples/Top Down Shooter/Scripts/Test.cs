using PurrNet;
using PurrNet.Packing;
using PurrNet.Transports;
using UnityEngine;

public class Test : NetworkIdentity
{
    [SerializeField] private CompressedVector3 _testData;

    private void Update()
    {
        if (!isController)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
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
            deltaModule.Write(packer, player, 123, testData);
            ReceiveData(player, packer);
        }
    }

    [TargetRpc(requireServer:false, channel:Channel.Unreliable)]
    private void ReceiveData(PlayerID target, BitPacker packer, RPCInfo info = default)
    {
        CompressedVector3 receivedData = default;
        PackedUInt valueId = default;

        if (!networkManager.TryGetModule(out DeltaModule deltaModule, false))
            return;

        deltaModule.Read(packer, localPlayerForced, 123, ref receivedData, ref valueId);

        packer.Dispose();
    }
}
