using System;
using JetBrains.Annotations;
#if UNITY_EDITOR && PURR_BUTTONS
using System.Reflection;
using PurrNet.Logging;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Scripting;

namespace PurrNet
{
    [AttributeUsage(AttributeTargets.Method), UsedImplicitly]
    public class PurrButtonAttribute : PreserveAttribute
    {
        public string ButtonName { get; private set; }

        public PurrButtonAttribute(string buttonName = "")
        {
            ButtonName = buttonName;
        }
    }

#if UNITY_EDITOR && PURR_BUTTONS
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class PurrButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MonoBehaviour targetMB = (MonoBehaviour)target;

            MethodInfo[] methods = targetMB.GetType().GetMethods(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                PurrButtonAttribute buttonAttr = method.GetCustomAttribute<PurrButtonAttribute>();

                if (buttonAttr != null)
                {
                    string buttonName = !string.IsNullOrEmpty(buttonAttr.ButtonName)
                        ? buttonAttr.ButtonName
                        : ObjectNames.NicifyVariableName(method.Name);

                    if (GUILayout.Button(buttonName))
                    {
                        ParameterInfo[] parameters = method.GetParameters();

                        if (parameters.Length == 0)
                        {
                            method.Invoke(targetMB, null);
                        }
                        else
                        {
                            PurrLogger.LogWarning($"Cannot invoke method '{method.Name}' with PurrButton attribute because it has parameters. PurrButton only works with parameterless methods.");
                        }
                    }
                }
            }
        }
    }
#endif
}
