using System;
using System.Collections.Generic;
using PurrNet.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrNet.Modules
{
    public static class SceneObjectsModule
    {
        public static event Func<GameObject, bool> onFilterSceneObjects;

        private static readonly List<NetworkIdentity> _sceneIdentities = new List<NetworkIdentity>();

        static bool PassesFilters(GameObject go)
        {
            if (onFilterSceneObjects == null)
                return true;

            var list = onFilterSceneObjects.GetInvocationList();

            for (var i = 0; i < list.Length; i++)
            {
                var @delegate = list[i];
                try
                {
                    var filter = (Func<GameObject, bool>)@delegate;
                    if (!filter(go))
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
                if (!PassesFilters(rootObject)) continue;

                rootObject.gameObject.GetComponentsInChildren(true, _sceneIdentities);

                if (_sceneIdentities.Count == 0) continue;

                rootObject.gameObject.MakeSureAwakeIsCalled();
                networkIdentities.AddRange(_sceneIdentities);
            }
        }
    }
}
