using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [CustomEditor(typeof(NetworkOwnershipToggle))]
    public class NetworkOwnershipToggleInspector : UnityEditor.Editor
    {
        private static readonly Color HeaderColor = new Color(0.15f, 0.25f, 0.35f); // Dark Blue Header

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HeaderPaint();

            SerializedProperty toggleTargets = serializedObject.FindProperty("_toggleTargets");
            EditorGUILayout.PropertyField(toggleTargets, new GUIContent("Ownership States"), true);

            serializedObject.ApplyModifiedProperties();
        }

        private void HeaderPaint()
        {
            GUILayout.Space(5);

            // Dynamic text content
            string title = "Network Ownership Toggle\n";
            string description = "Manages GameObjects and Components by dynamically activating or deactivating them based on network ownership status. Mark targets as true to activate them for the owner, or false to deactivate them for the owner.";

            // Calculate the required height based on text size
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };

            GUIStyle descStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            float titleHeight = titleStyle.CalcHeight(new GUIContent(title), EditorGUIUtility.currentViewWidth);
            float descHeight = descStyle.CalcHeight(new GUIContent(description), EditorGUIUtility.currentViewWidth);

            float totalHeight = titleHeight + descHeight + 10f; // Padding

            // Draw background
            Rect headerRect = EditorGUILayout.GetControlRect(false, totalHeight);
            EditorGUI.DrawRect(headerRect, HeaderColor);

            // Draw text
            EditorGUI.LabelField(new Rect(headerRect.x, headerRect.y + 2f, headerRect.width, titleHeight), title, titleStyle);
            EditorGUI.LabelField(new Rect(headerRect.x, headerRect.y + titleHeight + 2f, headerRect.width, descHeight), description, descStyle);

            GUILayout.Space(5);
        }
    }
}