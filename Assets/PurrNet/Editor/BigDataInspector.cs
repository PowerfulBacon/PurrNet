using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [CustomPropertyDrawer(typeof(SyncBigData), true)]
    public class BigDataInspector : PropertyDrawer
    {
        const float PROGRESS_HEIGHT = 16f;
        const float PROGRESS_HEIGHT_TOTAL = 16f + PROGRESS_PADDING * 2f;
        const float PROGRESS_PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var ownerAuth = property.FindPropertyRelative("_ownerAuth");
            var maxKBPerSec = property.FindPropertyRelative("_maxKBPerSec");
            var ownerAuthHeight = EditorGUI.GetPropertyHeight(ownerAuth);
            var maxKBPerSecHeight = EditorGUI.GetPropertyHeight(maxKBPerSec);

            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight + ownerAuthHeight + maxKBPerSecHeight + PROGRESS_HEIGHT_TOTAL;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            const float LEFT_PADDING = 10f;
            var ownerAuth = property.FindPropertyRelative("_ownerAuth");
            var maxKBPerSec = property.FindPropertyRelative("_maxKBPerSec");
            var receivingState = property.FindPropertyRelative("_syncStatus");

            var ownerAuthHeight = EditorGUI.GetPropertyHeight(ownerAuth);
            var maxKBPerSecHeight = EditorGUI.GetPropertyHeight(maxKBPerSec);

            var rect = new Rect(position.x + LEFT_PADDING, position.y + EditorGUIUtility.singleLineHeight, position.width - LEFT_PADDING, ownerAuthHeight);
            EditorGUI.PropertyField(rect, ownerAuth);

            rect.y += ownerAuthHeight;
            rect.height = maxKBPerSecHeight;
            EditorGUI.PropertyField(rect, maxKBPerSec);

            rect.y += maxKBPerSecHeight + PROGRESS_PADDING;
            rect.height = PROGRESS_HEIGHT;

            var percent = receivingState.FindPropertyRelative("percent").floatValue;

            if (percent <= 0f)
                 EditorGUI.ProgressBar(rect, percent, "Idle");
            else EditorGUI.ProgressBar(rect, percent, percent >= 1f ? "Done!" : "Downloading");
            EditorGUI.EndProperty();
        }
    }
}
