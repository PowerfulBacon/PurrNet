using PurrNet.Logging;
using PurrNet.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PurrNet
{
    public partial class NetworkIdentity
    {
        /// <summary>
        /// The modules attached to the identity.
        /// </summary>
        public IReadOnlyList<NetworkModule> modules => _externalModulesView;

        /// <summary>
        /// External modules, without null elements.
        /// The index does not line up with the internal ID of the module, use TryGetModule() to get
        /// a module with a specific ID.
        /// </summary>
        private readonly List<NetworkModule> _externalModulesView = new List<NetworkModule>();
        private readonly List<NetworkModule> _modules = new List<NetworkModule>();

        private byte _moduleId;

        private Queue<byte> _overrideModuleIDs;

        /// <summary>
        /// If we run out of module IDs, then we can recycle any that have been detached.
        /// </summary>
        private Queue<byte> _recycledModuleIDs = null;

        [UsedByIL]
        public void UnregisterModuleInternal(NetworkModule module)
        {
            UnityEngine.Debug.Log($"Module {module.name} was detached");
            // Remove the module
            _externalModulesView.Remove(module);
            _modules[module.index] = null;
            // Despawn the module
            try
            {
                module.OnDespawned();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            try
            {
                module.OnDespawned(isServer);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            // Create a new stack for recycled IDs
            if (_recycledModuleIDs == null)
                _recycledModuleIDs = new Queue<byte>();
            _recycledModuleIDs.Enqueue(module.index);
        }

        [UsedByIL]
        public void RegisterModuleInternal(string moduleName, string type, NetworkModule module, bool isNetworkIdentity)
        {
            Debug.Log($"Init method called for {moduleName}");

            bool incrementModuleID = true;

            byte newModuleID = _moduleId;

            if (_overrideModuleIDs != null)
            {
                while (_overrideModuleIDs.Count > 0)
                {
                    // Check if we already assigned this module ID directly
                    byte current = _overrideModuleIDs.Dequeue();
                    // Already assigned via force assign, skip
                    if (_externalModulesView.Any(x => x.index == current))
                    {
                        continue;
                    }
                    // Force set this module ID
                    newModuleID = current;
                    incrementModuleID = false;
                    break;
                }
                Debug.Log($"Force assigning new module to {newModuleID}");
                // Used up all the overrides
                if (_overrideModuleIDs.Count == 0)
                    _overrideModuleIDs = null;
                while (newModuleID > _modules.Count)
                {
                    _modules.Add(null);
                }
            }

            if (newModuleID >= byte.MaxValue)
            {
                // Re-use any old IDs which are no longer in use if we run past the byte limit.
                if (_recycledModuleIDs != null && _recycledModuleIDs.Count > 0)
                {
                    newModuleID = _recycledModuleIDs.Dequeue();
                    incrementModuleID = false;
                }
                else if (_modules.Any(x => x?.isDynamic ?? false))
                {
                    throw new System.Exception($"Too many modules in {GetType().Name}! Max is {byte.MaxValue}.\n" +
                                               $"This can be caused by dynamically created modules which are not " +
                                               $"being detached correctly when references to them are removed.");
                }
                else
                {
                    throw new System.Exception($"Too many modules in {GetType().Name}! Max is {byte.MaxValue}.\n" +
                                               $"This could also happen with circular dependencies.");
                }
            }

            if (module == null)
            {
                if (incrementModuleID)
                    ++_moduleId;
                _modules.Add(null);

                if (isNetworkIdentity)
                {
                    PurrLogger.LogWarning($"Module in {GetType().Name} is null: <i>{type}</i> {moduleName};\n" +
                                          $"You can initialize it on Awake or override OnInitializeModules.",
                        this);
                }

                return;
            }

            if (newModuleID == _modules.Count)
            {
                module.SetComponentParent(this, newModuleID, moduleName);
                _modules.Add(module);
            }
            else if (newModuleID < _modules.Count)
            {
                if (_modules[newModuleID] != null)
                    throw new LateModuleException($"Module in {GetType().Name} has overflowed its space of allowed IDs. This " +
                        $"can happen when modules are created in the constructor of an object, but are created with non-deterministic " +
                        $"behaviour. The server and the client must initiate any local modules in the exact same way, otherwise they " +
                        $"should be stored inside of a SyncVar to become a dynamic module.");
                module.SetComponentParent(this, newModuleID, moduleName);
                _modules[newModuleID] = module;
            }
            else
            {
                throw new LateModuleException("ModuleID was greater than the number of modules. An internal issue has occurred, possibly due to another " +
                    "exception.");
            }
            _externalModulesView.Add(module);
            if (incrementModuleID)
                _moduleId++;
        }

        /// <summary>
        /// Register a known module which should already exist.
        /// </summary>
        /// <param name="moduleName">The name of the module being registered.</param>
        /// <param name="moduleId">The ID of the module to use.</param>
        /// <param name="type">The type name of the module being used.</param>
        /// <param name="module">The module being registered</param>
        /// <exception cref="System.Exception"></exception>
        /// <exception cref="System.NullReferenceException"></exception>
        internal void RegisterKnownModuleInternal(string moduleName, byte moduleId, string type, NetworkModule module)
        {
            if (moduleId >= byte.MaxValue)
            {
                throw new System.Exception($"Too many modules in {GetType().Name}! Max is {byte.MaxValue}.\n" +
                                           $"This could also happen with circular dependencies.");
            }

            if (module == null)
            {
                throw new System.NullReferenceException("Attempting to register a null module by ID, which is not" +
                    "allowed. When an ID is provided, the module is assumed to be existing.");
            }

            // This will cause the module ID to go backwards in some cases, but that behaviour
            // is necessary for the client; which this can only be called on. If something is created
            // at position 9, any network modules inside of its constructor will be named from 10, 11, etc.
            // and 9 may be delivered after 18, so we want to revert backwards.
            // The client should never be making its own network modules outside of the constructor, and
            // they wouldn't work or affect this if they did anyway.
            // Once we hit the max value, we do not go backwards as we only pull from our list of recycled
            // IDs instead.
            if (_moduleId < byte.MaxValue)
            {
                _moduleId = moduleId;
                _moduleId++;
            }
            else
            {
                _recycledModuleIDs = new Queue<byte>(_recycledModuleIDs.Where(x => x != moduleId));
            }

            module.SetComponentParent(this, moduleId, moduleName);

            // Expand the array
            while (_modules.Count < moduleId + 1)
            {
                _modules.Add(null);
            }
            
            _modules[moduleId] = module;
            _externalModulesView.Add(module);
        }

        public bool TryGetModule(byte moduleId, out NetworkModule module)
        {
            if (moduleId >= _modules.Count)
            {
                module = null;
                return false;
            }

            module = _modules[moduleId];
            return module != null;
        }

        private void RegisterEvents()
        {
            for (int i = 0; i < _externalModulesView.Count; i++)
            {
                NetworkModule module = _externalModulesView[i];
                if (module is ITick tickableModule)
                {
                    _tickables.Add(tickableModule);
                }
            }
        }

        [TargetRpc(Transports.Channel.ReliableOrdered)]
        public static void SyncIdentityModuleIDs(PlayerID target, NetworkIdentity identity, byte moduleID, byte[] overrideModuleIDs, byte[] recycledModuleIDs)
        {
            identity._recycledModuleIDs = new Queue<byte>(recycledModuleIDs);
            identity._overrideModuleIDs = new Queue<byte>(overrideModuleIDs);
            // If we are using recycled module IDs, then we need to make sure we don't use sequential
            // ordering and only pull form recycled IDs.
            identity._moduleId = moduleID;
            while (identity._modules.Count < identity._moduleId)
            {
                identity._modules.Add(null);
            }
            Debug.Log($"Identity module ID state synced to {string.Join(", ", recycledModuleIDs)} with forced ID ordering of {string.Join(", ", overrideModuleIDs)}");
        }

    }
}