using System;
using System.Collections.Generic;
using PurrNet.Logging;
using UnityEngine.SceneManagement;

namespace PurrNet.Modules
{
    public static class SceneObjectsModule
    {
        public static event Func<NetworkIdentity, bool> onFilterSceneObjects;

        private static readonly List<NetworkIdentity> _sceneIdentities = new List<NetworkIdentity>();

        static bool PassesFilters(NetworkIdentity id)
        {
            if (onFilterSceneObjects == null)
                return true;

            var list = onFilterSceneObjects.GetInvocationList();

            for (var i = 0; i < list.Length; i++)
            {
                var @delegate = list[i];
                try
                {
                    var filter = (Func<NetworkIdentity, bool>)@delegate;
                    if (!filter(id))
                        return false;
                }
                catch (Exception e)
                {
                    PurrLogger.LogError($"Exception thrown by filter: {e}");
                }
            }

            return true;
        }

        public static void GetSceneIdentities(Scene scene, List<NetworkIdentity> networkIdentities)
        {
            var rootGameObjects = scene.GetRootGameObjects();

            PurrSceneInfo sceneInfo = null;

            foreach (var rootObject in rootGameObjects)
            {
                if (rootObject.TryGetComponent<PurrSceneInfo>(out var si))
                {
                    sceneInfo = si;
                    break;
                }
            }

            if (sceneInfo)
                rootGameObjects = sceneInfo.rootGameObjects.ToArray();

            for (var i = 0; i < rootGameObjects.Length; i++)
            {
                var rootObject = rootGameObjects[i];

                if (!rootObject || rootObject.scene.handle != scene.handle) continue;

                rootObject.gameObject.GetComponentsInChildren(true, _sceneIdentities);

                if (_sceneIdentities.Count == 0) continue;

                rootObject.gameObject.MakeSureAwakeIsCalled();

                for (var index = 0; index < _sceneIdentities.Count; index++)
                {
                    var id = _sceneIdentities[index];
                    if (!PassesFilters(id)) continue;
                    networkIdentities.Add(id);
                }
            }
        }
    }
}
