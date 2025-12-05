#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Atmospherics.Voxel;

namespace Atmospherics.Editor
{
    public class VoxelIntegrationSetup : EditorWindow
    {
        private Vector3Int gridSize = new Vector3Int(20, 10, 20);
        private float voxelSize = 1f;
        private float volumePerVoxel = 1f;
        private bool enableDynamicUpdates = true;
        private bool enableTerraforming = true;

        [MenuItem("Atmospherics/Voxel Integration Setup")]
        public static void ShowWindow()
        {
            GetWindow<VoxelIntegrationSetup>("Voxel Integration Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Voxel Integration Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Grid Configuration", EditorStyles.boldLabel);
            gridSize = EditorGUILayout.Vector3IntField("Grid Size", gridSize);
            voxelSize = EditorGUILayout.FloatField("Voxel Size (m)", voxelSize);
            volumePerVoxel = EditorGUILayout.FloatField("Volume per Voxel (m³)", volumePerVoxel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Features", EditorStyles.boldLabel);
            enableDynamicUpdates = EditorGUILayout.Toggle("Dynamic Updates", enableDynamicUpdates);
            enableTerraforming = EditorGUILayout.Toggle("Terraforming Integration", enableTerraforming);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Voxel Atmospheric System", GUILayout.Height(40)))
            {
                CreateVoxelSystem();
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This will create a voxel-atmospheric bridge system that automatically:\n" +
                "• Creates atmospheric zones from connected voxels\n" +
                "• Manages gas flow between zones\n" +
                "• Handles voxel destruction and creation\n" +
                "• Integrates with terraforming systems",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Test Scene"))
            {
                CreateTestScene();
            }
        }

        private void CreateVoxelSystem()
        {
            GameObject systemObj = new GameObject("Voxel Atmospheric System");

            VoxelAtmosphericBridge bridge = systemObj.AddComponent<VoxelAtmosphericBridge>();
            bridge.gridSize = gridSize;
            bridge.voxelSize = voxelSize;
            bridge.volumePerVoxel = volumePerVoxel;
            bridge.dynamicUpdates = enableDynamicUpdates;
            bridge.autoCreateNodes = true;
            bridge.showDebugGizmos = true;

            if (enableTerraforming)
            {
                VoxelTerraformingIntegration terraform = systemObj.AddComponent<VoxelTerraformingIntegration>();
                terraform.atmosphericBridge = bridge;
                terraform.enableAtmosphericLeaks = true;
                terraform.releaseGasOnVoxelDestruction = true;
            }

            Selection.activeGameObject = systemObj;
            EditorUtility.SetDirty(systemObj);

            Debug.Log("Voxel Atmospheric System created!");
            EditorUtility.DisplayDialog(
                "Success",
                $"Voxel Atmospheric System created!\n\n" +
                $"Grid Size: {gridSize}\n" +
                $"Voxel Size: {voxelSize}m\n" +
                $"Total Volume: {gridSize.x * gridSize.y * gridSize.z * volumePerVoxel}m³\n\n" +
                "Use the VoxelAtmosphericBridge component to set voxels and build zones.",
                "OK"
            );
        }

        private void CreateTestScene()
        {
            GameObject systemObj = new GameObject("Voxel Test Scene");

            VoxelAtmosphericBridge bridge = systemObj.AddComponent<VoxelAtmosphericBridge>();
            bridge.gridSize = new Vector3Int(10, 5, 10);
            bridge.voxelSize = 2f;
            bridge.volumePerVoxel = 8f;
            bridge.autoCreateNodes = false;
            bridge.showDebugGizmos = true;

            VoxelVisualizer visualizer = systemObj.AddComponent<VoxelVisualizer>();
            visualizer.bridge = bridge;
            visualizer.showSolidVoxels = true;
            visualizer.showEmptyVoxels = false;

            VoxelRuntimeEditor editor = systemObj.AddComponent<VoxelRuntimeEditor>();
            editor.bridge = bridge;
            editor.showOnScreenInfo = true;

            if (enableTerraforming)
            {
                VoxelTerraformingIntegration terraform = systemObj.AddComponent<VoxelTerraformingIntegration>();
                terraform.atmosphericBridge = bridge;
                editor.terraforming = terraform;
            }

            for (int x = 0; x < 10; x++)
            {
                for (int z = 0; z < 10; z++)
                {
                    bridge.SetVoxel(x, 0, z, VoxelAtmosphericBridge.VoxelType.Solid);
                    bridge.SetVoxel(x, 4, z, VoxelAtmosphericBridge.VoxelType.Solid);
                }
            }

            for (int y = 1; y <= 3; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    bridge.SetVoxel(x, y, 0, VoxelAtmosphericBridge.VoxelType.Solid);
                    bridge.SetVoxel(x, y, 9, VoxelAtmosphericBridge.VoxelType.Solid);
                }
                for (int z = 1; z < 9; z++)
                {
                    bridge.SetVoxel(0, y, z, VoxelAtmosphericBridge.VoxelType.Solid);
                    bridge.SetVoxel(9, y, z, VoxelAtmosphericBridge.VoxelType.Solid);
                }
            }

            for (int x = 1; x < 4; x++)
            {
                for (int y = 1; y <= 3; y++)
                {
                    for (int z = 1; z < 4; z++)
                    {
                        bridge.SetVoxel(x, y, z, VoxelAtmosphericBridge.VoxelType.Empty);
                    }
                }
            }

            for (int x = 6; x < 9; x++)
            {
                for (int y = 1; y <= 3; y++)
                {
                    for (int z = 6; z < 9; z++)
                    {
                        bridge.SetVoxel(x, y, z, VoxelAtmosphericBridge.VoxelType.Empty);
                    }
                }
            }

            bridge.SetVoxel(5, 1, 5, VoxelAtmosphericBridge.VoxelType.Empty);
            bridge.SetVoxel(5, 2, 5, VoxelAtmosphericBridge.VoxelType.Empty);

            bridge.RebuildAtmosphericZones();

            Selection.activeGameObject = systemObj;
            EditorUtility.SetDirty(systemObj);

            Debug.Log("Voxel test scene created with two rooms connected by a hallway!");
            EditorUtility.DisplayDialog(
                "Test Scene Created",
                "Voxel test scene created!\n\n" +
                "• Two sealed rooms (3x3x3 voxels each)\n" +
                "• Connected by a 1x1 hallway\n" +
                "• Automatic atmospheric zones\n\n" +
                "Enter Play mode to see atmospheric simulation.\n" +
                "Try modifying voxels at runtime!",
                "OK"
            );
        }
    }
}
#endif
