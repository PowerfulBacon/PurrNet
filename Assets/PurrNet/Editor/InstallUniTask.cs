using System;
using UnityEditor;
using UnityEditor.Build;

namespace PurrNet.Editor
{
    public static class EnableLeakDetector
    {
#if PURR_LEAKS_CHECK
        [MenuItem("Tools/PurrNet/Debug/Disable Leak Detection", priority = 200)]
        public static void Uninstall()
        {
            // add Scripting symbol
            var activeBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(activeBuildTargetGroup);

            var content = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            int idxOf = content.IndexOf("PURR_LEAKS_CHECK", StringComparison.Ordinal);
            bool isNextSemicolon = idxOf < content.Length - 1 && content[idxOf + 1] == ';';
            if (isNextSemicolon)
                idxOf++;
            content = content.Remove(idxOf, "PURR_LEAKS_CHECK".Length);
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, content);
        }
#else
        [MenuItem("Tools/PurrNet/Debug/Enable Leak Detection", priority = 200)]
        public static void Install()
        {
            var activeBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(activeBuildTargetGroup);

            var content = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            bool needsSemicolon = content.Length > 0 && content[^1] != ';';
            content += needsSemicolon ? ";" : "";
            content += "PURR_LEAKS_CHECK";
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, content);
        }
#endif
    }

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
