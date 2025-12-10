using UnityEngine;

namespace Atmospherics.Voxel
{
    public class VoxelVisualizationUI : MonoBehaviour
    {
        [Header("References")]
        public VoxelAtmosphericVisualizer visualizer;
        public VoxelGasFlowEffects flowEffects;

        [Header("UI Settings")]
        public KeyCode toggleUIKey = KeyCode.Tab;
        public KeyCode cycleModeKey = KeyCode.V;
        public KeyCode toggleFlowEffectsKey = KeyCode.F;
        public bool showUI = true;
        public int fontSize = 16;
        public Rect uiPosition = new Rect(10, 100, 300, 400);

        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private bool initialized = false;

        private void Start()
        {
            if (visualizer == null)
            {
                visualizer = GetComponent<VoxelAtmosphericVisualizer>();
            }

            if (flowEffects == null)
            {
                flowEffects = GetComponent<VoxelGasFlowEffects>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleUIKey))
            {
                showUI = !showUI;
            }

            if (Input.GetKeyDown(cycleModeKey) && visualizer != null)
            {
                CycleVisualizationMode();
            }

            if (Input.GetKeyDown(toggleFlowEffectsKey) && flowEffects != null)
            {
                flowEffects.autoDetectFlow = !flowEffects.autoDetectFlow;
                if (!flowEffects.autoDetectFlow)
                {
                    flowEffects.ClearAllEmitters();
                }
            }
        }

        private void InitializeStyles()
        {
            if (initialized) return;

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = fontSize + 4;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = fontSize;
            buttonStyle.normal.textColor = Color.white;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = Color.white;

            initialized = true;
        }

        private void OnGUI()
        {
            if (!showUI) return;

            InitializeStyles();

            GUILayout.BeginArea(uiPosition);
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== Atmosphere Visualization ===", headerStyle);
            GUILayout.Space(10);

            if (visualizer != null)
            {
                GUILayout.Label("Visualization Mode:", labelStyle);
                
                if (GUILayout.Button($"Current: {visualizer.currentMode}", buttonStyle))
                {
                    CycleVisualizationMode();
                }

                GUILayout.Space(5);

                if (GUILayout.Button("None", buttonStyle))
                {
                    visualizer.SetVisualizationMode(VoxelAtmosphericVisualizer.VisualizationMode.None);
                }

                if (GUILayout.Button("Pressure", buttonStyle))
                {
                    visualizer.SetVisualizationMode(VoxelAtmosphericVisualizer.VisualizationMode.Pressure);
                }

                if (GUILayout.Button("Oxygen", buttonStyle))
                {
                    visualizer.SetVisualizationMode(VoxelAtmosphericVisualizer.VisualizationMode.Oxygen);
                }

                if (GUILayout.Button("Temperature", buttonStyle))
                {
                    visualizer.SetVisualizationMode(VoxelAtmosphericVisualizer.VisualizationMode.Temperature);
                }

                if (GUILayout.Button("Breathability", buttonStyle))
                {
                    visualizer.SetVisualizationMode(VoxelAtmosphericVisualizer.VisualizationMode.Breathability);
                }

                GUILayout.Space(10);

                visualizer.showOnlyPassableVoxels = GUILayout.Toggle(
                    visualizer.showOnlyPassableVoxels, 
                    "Show Only Air Voxels", 
                    labelStyle
                );
            }

            GUILayout.Space(10);

            if (flowEffects != null)
            {
                GUILayout.Label("Gas Flow Effects:", labelStyle);
                
                bool newFlowState = GUILayout.Toggle(
                    flowEffects.autoDetectFlow, 
                    flowEffects.autoDetectFlow ? "Enabled" : "Disabled", 
                    buttonStyle
                );

                if (newFlowState != flowEffects.autoDetectFlow)
                {
                    flowEffects.autoDetectFlow = newFlowState;
                    if (!flowEffects.autoDetectFlow)
                    {
                        flowEffects.ClearAllEmitters();
                    }
                }

                GUILayout.Label($"Active Emitters: {flowEffects.maxActiveEmitters}", labelStyle);
            }

            GUILayout.Space(10);

            GUILayout.Label("Controls:", headerStyle);
            GUILayout.Label($"{toggleUIKey}: Toggle UI", labelStyle);
            GUILayout.Label($"{cycleModeKey}: Cycle Mode", labelStyle);
            GUILayout.Label($"{toggleFlowEffectsKey}: Toggle Flow FX", labelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void CycleVisualizationMode()
        {
            if (visualizer == null) return;

            int currentIndex = (int)visualizer.currentMode;
            int maxIndex = System.Enum.GetValues(typeof(VoxelAtmosphericVisualizer.VisualizationMode)).Length;
            int nextIndex = (currentIndex + 1) % maxIndex;

            visualizer.SetVisualizationMode((VoxelAtmosphericVisualizer.VisualizationMode)nextIndex);
        }
    }
}
