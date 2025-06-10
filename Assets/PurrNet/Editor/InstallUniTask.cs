using UnityEditor;

namespace PurrNet.Editor
{
    public static class InstallUniTask
    {
#if UNITASK_PURRNET_SUPPORT
        [MenuItem("Tools/PurrNet/Packages/Uninstall UniTask", priority = 100)]
        public static void Uninstall()
        {
            if (EditorUtility.DisplayDialog("Uninstall UniTask", "This will remove UniTask from the package manager. Do you want to continue?", "Yes", "No"))
            {
                UnityEditor.PackageManager.Client.Remove("com.cysharp.unitask");
            }
        }
#else
        [MenuItem("Tools/PurrNet/Packages/Install UniTask", priority = 100)]
        public static void Install()
        {
            if (EditorUtility.DisplayDialog("Install UniTask", "This will install UniTask from the package manager. Do you want to continue?", "Yes", "No"))
            {
                UnityEditor.PackageManager.Client.Add("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
            }
        }
#endif
    }
}
