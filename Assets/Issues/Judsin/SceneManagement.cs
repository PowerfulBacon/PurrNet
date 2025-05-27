using System.Collections;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Utils;
using TriInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpellBound.Core {
    public sealed class SceneManagement : NetworkBehaviour {
        // Scene Names
        [SerializeField]
        private string LoadingSceneName = "Loading";
        [SerializeField]
        private string WorldSceneName = "World";

        // Game-Ops
        private PlayerSpawner _playerSpawner;

        private void Awake() {
            InstanceHandler.RegisterInstance(this);
            DontDestroyOnLoad(this);
        }

        protected override void OnDestroy() {
            InstanceHandler.UnregisterInstance<SceneManagement>();
            // PlayerSpawner bookkeeping.
            if (!_playerSpawner) return;
            _playerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
        }

        [Button("LoadGame")]
        public void LoadGame()
        {
            StartCoroutine(LoadGameCoroutine(!ApplicationContext.isClone));
        }

        public IEnumerator LoadGameCoroutine(bool asHost, string serverAddress = null) {
            // Loading Screen!
            yield return SceneManager.LoadSceneAsync(LoadingSceneName, LoadSceneMode.Additive);

            // First things first - connect to the server.
            if (asHost) {
                StartHost();

                // The server is responsible for loading in the World scene and setting the public scene for all clients.
                yield return ChangeScene(WorldSceneName, LoadSceneMode.Additive, true);

                if (NetworkManager.main != null) {
                    _playerSpawner = FindFirstObjectByType<PlayerSpawner>();
                    _playerSpawner.OnPlayerSpawned += HandlePlayerSpawned;
                } else Debug.LogError("NetworkManager is null!", this);
            }
            else {
                StartConnection(serverAddress);

                NetworkManager.main.scenePlayersModule.onPlayerJoinedScene += HandlePlayerJoinedScene;
                NetworkManager.main.scenePlayersModule.onPlayerLoadedScene += HandlePlayerLoadedScene;
                NetworkManager.main.sceneModule.onSceneLoaded += HandleSceneLoaded;
                NetworkManager.main.playerModule.onPlayerJoined += HandlePlayerJoined;

                // Once the client has successfully connected then the sceneModule will try to load the networked scene.
                yield return AwaitSceneLoaded(WorldSceneName);
            }
        }

        private void HandlePlayerJoinedScene(PlayerID playerID, SceneID sceneID, bool asServer) {
            Debug.Log($"Player {playerID} in scene {sceneID} onPlayerJoinedScene fired.");
        }

        private void HandlePlayerLoadedScene(PlayerID playerID, SceneID sceneID, bool asServer) {
            Debug.Log($"Player {playerID} in scene {sceneID} onPlayerLoadedScene fired.");
        }

        private void HandleSceneLoaded(SceneID sceneID, bool asServer) {
            Debug.Log($"Scene {sceneID} onSceneLoaded fired.");
        }

        private void HandlePlayerJoined(PlayerID playerID, bool something, bool somethingTwo) {
            Debug.Log($"Player {playerID} onPlayerJoined fired.");
        }

        private int SceneNameToBuildIndex(string name) {
            var bIdxCount = SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < bIdxCount; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

                if (sceneName == name)
                {
                    return i;
                }
            }

            return -1;
        }

        private IEnumerator AwaitSceneLoaded(string sceneName) {
            var sceneBuildID = SceneNameToBuildIndex(sceneName);
            var sceneIsLoaded = false;
            while (!sceneIsLoaded) {
                if (NetworkManager.main.sceneModule.IsSceneLoaded(sceneBuildID)) {
                    sceneIsLoaded = true;
                    Debug.Log("Scene is loaded is true.");
                }
                yield return null;
            }

            if (NetworkManager.main != null) {
                _playerSpawner = FindFirstObjectByType<PlayerSpawner>();
                if (_playerSpawner != null) {
                    _playerSpawner.OnPlayerSpawned += HandlePlayerSpawned;
                } else Debug.LogError("PlayerSpawner is null!", this);
            } else Debug.LogError("NetworkManager is null!", this);
        }

        private IEnumerator WaitForNetworkedSceneLoaded(AsyncOperation op) {
            // Will stop the scene progress at 90%.
            op.allowSceneActivation = false;

            while (op.progress < 0.9f) {
                yield return null;
            }

            op.allowSceneActivation = true;

            // Exit this once the scene is done loading the final 10%.
            while (!op.isDone) yield return null;
        }

        private void StartHost() {
            if (!NetworkManager.main.isOffline) return;
            NetworkManager.main.StartHost();
        }

        private void StartConnection(string input) {
            if (input == null) {
                Debug.LogError("Connection address is null", this);
                return;
            }

            NetworkManager.main.StartClient();
        }

        [Button("HandleTerrainLoaded")]
        public void HandleTerrainLoaded() {
            Debug.Log("First line of HandleTerrainLoaded.");
            var worldScene = SceneManager.GetSceneByName(WorldSceneName);
            _playerSpawner.SetSpawnDetails(worldScene);
            _playerSpawner.SetSpawnReady(true);
        }

        private void HandlePlayerSpawned() {
            SceneManager.UnloadSceneAsync(LoadingSceneName);
        }

        private IEnumerator ChangeScene(string sceneName, LoadSceneMode mode, bool isPublic) {
            PurrSceneSettings settings = new() {
                isPublic = isPublic,
                mode = mode,
            };

            return WaitForNetworkedSceneLoaded(NetworkManager.main.sceneModule.LoadSceneAsync(sceneName, settings));
        }
    }
}
