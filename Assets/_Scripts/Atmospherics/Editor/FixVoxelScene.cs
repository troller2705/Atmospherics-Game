#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Atmospherics.Voxel;
using Atmospherics.Core;
using Atmospherics.Visualization;

namespace Atmospherics.Editor
{
    public class FixVoxelScene : EditorWindow
    {
        [MenuItem("Atmospherics/Enable All Visualizations")]
        public static void ShowWindow()
        {
            EnableAllVisualizations();
        }

        private static void EnableAllVisualizations()
        {
            int voxelSystemsFixed = 0;
            int nodesFixed = 0;
            int pipesFixed = 0;

            VoxelAtmosphericBridge[] bridges = FindObjectsByType<VoxelAtmosphericBridge>(FindObjectsSortMode.None);

            foreach (var bridge in bridges)
            {
                GameObject obj = bridge.gameObject;

                if (obj.GetComponent<VoxelVisualizer>() == null)
                {
                    VoxelVisualizer visualizer = obj.AddComponent<VoxelVisualizer>();
                    visualizer.bridge = bridge;
                    visualizer.showSolidVoxels = true;
                    visualizer.showEmptyVoxels = false;
                    visualizer.showPartialVoxels = true;
                    Debug.Log($"Added VoxelVisualizer to {obj.name}");
                }

                if (obj.GetComponent<VoxelRuntimeEditor>() == null)
                {
                    VoxelRuntimeEditor editor = obj.AddComponent<VoxelRuntimeEditor>();
                    editor.bridge = bridge;
                    editor.terraforming = obj.GetComponent<VoxelTerraformingIntegration>();
                    editor.showOnScreenInfo = true;
                    Debug.Log($"Added VoxelRuntimeEditor to {obj.name}");
                }

                EditorUtility.SetDirty(obj);
                voxelSystemsFixed++;
            }

            AtmosphericNode[] nodes = FindObjectsByType<AtmosphericNode>(FindObjectsSortMode.None);
            foreach (var node in nodes)
            {
                if (node.GetComponent<AtmosphericZoneVisualizer>() == null)
                {
                    AtmosphericZoneVisualizer viz = node.gameObject.AddComponent<AtmosphericZoneVisualizer>();
                    viz.showZoneBoundary = true;
                    viz.showPressureColor = true;
                    viz.sphereRadius = 1.5f;
                    EditorUtility.SetDirty(node.gameObject);
                    nodesFixed++;
                }
            }

            Pipe[] pipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);
            foreach (var pipe in pipes)
            {
                if (pipe.GetComponent<PipeVisualizer>() == null)
                {
                    PipeVisualizer viz = pipe.gameObject.AddComponent<PipeVisualizer>();
                    viz.showPipe = true;
                    viz.showFlowDirection = true;
                    viz.pipeThickness = 0.15f;
                    EditorUtility.SetDirty(pipe.gameObject);
                    pipesFixed++;
                }
            }

            string message = $"Visualization components added:\n\n" +
                $"• {voxelSystemsFixed} voxel systems\n" +
                $"• {nodesFixed} atmospheric zones\n" +
                $"• {pipesFixed} pipes\n\n" +
                "Enter Play mode to see 3D meshes!";

            EditorUtility.DisplayDialog("Visualizations Enabled!", message, "OK");
            Debug.Log($"Enabled visualizations: {voxelSystemsFixed} voxel systems, {nodesFixed} nodes, {pipesFixed} pipes");
        }
    }
}
#endif
