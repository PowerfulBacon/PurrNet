using System.Collections.Generic;
using PurrNet.Logging;

namespace PurrNet.Modules
{
    public class DeltaMessagerFactory : INetworkModule
    {
        readonly ScenesModule _scenes;
        readonly ScenePlayersModule _scenePlayers;
        readonly PlayersBroadcaster _broadcaster;

        readonly List<DeltaMessagerModule> _rawModules = new();
        readonly Dictionary<SceneID, DeltaMessagerModule> _modules = new();

        public DeltaMessagerFactory(ScenesModule scenes, ScenePlayersModule scenePlayers, PlayersBroadcaster broadcaster)
        {
            _scenes = scenes;
            _scenePlayers = scenePlayers;
            _broadcaster = broadcaster;
        }

        public bool TryGetModule(SceneID scene, out DeltaMessagerModule module)
        {
            return _modules.TryGetValue(scene, out module);
        }

        public void Enable(bool asServer)
        {
            var scenes = _scenes.sceneStates;

            foreach (var (id, sceneState) in scenes)
            {
                if (sceneState.scene.isLoaded)
                    OnPreSceneLoaded(id, asServer);
            }

            _scenes.onPreSceneLoaded += OnPreSceneLoaded;
            _scenes.onSceneUnloaded += OnSceneUnloaded;

        }

        public void Disable(bool asServer)
        {
            for (var i = 0; i < _rawModules.Count; i++)
                _rawModules[i].Disable();

            _scenes.onPreSceneLoaded -= OnPreSceneLoaded;
            _scenes.onSceneUnloaded -= OnSceneUnloaded;
        }

        private void OnPreSceneLoaded(SceneID scene, bool asServer)
        {
            if (_modules.ContainsKey(scene))
            {
                PurrLogger.LogError(
                    $"DeltaMessager for scene {scene} already exists; trying to create another one?");
                return;
            }

            var deltaMessagerModule = new DeltaMessagerModule(scene, _broadcaster, _scenePlayers);
            deltaMessagerModule.Enable();
            _rawModules.Add(deltaMessagerModule);
            _modules.Add(scene, deltaMessagerModule);
        }

        private void OnSceneUnloaded(SceneID scene, bool asServer)
        {
            if (!_modules.TryGetValue(scene, out var messager))
            {
                PurrLogger.LogError($"DeltaMessager for scene {scene} doesn't exist; trying to remove it?");
                return;
            }

            messager.Disable();

            _rawModules.Remove(messager);
            _modules.Remove(scene);
        }
    }
}
