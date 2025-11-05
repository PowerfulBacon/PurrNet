using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet
{
    public struct ChildRPCPacket : IPackedAuto, IRpc
    {
        public NetworkID networkId;
        public SceneID sceneId;
        public PlayerID senderId;
        public PlayerID? targetId;
        public byte rpcId;
        public byte childId;
        public ByteData data;

        public ByteData rpcData
        {
            get { return data; }
            set { data = value; }
        }

        public PlayerID senderPlayerId => senderId;

        public PlayerID targetPlayerId
        {
            get => targetId ?? default;
            set => targetId = value;
        }
    }
}