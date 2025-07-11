using PurrNet.Transports;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using Unity.EditorCoroutines.Editor;
using System.Collections;

namespace PurrNet.Editor
{
    public static class PurrNetToolBarStatus
    {
        private static GUIContent pebblesIcon;

        [InitializeOnLoadMethod]
        static void Init()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            NetworkManager.onAnyServerConnectionState += OnConnectionStateChanged;
            NetworkManager.onAnyClientConnectionState += OnConnectionStateChanged;
            
            var pebblesTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/PurrNet/Editor/Editor Default Resources/Pebbles.png");
            pebblesIcon = new GUIContent(pebblesTexture);
        }

        private static void OnConnectionStateChanged(ConnectionState state)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(AutoRefreshCoroutine());
        }

        private static IEnumerator AutoRefreshCoroutine()
        {
            while (IsTransitioning(NetworkManager.main)) {
                yield return new EditorWaitForSeconds(0.5f);
            }
            ToolbarExtender.RequestToolbarRepaint();
        }

        private static bool IsTransitioning(NetworkManager? manager) =>
            manager?.serverState is ConnectionState.Connecting or ConnectionState.Disconnecting ||
            manager?.clientState is ConnectionState.Connecting or ConnectionState.Disconnecting;

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            var manager = NetworkManager.main;

            GUILayout.BeginHorizontal();
            
            GUILayout.Label(pebblesIcon, GUILayout.Width(22), GUILayout.Height(22));
            GUILayout.Label("PurrNet", GUILayout.ExpandWidth(false));
            
            DrawConnectionButton(manager, true);  // Server
            DrawConnectionButton(manager, false); // Client
            
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
        }

        private static void DrawConnectionButton(NetworkManager manager, bool isServer)
        {
            var state = isServer ? manager?.serverState : manager?.clientState;
            var isActive = isServer ? manager?.isServer == true : manager?.isClient == true;
            var isTransitioning = state is ConnectionState.Connecting or ConnectionState.Disconnecting;
            
            string buttonText = isTransitioning ? state.ToString() : 
                               isActive ? $"Stop {(isServer ? "Server" : "Client")}" : 
                               $"Start {(isServer ? "Server" : "Client")}";

            GUI.enabled = manager != null && !isTransitioning;
            if (GUILayout.Button(buttonText, GUILayout.Width(100)))
            {
                if (isServer)
                {
                    if (manager.isServer) manager.StopServer();
                    else manager.StartServer();
                }
                else
                {
                    if (manager.isClient) manager.StopClient();
                    else manager.StartClient();
                }
            }

            TransportInspector.DrawLed(state);
        }
    }
}
