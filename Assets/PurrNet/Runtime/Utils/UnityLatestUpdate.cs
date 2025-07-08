using System;
using UnityEngine;

namespace PurrNet
{
    [DefaultExecutionOrder(32000)]
    public class UnityLatestUpdate : MonoBehaviour
    {
        static UnityLatestUpdate _instance;

        public static event Action onLatestUpdate;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnSubsystemRegistration()
        {
            if (_instance)
                return;

            var go = new GameObject("PurrNet_UnityLatestUpdate")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            DontDestroyOnLoad(go);

            _instance = go.AddComponent<UnityLatestUpdate>();
        }

        private void LateUpdate()
        {
            onLatestUpdate?.Invoke();
        }
    }
}
