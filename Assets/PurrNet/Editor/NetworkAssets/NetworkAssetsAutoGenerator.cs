#if UNITY_EDITOR
using UnityEditor;

namespace PurrNet.Editor
{
    [InitializeOnLoad]
    public static class NetworkAssetsAutoGenerator
    {
        static NetworkAssetsAutoGenerator()
        {
            EditorApplication.delayCall += GenerateAll;
        }

        private static void GenerateAll()
        {
            var guids = AssetDatabase.FindAssets("t:NetworkAssets");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<NetworkAssets>(path);
                if (!asset || !asset.autoGenerate) continue;

                asset.GenerateAssets();
            }
        }
    }
}
#endif