#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PurrNet.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float HeaderHeight = 20f;
        private const float ElementPadding = 2f;
        private const float BottomPadding = 8f;
        private const float ColumnHeaderHeight = 18f;
        private bool _foldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            SerializedProperty valuesProp = property.FindPropertyRelative("values");
            SerializedProperty stringKeysProp = property.FindPropertyRelative("stringKeys");
            SerializedProperty stringValuesProp = property.FindPropertyRelative("stringValues");

            bool useSerializableTypes = keysProp != null && keysProp.arraySize > 0;
            SerializedProperty displayKeysProp = useSerializableTypes ? keysProp : stringKeysProp;
            SerializedProperty displayValuesProp = useSerializableTypes ? valuesProp : stringValuesProp;


            if (_foldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"{property.displayName} ({keysProp.arraySize} elements)");
                if (displayKeysProp != null && displayValuesProp != null)
                {
                    int count = displayKeysProp.arraySize;
                    for (int i = 0; i < count; i++)
                    {
                        EditorGUILayout.BeginVertical("box");
                        if (displayKeysProp.GetArrayElementAtIndex(i).GetType() == typeof(string) || displayKeysProp.GetArrayElementAtIndex(i).GetType().IsPrimitive)
                        {
                            EditorGUILayout.PropertyField(displayValuesProp.GetArrayElementAtIndex(i), new GUIContent($"{displayKeysProp.GetArrayElementAtIndex(i)}"), true);
                        }
                        else
                        {
                            bool isEnabled = GUI.enabled;
                            GUI.enabled = false;
                            EditorGUILayout.PropertyField(displayKeysProp.GetArrayElementAtIndex(i), new GUIContent("Key"), true);
                            GUI.enabled = isEnabled;
                            EditorGUILayout.PropertyField(displayValuesProp.GetArrayElementAtIndex(i), new GUIContent("Value"), true);
                        }
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif