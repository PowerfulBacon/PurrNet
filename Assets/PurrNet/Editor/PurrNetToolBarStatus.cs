using PurrNet.Transports;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace PurrNet.Editor
{
    public static class PurrNetToolBarStatus
    {
        private static int _extraDraws;
        private static GUIContent pebblesIcon;

        [InitializeOnLoadMethod]
        static void Init()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            NetworkManager.onAnyServerConnectionState += OnConnectionStateChanged;
            NetworkManager.onAnyClientConnectionState += OnConnectionStateChanged;
            var defaultIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/PurrNet/Editor/Editor Default Resources/Pebbles.png");
            pebblesIcon = new GUIContent(defaultIcon);
        }

        private static void OnConnectionStateChanged(ConnectionState state)
        {
            ToolbarExtender.RequestToolbarRepaint();
            _extraDraws = 10; // Force a repaint to ensure the toolbar updates
        }

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            var manager = NetworkManager.main;

            GUILayout.BeginHorizontal("EditorToolbar");
            
            GUILayout.Label(pebblesIcon, GUILayout.Width(22), GUILayout.Height(22));
            GUILayout.Label("PurrNet", GUILayout.ExpandWidth(false));
            
            ConnectionState? serverState = manager == null ? null : manager.serverState; 
            bool serverButtonEnabled = manager != null && serverState != ConnectionState.Connecting &&
                                 serverState != ConnectionState.Disconnecting;
            string serverButtonText =
                serverState == ConnectionState.Connecting || serverState == ConnectionState.Disconnecting
                    ? serverState.ToString()
                    : (manager?.isServer == true)
                        ? "Stop Server"
                        : "Start Server";

            GUI.enabled = serverButtonEnabled;
            if (GUILayout.Button(serverButtonText, GUILayout.Width(100))) {
                if (manager.isServer) {
                    manager.StopServer();
                }
                else {
                    manager.StartServer();
                }
            }

            TransportInspector.DrawLed(serverState);

            ConnectionState? clientState = manager == null ? null : manager.clientState;  
            bool clientButtonEnabled = manager != null && clientState != ConnectionState.Connecting &&
                                 clientState != ConnectionState.Disconnecting;
            string clientButtonText =
                clientState == ConnectionState.Connecting || clientState == ConnectionState.Disconnecting
                    ? clientState.ToString()
                    : (manager?.isClient == true)
                        ? "Stop Client"
                        : "Start Client";

            GUI.enabled = clientButtonEnabled;
            if (GUILayout.Button(clientButtonText, GUILayout.Width(100))) {
                if (manager.isClient) {
                    manager.StopClient();
                }
                else {
                    manager.StartClient();
                }
            }

            TransportInspector.DrawLed(clientState);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            if (_extraDraws > 0) {
                _extraDraws--;
                ToolbarExtender.RequestToolbarRepaint();
            }
        }
    }
}
