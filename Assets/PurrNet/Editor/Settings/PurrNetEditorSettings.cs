using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    public static class PurrNetEditorSettings
    {
        static readonly HashSet<string> _keywords =
            new HashSet<string>(new[] { "PurrNet", "Networking", "Strip" });

        [SettingsProvider]
        public static SettingsProvider CreatePurrNetSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Networking/PurrNet", SettingsScope.Project)
            {
                keywords = _keywords,
                label = "PurrNet",
                guiHandler = GUIHandler
            };
            return provider;
        }

        private static void GUIHandler(string searchContext)
        {
            GUILayout.BeginVertical("helpbox");

            var settings = PurrNetSettings.GetOrCreateSettings();

            EditorGUI.BeginChangeCheck();

            var result = EditorGUILayout.EnumPopup(
                new GUIContent("Strip Code Mode",
                    "Defines how PurrNet will handle unused RPCs and SyncVars in builds. " +
                    "This can help reduce build size and improve performance."),
                settings.stripCodeMode);
            settings.stripCodeMode = (StripCodeMode)result;

            if (EditorGUI.EndChangeCheck())
                PurrNetSettings.SaveSettings(settings);

            GUILayout.EndVertical();
        }
    }
}
