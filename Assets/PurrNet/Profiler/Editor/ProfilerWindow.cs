using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PurrNet.Profiler.Editor
{
    public class ProfilerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private Vector2 graphScrollPosition;

        // Graph settings
        private int selectedSampleIndex = -1;
        private int hoveredSampleIndex = -1; // Track which sample is being hovered
        private const float graphHeight = 200f;
        private const float barWidth = 20f; // Constant width for each bar
        private const float labelWidth = 50f; // Width for the labels on the left side

        // Graph data
        private readonly List<float> receivedRpcData = new List<float>();
        private readonly List<float> sentRpcData = new List<float>();
        private readonly List<float> receivedBroadcastData = new List<float>();
        private readonly List<float> sentBroadcastData = new List<float>();
        private readonly List<float> forwardedBytesData = new List<float>();

        [MenuItem("Tools/PurrNet/Profiler")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProfilerWindow>("PurrNet Profiler");
            window.Show();
        }

        void OnEnable()
        {
            // Subscribe to the onSampleEnded event to refresh the GUI
            Statistics.onSampleEnded += OnSampleEnded;
        }

        void OnDisable()
        {
            // Unsubscribe from the event when the window is closed
            Statistics.onSampleEnded -= OnSampleEnded;
        }

        private void OnSampleEnded()
        {
            // Refresh the GUI when a new sample is added
            Repaint();
        }

        void OnGUI()
        {
            GUILayout.Label("PurrNet Profiler", EditorStyles.boldLabel);

            // Add button row
            EditorGUILayout.BeginHorizontal();

            // Add record/stop button
            if (GUILayout.Button(Statistics.paused ? "Start Recording" : "Stop Recording"))
            {
                Statistics.paused = !Statistics.paused;
                Repaint();
            }

            // Add clear button
            if (GUILayout.Button("Clear"))
            {
                Statistics.samples.Clear();
                selectedSampleIndex = -1;
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            var samples = Statistics.samples;

            // Update graph data
            UpdateGraphData();

            // Always draw the graph
            DrawGraph();

            // Draw details view if a sample is selected
            if (selectedSampleIndex >= 0 && selectedSampleIndex < samples.Count)
            {
                DrawSampleDetails(samples[selectedSampleIndex]);
            }
            else if (samples.Count > 0)
            {
                int idx = samples.Count - 1;
                DrawSample(samples[idx], idx);
            }
        }

        private void UpdateGraphData()
        {
            // Clear existing data
            receivedRpcData.Clear();
            sentRpcData.Clear();
            receivedBroadcastData.Clear();
            sentBroadcastData.Clear();
            forwardedBytesData.Clear();

            // Add data from each sample
            foreach (var sample in Statistics.samples)
            {
                receivedRpcData.Add(sample.receivedRpcs.Sum(rpc => rpc.data));
                sentRpcData.Add(sample.sentRpcs.Sum(rpc => rpc.data));
                receivedBroadcastData.Add(sample.receivedBroadcasts.Sum(b => b.data));
                sentBroadcastData.Add(sample.sentBroadcasts.Sum(b => b.data));
                forwardedBytesData.Add(sample.forwardedBytes.Sum());
            }

            bool isInPlayMode = EditorApplication.isPlaying;
            bool isPaused = EditorApplication.isPaused;
            // Auto-scroll to the end when new data is added and actively recording
            if (receivedRpcData.Count > 0 && isInPlayMode && !isPaused)
            {
                float totalWidth = receivedRpcData.Count * barWidth;
                graphScrollPosition.x = totalWidth;
            }
        }

        private void DrawGraph()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Network Traffic Graph", EditorStyles.boldLabel);

            // Get the maximum value for scaling
            float maxValue = 1f; // Avoid division by zero
            if (receivedRpcData.Count > 0)
            {
                // Calculate the maximum total height for each bar
                for (int i = 0; i < receivedRpcData.Count; i++)
                {
                    float totalBarHeight = receivedRpcData[i] + 
                                          sentRpcData[i] + 
                                          receivedBroadcastData[i] + 
                                          sentBroadcastData[i] + 
                                          forwardedBytesData[i];
                    maxValue = Math.Max(maxValue, totalBarHeight);
                }
            }

            // Calculate the total width needed for all bars
            float totalWidth = receivedRpcData.Count * barWidth;

            // Create a horizontal layout for the graph and labels
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);

            // Draw the Y-axis labels
            EditorGUILayout.BeginVertical(GUILayout.Width(labelWidth));

            // Draw value labels on the left side
            for (int i = 4; i >= 0; i--)
            {
                float value = maxValue * i / 4;
                string label = FormatBytes(value);
                EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth), GUILayout.Height(graphHeight / 5));
            }

            EditorGUILayout.EndVertical();

            // Create a scroll view for the graph
            graphScrollPosition = EditorGUILayout.BeginScrollView(graphScrollPosition, GUILayout.Height(graphHeight + 20));

            // Draw the graph background
            var graphRect = GUILayoutUtility.GetRect(totalWidth, graphHeight);

            // Draw background
            EditorGUI.DrawRect(graphRect, new Color(0.2f, 0.2f, 0.2f, 1));

            // Draw grid lines
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 1);
            float gridSegmentHeight = graphRect.height / 5;
            for (int i = 0; i <= 5; i++)
            {
                float y = graphRect.y + (graphRect.height - i * gridSegmentHeight);
                Handles.DrawLine(
                    new Vector3(graphRect.x, y, 0),
                    new Vector3(graphRect.x + graphRect.width, y, 0)
                );
            }

            // Draw data points
            if (receivedRpcData.Count > 0)
            {
                const float spacing = 2f;
                hoveredSampleIndex = -1; // Reset hover index at the start of drawing

                for (int i = 0; i < receivedRpcData.Count; i++)
                {
                    float x = graphRect.x + (i * barWidth + i * spacing);
                    float currentY = graphRect.y + graphRect.height;

                    // Create a click/hover rect for the entire bar
                    var barRect = new Rect(x, graphRect.y, barWidth, graphRect.height);

                    // Check for hover
                    if (barRect.Contains(Event.current.mousePosition))
                    {
                        hoveredSampleIndex = i;

                        // Show tooltip with sample information
                        if (i >= 0 && i < Statistics.samples.Count)
                        {
                            var sample = Statistics.samples[i];
                            string tooltip = $"Frame {i}\n" +
                                            $"Received RPCs: {FormatBytes(sample.receivedRpcs.Sum(rpc => rpc.data))}\n" +
                                            $"Sent RPCs: {FormatBytes(sample.sentRpcs.Sum(rpc => rpc.data))}\n" +
                                            $"Received Broadcasts: {FormatBytes(sample.receivedBroadcasts.Sum(b => b.data))}\n" +
                                            $"Sent Broadcasts: {FormatBytes(sample.sentBroadcasts.Sum(b => b.data))}\n" +
                                            $"Forwarded: {FormatBytes(sample.forwardedBytes.Sum())}";

                            GUI.tooltip = tooltip;
                        }

                        Repaint(); // Repaint to update hover effect
                    }

                    // Determine if this bar is selected or hovered
                    bool isSelected = (i == selectedSampleIndex);
                    bool isHovered = (i == hoveredSampleIndex);

                    // Draw a highlight for selected or hovered bars
                    if (isSelected || isHovered)
                    {
                        var highlightColor = isSelected ?
                            new Color(1f, 1f, 1f, 0.3f) : // White for selected
                            new Color(0.8f, 0.8f, 0.8f, 0.2f); // Light gray for hovered

                        EditorGUI.DrawRect(barRect, highlightColor);
                    }

                    // Draw received RPCs
                    float height = (receivedRpcData[i] / maxValue) * graphRect.height;
                    EditorGUI.DrawRect(
                        new Rect(x, currentY - height, barWidth, height),
                        new Color(0.2f, 0.8f, 0.2f, 0.8f)
                    );
                    currentY -= height;

                    // Draw sent RPCs
                    height = (sentRpcData[i] / maxValue) * graphRect.height;
                    EditorGUI.DrawRect(
                        new Rect(x, currentY - height, barWidth, height),
                        new Color(0.8f, 0.2f, 0.2f, 0.8f)
                    );
                    currentY -= height;

                    // Draw received broadcasts
                    height = (receivedBroadcastData[i] / maxValue) * graphRect.height;
                    EditorGUI.DrawRect(
                        new Rect(x, currentY - height, barWidth, height),
                        new Color(0.2f, 0.2f, 0.8f, 0.8f)
                    );
                    currentY -= height;

                    // Draw sent broadcasts
                    height = (sentBroadcastData[i] / maxValue) * graphRect.height;
                    EditorGUI.DrawRect(
                        new Rect(x, currentY - height, barWidth, height),
                        new Color(0.8f, 0.8f, 0.2f, 0.8f)
                    );
                    currentY -= height;

                    // Draw forwarded bytes
                    height = (forwardedBytesData[i] / maxValue) * graphRect.height;
                    EditorGUI.DrawRect(
                        new Rect(x, currentY - height, barWidth, height),
                        new Color(0.8f, 0.2f, 0.8f, 0.8f)
                    );

                    // Handle click to select sample
                    if (Event.current.type == EventType.MouseDown && barRect.Contains(Event.current.mousePosition))
                    {
                        // Toggle selection - if already selected, deselect it
                        if (selectedSampleIndex == i)
                        {
                            selectedSampleIndex = -1;
                        }
                        else
                        {
                            selectedSampleIndex = i;

                            // Pause the editor if in play mode
                            if (Application.isPlaying)
                            {
                                EditorApplication.isPaused = true;
                            }
                        }
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Draw legend at the bottom of the graph
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            GUILayout.FlexibleSpace();

            // Draw each legend item
            DrawLegendItem("Received RPCs", new Color(0.2f, 0.8f, 0.2f, 0.8f));
            DrawLegendItem("Sent RPCs", new Color(0.8f, 0.2f, 0.2f, 0.8f));
            DrawLegendItem("Received Broadcasts", new Color(0.2f, 0.2f, 0.8f, 0.8f));
            DrawLegendItem("Sent Broadcasts", new Color(0.8f, 0.8f, 0.2f, 0.8f));
            DrawLegendItem("Forwarded Bytes", new Color(0.8f, 0.2f, 0.8f, 0.8f));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private string FormatBytes(float bytes)
        {
            if (bytes < 1024)
                return $"{bytes:F0}B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024:F1}KB";
            else if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024 * 1024):F1}MB";
            else
                return $"{bytes / (1024 * 1024 * 1024):F1}GB";
        }

        private void DrawLegendItem(string label, Color color)
        {
            var colorRect = GUILayoutUtility.GetRect(15, 15, GUILayout.ExpandWidth(false));
            GUILayout.Space(5);
            EditorGUI.DrawRect(colorRect, color);
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Space(5);
        }

        private Vector2 detailsScrollPosition;

        // Dictionary to store foldout states
        private readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        // Helper method to get foldout state
        private bool GetFoldoutState(string key)
        {
            if (foldoutStates.TryAdd(key, false))
                return false;
            return foldoutStates[key];
        }

        // Helper method to set foldout state
        private void SetFoldoutState(string key, bool value)
        {
            foldoutStates[key] = value;
        }

        private void DrawSampleDetails(TickSample sample)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sample Details", EditorStyles.boldLabel);
            if (GUILayout.Button("Back to List", GUILayout.Width(100)))
            {
                selectedSampleIndex = -1;
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            // Create a scroll view for the sample details
            try
            {
                detailsScrollPosition = EditorGUILayout.BeginScrollView(detailsScrollPosition, false, true);

                if (sample.receivedRpcs.Count > 0)
                {
                    EditorGUILayout.LabelField("Received RPCs", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    // Aggregate received RPCs by type and method
                    var aggregatedReceivedRpcs = sample.receivedRpcs
                        .GroupBy(rpc => new { rpc.type, rpc.method })
                        .Select(group => new
                        {
                            Type = group.Key.type,
                            Method = group.Key.method,
                            Count = group.Count(),
                            TotalBytes = group.Sum(rpc => rpc.data),
                            Items = group.ToList()
                        })
                        .OrderByDescending(rpc => rpc.TotalBytes);

                    foreach (var rpcGroup in aggregatedReceivedRpcs)
                    {
                        // Create a foldout for each RPC group
                        string label = $"{rpcGroup.Type.Name}.{rpcGroup.Method} ({FormatBytes(rpcGroup.TotalBytes)}) - {rpcGroup.Count} calls";
                        bool expanded = EditorGUILayout.Foldout(GetFoldoutState($"received_{rpcGroup.Type.Name}_{rpcGroup.Method}"), label);
                        SetFoldoutState($"received_{rpcGroup.Type.Name}_{rpcGroup.Method}", expanded);

                        if (expanded)
                        {
                            EditorGUI.indentLevel++;
                            foreach (var rpc in rpcGroup.Items)
                            {
                                EditorGUILayout.BeginHorizontal();
                                
                                // Add a button to ping the context object if it exists
                                if (rpc.context != null)
                                    EditorGUILayout.ObjectField(rpc.context, typeof(UnityEngine.Object), true);
                                
                                EditorGUILayout.LabelField($"{FormatBytes(rpc.data)} bytes");
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                if (sample.sentRpcs.Count > 0)
                {
                    EditorGUILayout.LabelField("Sent RPCs", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    // Aggregate sent RPCs by type and method
                    var aggregatedSentRpcs = sample.sentRpcs
                        .GroupBy(rpc => new { rpc.type, rpc.method })
                        .Select(group => new
                        {
                            Type = group.Key.type,
                            Method = group.Key.method,
                            Count = group.Count(),
                            TotalBytes = group.Sum(rpc => rpc.data),
                            Items = group.ToList()
                        })
                        .OrderByDescending(rpc => rpc.TotalBytes);

                    foreach (var rpcGroup in aggregatedSentRpcs)
                    {
                        // Create a foldout for each RPC group
                        string label = $"{rpcGroup.Type.Name}.{rpcGroup.Method} ({FormatBytes(rpcGroup.TotalBytes)}) - {rpcGroup.Count} calls";
                        bool expanded = EditorGUILayout.Foldout(GetFoldoutState($"sent_{rpcGroup.Type.Name}_{rpcGroup.Method}"), label);
                        SetFoldoutState($"sent_{rpcGroup.Type.Name}_{rpcGroup.Method}", expanded);

                        if (expanded)
                        {
                            EditorGUI.indentLevel++;
                            foreach (var rpc in rpcGroup.Items)
                            {
                                EditorGUILayout.BeginHorizontal();
                                
                                // Add a button to ping the context object if it exists
                                if (rpc.context != null)
                                    EditorGUILayout.ObjectField(rpc.context, typeof(UnityEngine.Object), true);
                                
                                EditorGUILayout.LabelField($"{FormatBytes(rpc.data)} bytes");
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                if (sample.receivedBroadcasts.Count > 0)
                {
                    EditorGUILayout.LabelField("Received Broadcasts", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    // Aggregate received broadcasts by type
                    var aggregatedReceivedBroadcasts = sample.receivedBroadcasts
                        .GroupBy(broadcast => broadcast.type)
                        .Select(group => new
                        {
                            Type = group.Key,
                            Count = group.Count(),
                            TotalBytes = group.Sum(broadcast => broadcast.data),
                            Items = group.ToList()
                        })
                        .OrderByDescending(broadcast => broadcast.TotalBytes);

                    foreach (var broadcastGroup in aggregatedReceivedBroadcasts)
                    {
                        // Create a foldout for each broadcast group
                        string label = $"{broadcastGroup.Type.Name} ({FormatBytes(broadcastGroup.TotalBytes)}) - {broadcastGroup.Count} broadcasts";
                        bool expanded = EditorGUILayout.Foldout(GetFoldoutState($"received_broadcast_{broadcastGroup.Type.Name}"), label);
                        SetFoldoutState($"received_broadcast_{broadcastGroup.Type.Name}", expanded);

                        if (expanded)
                        {
                            EditorGUI.indentLevel++;
                            foreach (var broadcast in broadcastGroup.Items)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{FormatBytes(broadcast.data)} bytes");
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                if (sample.sentBroadcasts.Count > 0)
                {
                    EditorGUILayout.LabelField("Sent Broadcasts", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    // Aggregate sent broadcasts by type
                    var aggregatedSentBroadcasts = sample.sentBroadcasts
                        .GroupBy(broadcast => broadcast.type)
                        .Select(group => new
                        {
                            Type = group.Key,
                            Count = group.Count(),
                            TotalBytes = group.Sum(broadcast => broadcast.data),
                            Items = group.ToList()
                        })
                        .OrderByDescending(broadcast => broadcast.TotalBytes);

                    foreach (var broadcastGroup in aggregatedSentBroadcasts)
                    {
                        // Create a foldout for each broadcast group
                        string label = $"{broadcastGroup.Type.Name} ({FormatBytes(broadcastGroup.TotalBytes)}) - {broadcastGroup.Count} broadcasts";
                        bool expanded = EditorGUILayout.Foldout(GetFoldoutState($"sent_broadcast_{broadcastGroup.Type.Name}"), label);
                        SetFoldoutState($"sent_broadcast_{broadcastGroup.Type.Name}", expanded);

                        if (expanded)
                        {
                            EditorGUI.indentLevel++;
                            foreach (var broadcast in broadcastGroup.Items)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{FormatBytes(broadcast.data)} bytes");
                                EditorGUILayout.EndHorizontal();
                            }
                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                // Draw Forwarded Bytes
                if (sample.forwardedBytes.Count > 0)
                {
                    EditorGUILayout.LabelField("Forwarded Bytes", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    int totalBytes = sample.forwardedBytes.Sum();
                    EditorGUILayout.LabelField($"Total: {FormatBytes(totalBytes)}");
                    EditorGUILayout.LabelField($"Count: {sample.forwardedBytes.Count} packets");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndScrollView();
            }
            catch
            {
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSample(TickSample sample, int index)
        {
            // Determine if this sample is selected
            bool isSelected = (index == selectedSampleIndex);

            // Use a different style for the selected sample
            var boxStyle = isSelected ?
                new GUIStyle(EditorStyles.helpBox) { normal = { background = EditorGUIUtility.whiteTexture }, border = new RectOffset(2, 2, 2, 2) } :
                EditorStyles.helpBox;

            EditorGUILayout.BeginVertical(boxStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sample", EditorStyles.boldLabel);
            if (GUILayout.Button("View Details", GUILayout.Width(100)))
            {
                selectedSampleIndex = index;
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            // Draw summary
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"RPCs: {sample.receivedRpcs.Count} received, {sample.sentRpcs.Count} sent");
            EditorGUILayout.LabelField($"Broadcasts: {sample.receivedBroadcasts.Count} received, {sample.sentBroadcasts.Count} sent");
            EditorGUILayout.LabelField($"Forwarded: {FormatBytes(sample.forwardedBytes.Sum())} in {sample.forwardedBytes.Count} packets");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}
