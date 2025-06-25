using PurrNet.Transports;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace PurrNet.Editor
{
    public static class PurrNetToolBarStatus
    {
        private static int _extraDraws;

        [InitializeOnLoadMethod]
        static void Init()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            NetworkManager.onAnyServerConnectionState += OnConnectionStateChanged;
            NetworkManager.onAnyClientConnectionState += OnConnectionStateChanged;
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

            GUILayout.BeginHorizontal("helpbox");
            GUILayout.Label("PurrNet", GUILayout.ExpandWidth(false));
            GUILayout.Label("Server", GUILayout.ExpandWidth(false));
            TransportInspector.DrawLed(manager ? manager.serverState : null);

            GUILayout.Label("Client", GUILayout.ExpandWidth(false));
            TransportInspector.DrawLed(manager ? manager.clientState : null);
            GUILayout.EndHorizontal();

            if (_extraDraws > 0)
            {
                _extraDraws--;
                ToolbarExtender.RequestToolbarRepaint();
            }
        }
    }
}
