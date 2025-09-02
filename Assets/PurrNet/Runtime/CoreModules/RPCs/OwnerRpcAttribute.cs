using PurrNet.Modules;
using PurrNet.Transports;
using UnityEngine.Scripting;

namespace PurrNet
{
    /// <summary>
    /// Creates an RPC that executes only on the owner of the
    /// identity.
    /// If the identity is owned by the server, executes on the
    /// server.
    /// </summary>
    public class OwnerRpcAttribute : PreserveAttribute
    {
        /// <summary>
        /// Creates the OwnerRpc
        /// </summary>
        /// <param name="channel">
        /// The method in which the RPC's packet should be sent via
        /// the network.
        /// </param>
        /// <param name="runLocally">
        /// If set to true, then the caller will run the RPC on top
        /// of raising the RPC like normal.
        /// If the local instance is  owner of the identity, then
        /// it will be executed locally regardless of this value.
        /// If the local instance is the owner of the identity,
        /// and this value is set, then the RPC is only executed
        /// once.
        /// </param>
        /// <param name="requireServer">
        /// Only the server is allowed to raise this RPC, when this
        /// is specified. When set to false, any client in the game
        /// will be able to raise this RPC to be executed on the
        /// client who owns the identity.
        /// </param>
        /// <param name="compressionLevel">
        /// Compression level of the data sent in the call.
        /// </param>
        /// <param name="asyncTimeoutInSec">
        /// When an async RPC is used, how long should the system
        /// wait before considering the result to have timed out
        /// if a response was not recieved.
        /// </param>
        [UsedByIL]
        public OwnerRpcAttribute(
            Channel channel = Channel.ReliableOrdered,
            bool runLocally = false,
            bool requireServer = true,
            CompressionLevel compressionLevel = CompressionLevel.None,
            float asyncTimeoutInSec = 5f)
        {
        }
    }
}