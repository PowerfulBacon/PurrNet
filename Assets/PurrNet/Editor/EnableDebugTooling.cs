using UnityEditor;

namespace PurrNet.Editor
{
    public static class EnableDebugTooling
    {
#if PURR_DELTA_CHECK
        [MenuItem("Tools/PurrNet/Debug/Disable Delta Validator", priority = 200)]
        public static void UninstallDeltaCheck()
        {
            SymbolsHelper.RemoveSymbol("PURR_DELTA_CHECK");
        }
#else
        [MenuItem("Tools/PurrNet/Debug/Enable Delta Validator", priority = 200)]
        public static void InstallDeltaCheck()
        {
            SymbolsHelper.AddSymbol("PURR_DELTA_CHECK");
        }
#endif

#if PURR_LEAKS_CHECK
        [MenuItem("Tools/PurrNet/Debug/Disable Leak Detection", priority = 200)]
        public static void Uninstall()
        {
            SymbolsHelper.RemoveSymbol("PURR_LEAKS_CHECK");
        }
#else
        [MenuItem("Tools/PurrNet/Debug/Enable Leak Detection", priority = 200)]
        public static void Install()
        {
            SymbolsHelper.AddSymbol("PURR_LEAKS_CHECK");
        }
#endif

#if PURRNET_DEBUG_POOLING
        [MenuItem("Tools/PurrNet/Debug/Disable Pool Debug", priority = 200)]
        public static void UninstallPool()
        {
            SymbolsHelper.RemoveSymbol("PURRNET_DEBUG_POOLING");
        }
#else
        [MenuItem("Tools/PurrNet/Debug/Enable Pool Debug", priority = 200)]
        public static void InstallPool()
        {
            SymbolsHelper.AddSymbol("PURRNET_DEBUG_POOLING");
        }
#endif
    }
}
