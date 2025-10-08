using UnityEditor;
using UnityEditor.PackageManager;

namespace PurrNet.Editor
{
    public static class InstallCompatiblePackages
    {
        const string PACKAGES = "Tools/PurrNet/Packages";

#if EDGEGAP_PURRNET_SUPPORT
        [MenuItem(PACKAGES + "/Edgegap/Update Edgegap", priority = 100)]
        public static void UpdateEdgegap()
        {
            Client.Remove("com.edgegap.unity-servers-plugin");
            Client.Add(
                "https://github.com/edgegap/edgegap-unity-plugin.git#partner/purrnet-source");
            Client.Resolve();
        }

        [MenuItem(PACKAGES + "/Edgegap/Uninstall Edgegap", priority = 100)]
        public static void UninstallEdgegap()
        {
            if (EditorUtility.DisplayDialog("Uninstall Edgegap", "This will remove Edgegap from the package manager. Do you want to continue?", "Yes", "No"))
            {
                Client.Remove("com.edgegap.unity-servers-plugin");
                SymbolsHelper.RemoveSymbol("EDGEGAP_PLUGIN_SERVERS");
                Client.Resolve();
            }
        }
#else
        [MenuItem(PACKAGES + "/Install Edgegap", priority = 100)]
        public static async void InstallEdgegap()
        {
            if (!GitHelper.CheckGit())
                return;

            Client.Add("https://github.com/edgegap/edgegap-unity-plugin.git#partner/purrnet-source");
            Client.Resolve();
        }
#endif

#if UNITASK_PURRNET_SUPPORT
        [MenuItem(PACKAGES + "/Uninstall UniTask", priority = 101)]
        public static void UninstallUniTask()
        {
            if (EditorUtility.DisplayDialog("Uninstall UniTask", "This will remove UniTask from the package manager. Do you want to continue?", "Yes", "No"))
            {
                Client.Remove("com.cysharp.unitask");
                Client.Resolve();
            }
        }
#else
        [MenuItem(PACKAGES + "/Install UniTask", priority = 101)]
        public static void InstallUniTask()
        {
            if (!GitHelper.CheckGit())
                return;

            if (EditorUtility.DisplayDialog("Install UniTask",
                    "This will install UniTask from the package manager. Do you want to continue?", "Yes", "No"))
            {
                Client.Add("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
                Client.Resolve();
            }
        }
#endif
    }
}
