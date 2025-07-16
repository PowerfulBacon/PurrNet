using System;
using System.Threading.Tasks;
using UnityEngine;

namespace PurrNet
{
    [DefaultExecutionOrder(32000)]
    public class UnityLatestUpdate : MonoBehaviour
    {
        static UnityLatestUpdate _instance;

        public static event Action onUpdate;

        public static event Action onFixedUpdate;

        public static event Action onLatestUpdate;

        public static Task Yield()
        {
            var promise = new TaskCompletionSource<bool>();

            onUpdate += OnUpdate;

            return promise.Task;

            void OnUpdate()
            {
                if (promise.TrySetResult(true))
                    onUpdate -= OnUpdate;
            }
        }

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

        private void Update()
        {
            onUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            onLatestUpdate?.Invoke();
        }
    }
}
