using PurrNet.Transports;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace PurrNet.Editor
{
    public static class PurrNetToolBarStatus
    {
        private static GUIContent _pebblesIcon;

        [InitializeOnLoadMethod]
        static void Init()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            NetworkManager.onAnyServerConnectionState += OnConnectionStateChanged;
            NetworkManager.onAnyClientConnectionState += OnConnectionStateChanged;
            
            _pebblesIcon = new GUIContent(Resources.Load<Texture2D>("purrlogo"));
        }

        private static void OnConnectionStateChanged(ConnectionState state)
        {
            ToolbarExtender.RequestToolbarRepaint();
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            var manager = NetworkManager.main;

            GUILayout.BeginHorizontal();
            
            GUILayout.Label(_pebblesIcon, GUILayout.Width(22), GUILayout.Height(22));
            GUILayout.Label("PurrNet", GUILayout.ExpandWidth(false));
            
            DrawConnectionButton(manager, true);  // Server
            DrawConnectionButton(manager, false); // Client
            
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            if (IsClientOrServerTransitioning(manager)) {
                ToolbarExtender.RequestToolbarRepaint();
            }
        }

        private static void DrawConnectionButton(NetworkManager? manager, bool isServer)
        {
            ConnectionState? state = manager != null ? (isServer ? manager.serverState : manager.clientState) : null;
            var isActive = manager != null && (isServer ? manager.isServer : manager.isClient);
            var isTransitioning = state is ConnectionState.Connecting or ConnectionState.Disconnecting;
            
            string buttonText = isTransitioning ? state.ToString() : 
                               isActive ? $"Stop {(isServer ? "Server" : "Client")}" : 
                               $"Start {(isServer ? "Server" : "Client")}";

            GUI.enabled = manager != null && !isTransitioning;
            if (GUILayout.Button(buttonText, GUILayout.Width(100)))
            {
                if (isServer)
                {
                    if (isActive) manager.StopServer();
                    else manager?.StartServer();
                }
                else
                {
                    if (isActive) manager.StopClient();
                    else manager?.StartClient();
                }
            }

            TransportInspector.DrawLed(state);
            GUI.enabled = true;
        }
        
        private static bool IsClientOrServerTransitioning(NetworkManager? manager)
        {
            if (manager == null) return false;
            
            return manager.serverState is ConnectionState.Connecting or ConnectionState.Disconnecting ||
                   manager.clientState is ConnectionState.Connecting or ConnectionState.Disconnecting;
        }
    }
}
