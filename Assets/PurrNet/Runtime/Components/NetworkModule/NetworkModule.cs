using JetBrains.Annotations;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Profiler;
using PurrNet.Transports;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PurrNet
{
    public class NetworkModule
    {
        public NetworkIdentity parent { get; private set; }

        [UsedImplicitly] public string name { get; private set; }

        [UsedImplicitly] public byte index { get; private set; } = 255;

        [UsedImplicitly] public NetworkManager networkManager => parent ? parent.networkManager : null;

        [UsedImplicitly] public bool isSceneObject => parent && parent.isSceneObject;

        [UsedImplicitly] public bool isOwner => parent && parent.isOwner;

        [UsedImplicitly] public bool isClient => parent && parent.isClient;

        [UsedImplicitly] public bool isServer => parent && parent.isServer;

        [UsedImplicitly] public bool isServerOnly => parent && parent.isServerOnly;

        [UsedImplicitly] public bool isHost => parent && parent.isHost;

        [UsedImplicitly] public bool isSpawned => parent && parent.isSpawned;

        /// <summary>
        /// Is this module dynamic, that is, was it registered to its parent identity
        /// some time after construction?
        /// Non-dynamic modules are assumed to be created when the parent identity is
        /// constructed, which means they do not need to be created via serialisation
        /// as they will already exist when the identity is spawned on the client.
        /// Dynamic modules are created by events which the client will not know about,
        /// so need a new instance created when we read their value.
        /// </summary>
        public bool isDynamic { get; private set; }

        public bool hasOwner => parent.hasOwner;

        public bool hasConnectedOwner => parent && parent.hasConnectedOwner;

        [UsedImplicitly] public PlayerID? localPlayer => parent ? parent.localPlayer : null;

        [UsedByIL] protected PlayerID localPlayerForced => parent ? parent.localPlayerForced : default;

        public PlayerID? owner => parent ? parent.owner : null;

        public bool isController => parent && parent.isController;

        /// <summary>
        /// True if this network module has been initialized somewhere, prevents
        /// re-initialisation and the passing around of network modules which
        /// is not allowed. It must have a single parent, and once assigned cannot
        /// be reassigned.
        /// </summary>
        private bool wasInitialized = false;

        /// <summary>
        /// Has the module been initialized?
        /// </summary>
        public bool isModuleInitialized { get; internal set; } = false;

        [UsedImplicitly]
        public bool IsController(bool ownerHasAuthority) => parent && parent.IsController(ownerHasAuthority);

        [UsedImplicitly]
        public bool IsController(IOwnerAuth auth) => parent && parent.IsController(auth.ownerAuth);

        [UsedImplicitly]
        public bool IsController(bool ownerHasAuthority, bool asServer) =>
            parent && parent.IsController(asServer, ownerHasAuthority);

        [UsedByIL]
        public void Error(string message)
        {
            PurrLogger.LogWarning($"Module in {parent.GetType().Name} is null: <i>{message}</i>\n" +
                                  $"You can initialize it on Awake or override OnInitializeModules.", parent);
        }

        public virtual void OnReceivedRpc(int id, BitPacker stream, ChildRPCPacket packet, RPCInfo info, bool asServer) { }

        public static void OnReceivedRpc(int id, BitPacker stream, StaticRPCPacket packet, RPCInfo info, bool asServer) { }

        public virtual void OnSpawn()
        {
        }

        public virtual void OnSpawn(bool asServer)
        {
        }

        public virtual void OnDespawned()
        {
        }

        public virtual void OnDespawned(bool asServer)
        {
        }

        /// <summary>
        /// Called when an observer is added.
        /// Server only.
        /// </summary>
        /// <param name="player">The observer player id</param>
        public virtual void OnPreObserverAdded(PlayerID player)
        {
        }

        /// <summary>
        /// Called when an observer is added.
        /// Server only.
        /// </summary>
        /// <param name="player">The observer player id</param>
        /// <param name="isSpawner">If this object was just spawned and the observer is the spawner</param>
        public virtual void OnPreObserverAdded(PlayerID player, bool isSpawner)
        {
        }

        /// <summary>
        /// Called when an observer is added.
        /// Server only.
        /// </summary>
        /// <param name="player">The observer player id</param>
        public virtual void OnObserverAdded(PlayerID player)
        {
        }

        /// <summary>
        /// Called when an observer is added.
        /// Server only.
        /// </summary>
        /// <param name="player">The observer player id</param>
        /// <param name="isSpawner">If this object was just spawned and the observer is the spawner</param>
        public virtual void OnObserverAdded(PlayerID player, bool isSpawner)
        {
        }

        /// <summary>
        /// Called when an observer is removed.
        /// Server only.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnObserverRemoved(PlayerID player)
        {
        }

        public virtual void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
        }

        /// <summary>
        /// Called when the owner of this object changes.
        /// </summary>
        /// <param name="oldOwner">The old owner of this object</param>
        /// <param name="newOwner">The new owner of this object</param>
        /// <param name="isSpawnEvent">If this object was just spawned and the newOwner is the spawner</param>
        /// <param name="asServer">Is this on the server</param>
        public virtual void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool isSpawnEvent, bool asServer)
        {
        }

        public virtual void OnOwnerDisconnected(PlayerID ownerId)
        {
        }

        public virtual void OnOwnerReconnected(PlayerID ownerId)
        {
        }

        public void SetComponentParent(NetworkIdentity p, byte i, string moduleName)
        {
            parent = p;
            index = i;
            name = moduleName;
            wasInitialized = true;
        }

        [UsedByIL]
        public void RegisterModuleInternal(string moduleName, string type, NetworkModule module, bool isNetworkIdentity)
        {
            NetworkIdentity parentRef = this.parent;

            if (parentRef)
                parentRef.RegisterModuleInternal(moduleName, type, module, isNetworkIdentity);
            else PurrLogger.LogError($"Registering module '{moduleName}' failed since it is not spawned.");
        }

        [UsedByIL]
        public void RegisterKnownModuleInternal(string moduleName, byte moduleId, string type, NetworkModule module)
        {
            NetworkIdentity parentRef = this.parent;

            if (parentRef)
                parentRef.RegisterKnownModuleInternal(moduleName, moduleId, type, module);
            else PurrLogger.LogError($"Registering module '{moduleName}' failed since it is not spawned.");
        }

        [UsedByIL]
        protected void SendRPC(ChildRPCPacket packet, RPCSignature signature)
        {
            if (!parent)
            {
                if (signature.channel is Channel.ReliableOrdered or Channel.ReliableUnordered)
                    PurrLogger.LogError($"Trying to send RPC from '{GetType().Name}' which is not initialized.");
                return;
            }

            if (!parent.isSpawned)
            {
                if (signature.channel is Channel.ReliableOrdered or Channel.ReliableUnordered)
                    PurrLogger.LogError($"Trying to send RPC from '{parent.name}' which is not spawned.", parent);
                return;
            }

            NetworkManager nm = parent.networkManager;

            if (!nm.TryGetModule<RPCModule>(nm.isServer, out RPCModule module))
            {
                PurrLogger.LogError("Failed to get RPC module.", parent);
                return;
            }

            NetworkRules rules = networkManager.networkRules;
            bool shouldIgnoreOwnership = rules && rules.ShouldIgnoreRequireOwner();

            if (!shouldIgnoreOwnership && signature.requireOwnership && !isOwner)
            {
                if (!signature.runLocally)
                    PurrLogger.LogError(
                        $"Trying to send RPC '{signature.rpcName}' from '{GetType().Name}' without ownership.", parent);
                return;
            }

            bool shouldIgnore = rules && rules.ShouldIgnoreRequireServer();

            if (!shouldIgnore && signature.requireServer && !networkManager.isServer)
            {
                if (!signature.runLocally)
                    PurrLogger.LogError(
                        $"Trying to send RPC '{signature.rpcName}' from '{GetType().Name}' without server.", parent);
                return;
            }

            module.AppendToBufferedRPCs(packet, signature);

            switch (signature.type)
            {
                case RPCType.ServerRPC: parent.SendToServer(packet, signature.channel); break;
                case RPCType.ObserversRPC:
                {
                    if (isServer)
                        parent.SendToObservers(packet, ShouldSend, signature.channel);
                    else parent.SendToServer(packet, signature.channel);
                    break;
                }
                case RPCType.TargetRPC:
                    if (isServer)
                    {
                        using Pooling.DisposableList<PlayerID> targets = signature.GetTargets();
                        parent.Send(targets, packet, signature.channel);
                    }
                    else
                    {
                        using Pooling.DisposableList<PlayerID> targets = signature.GetTargets();

                        // TODO: we should batch this into one packet to the server instead of N
                        for (int i = 0; i < targets.Count; i++)
                        {
                            packet.targetPlayerId = targets[i];
                            parent.SendToServer(packet, signature.channel);
                        }
                    }
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return;

            bool ShouldSend(PlayerID player)
            {
                bool isLocalPlayer = player == networkManager.localPlayer;

                if (signature.runLocally && isLocalPlayer)
                    return false;

                if (signature.excludeSender && isLocalPlayer)
                    return false;

                return !signature.excludeOwner || parent.IsNotOwnerPredicate(player);
            }
        }

#if UNITY_EDITOR
        private Type _myType;
#endif

        [UsedByIL]
        protected bool ValidateReceivingRPC(RPCInfo info, RPCSignature signature, IRpc data, bool asServer)
        {
#if UNITY_EDITOR
            _myType ??= GetType();
            Statistics.ReceivedRPC(_myType, signature.type, signature.rpcName, data.rpcData.segment, parent);
#endif
            return parent && parent.ValidateIncomingRPC(info, signature, data, asServer);
        }

        [UsedByIL]
        protected object CallGeneric(string methodName, GenericRPCHeader rpcHeader)
        {
            NetworkIdentity.InstanceGenericKey key = new NetworkIdentity.InstanceGenericKey(methodName, GetType(), rpcHeader.types);

            if (!NetworkIdentity.genericMethods.TryGetValue(key, out MethodInfo gmethod))
            {
                MethodInfo method = GetType().GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                gmethod = method?.MakeGenericMethod(rpcHeader.types);

                NetworkIdentity.genericMethods.Add(key, gmethod);
            }

            if (gmethod == null)
            {
                PurrLogger.LogError($"Calling generic RPC failed. Method '{methodName}' not found.");
                return null;
            }

            return gmethod.Invoke(this, rpcHeader.values);
        }

        [UsedByIL]
        protected ChildRPCPacket BuildRPC(byte rpcId, BitPacker data)
        {
            if (!parent)
                throw new InvalidOperationException(
                    $"Trying to send RPC from '{GetType().Name}' which is not spawned.");

            ChildRPCPacket rpc = new ChildRPCPacket
            {
                networkId = parent.id!.Value,
                sceneId = parent.sceneId,
                childId = index,
                rpcId = rpcId,
                data = data.ToByteData(),
                senderId = RPCModule.GetLocalPlayer(networkManager)
            };

            return rpc;
        }

        public virtual void OnInitializeModules()
        {
        }

        /// <summary>
        /// Called when this object is spawned but before any other data is received.
        /// At this point you might be missing ownership data, module data, etc.
        /// This is only called once even if in host mode.
        /// </summary>
        public virtual void OnEarlySpawn()
        {
        }

        /// <summary>
        /// Called when this object is spawned but before any other data is received.
        /// At this point you might be missing ownership data, module data, etc.
        /// This is called twice in host mode, once for the server and once for the client.
        /// </summary>
        public virtual void OnEarlySpawn(bool asServer)
        {
        }

        /// <summary>
        /// Called when this object is put back into the pool.
        /// Use this to reset any values for the next spawn.
        /// </summary>
        public virtual void OnPoolReset()
        {
        }

        protected string GetPermissionErrorDetails(IOwnerAuth auth)
        {
            return GetPermissionErrorDetails(
                auth.ownerAuth,
                isServer,
                owner,
                localPlayer
            );
        }

        protected static string GetPermissionErrorDetails(bool ownerAuth, NetworkModule module)
        {
            return GetPermissionErrorDetails(
                ownerAuth,
                module.isServer,
                module.owner,
                module.localPlayer
            );
        }

        static string GetPermissionErrorDetails(bool ownerAuth, bool isServer, PlayerID? owner, PlayerID? local)
        {
            return ownerAuth switch
            {
                true when isServer =>
                    $"Server is trying to act on module that is `<b>ownerAuth</b>` but the owner is `<b>{owner}</b>` (not you).",
                true =>
                    $"Client is trying to act on module that is `<b>ownerAuth</b>` but the owner is `<b>{owner}</b>` (not you: `{local}`).",
                _ => "Client is trying to act on module that is not `<b>ownerAuth</b>`, only server can act on it."
            };
        }

        /// <summary>
        /// Store whether or not we can be dynamic
        /// </summary>
        private static Dictionary<Type, bool> _dynamicSafeCache = new Dictionary<Type, bool>();

        /// <summary>
        /// When we are added to a SyncVar, or contained inside another NetworkModule then
        /// we need to be initialized so that we can be properly networked.
        /// </summary>
        /// <param name="parentIdentity"></param>
        /// <param name="attachedToModule"></param>
        /// <param name="moduleID">If we already know the module ID, then it can be provided.</param>
        internal void LateInitialize(NetworkIdentity parentIdentity, NetworkModule attachedToModule, byte? moduleID = null)
        {
            // Check for re-registration
            if (wasInitialized)
            {
                throw new LateModuleException($"A network module called <b>{name}</b> was attached to the " +
                    $"module <b>{attachedToModule.name}</b>, but it had already been attached to " +
                    $"another module. Once a network module has been initialized, it must remain" +
                    $"attached to its parent module, and cannot be reassigned. Do not assign" +
                    $"network modules to any network modules such as SyncVar unless they are" +
                    $"new instances.");
            }
            // Check for a default constructor
            if (!_dynamicSafeCache.TryGetValue(GetType(), out bool isSafe))
            {
                isSafe = GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null) != null;
                _dynamicSafeCache.TryAdd(GetType(), isSafe);
            }
            if (!isSafe)
            {
                throw new LateModuleException($"The network module <b>{name}</b> of type <b>{GetType().Name}</b> referenced via the variable " +
                    $"<b>{attachedToModule.name} </b>" +
                    $"is initialized late via a SyncVar type, but does not have a default constructor. Please either provide" +
                    $"a default constructor if the state of the object does not depend on the values provided in the constructor " +
                    $"or mark the type as packable to override the packing behaviour.");
            }
            // Mark as dynamic
            isDynamic = true;
            // Register the module
            if (!moduleID.HasValue)
            {
                if (attachedToModule == null)
                {
                    parentIdentity.RegisterModuleInternal(name, GetType().Name, this, false);
                }
                else
                {
                    attachedToModule.RegisterModuleInternal(attachedToModule.name, attachedToModule.GetType().Name, this, false);
                }
            }
            else
            {
                if (attachedToModule == null)
                {
                    parentIdentity.RegisterKnownModuleInternal(name, moduleID.Value, GetType().Name, this);
                }
                else
                {
                    attachedToModule.RegisterKnownModuleInternal(attachedToModule.name, moduleID.Value, attachedToModule.GetType().Name, this);
                }
            }
            // Find and call the init method on ourselves
            // This init method will invoke RegisterModuleInternal, and will recursively
            // call the init methods on our children (if we have any).
            parentIdentity.LateInitializeModule(this);
        }

        /// <summary>
        /// Attach a network module to this module at runtime, allowing
        /// for the use of RPCs and contained network modules.
        /// Creating new network modules, or reassigning them at runtime
        /// will cause them to not function correctly if they have not 
        /// been attached.
        /// </summary>
        /// <param name="module"></param>
        public void Attach(NetworkModule module)
        {
            if (module == null)
                throw new NullReferenceException($"Attempting to attach a null module to a <b>{GetType().Name}</b>.");
            if (parent == null)
                throw new LateModuleException($"The network module <b>{module.GetType().Name}</b> cannot be attached to a <b>{GetType().Name}</b> " +
                    $"before it has been initialized.");
            module.LateInitialize(parent, this);
        }

        /// <summary>
        /// Detatch a network module from ourselves, which should then
        /// be immediately disposed and not reused.
        /// By detatching a network module, it will not longer function.
        /// Calling detatch for a network module that has all references
        /// removed is necessary to clean it up. If modules are dynamically
        /// created but not cleaned up, then the 255 limit will be reached
        /// quickly.
        /// </summary>
        /// <param name="module"></param>
        public void Detatch(NetworkModule module)
        {
            // TODO: Allow modules to be detached so that their ID can be recycled.
            throw new LateModuleException($"Types deriving from NetworkModule are immutable once added to a List can cannot be modified.");
        }

    }
}
