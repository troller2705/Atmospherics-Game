#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Atmospherics.Voxel;

namespace Atmospherics.Editor
{
    public class RebuildVoxelScene : EditorWindow
    {
        [MenuItem("Atmospherics/Rebuild Voxel Test Scene")]
        public static void ShowWindow()
        {
            if (EditorUtility.DisplayDialog("Rebuild Voxel Scene", 
                "This will delete existing voxel systems and create a new test scene.\n\nContinue?", 
                "Yes", "Cancel"))
            {
                RebuildScene();
            }
        }

        private static void RebuildScene()
        {
            VoxelAtmosphericBridge[] existing = FindObjectsByType<VoxelAtmosphericBridge>(FindObjectsSortMode.None);
            foreach (var atomosBridge in existing)
            {
                if (atomosBridge.gameObject.name.Contains("Voxel Test Scene"))
                {
                    DestroyImmediate(atomosBridge.gameObject);
                }
            }

            GameObject systemObj = new GameObject("Voxel Test Scene");

            VoxelAtmosphericBridge bridge = systemObj.AddComponent<VoxelAtmosphericBridge>();
            bridge.gridSize = new Vector3Int(10, 5, 10);
            bridge.voxelSize = 2f;
            bridge.volumePerVoxel = 8f;
            bridge.autoCreateNodes = true;
            bridge.showDebugGizmos = true;

            VoxelVisualizer visualizer = systemObj.AddComponent<VoxelVisualizer>();
            visualizer.bridge = bridge;
            visualizer.showSolidVoxels = true;
            visualizer.showEmptyVoxels = false;

            VoxelRuntimeEditor editor = systemObj.AddComponent<VoxelRuntimeEditor>();
            editor.bridge = bridge;
            editor.showOnScreenInfo = true;

            VoxelTerraformingIntegration terraform = systemObj.AddComponent<VoxelTerraformingIntegration>();
            terraform.atmosphericBridge = bridge;
            editor.terraforming = terraform;

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

            Selection.activeGameObject = systemObj;
            EditorUtility.SetDirty(systemObj);

            EditorUtility.DisplayDialog("Voxel Scene Rebuilt!", 
                "Test scene created with:\n\n" +
                "• Two rooms (3x3x3 voxels)\n" +
                "• Connected by hallway\n" +
                "• Voxel visualizer enabled\n" +
                "• Runtime editor enabled\n\n" +
                "Enter Play mode to see it!", 
                "OK");

            Debug.Log("Voxel test scene rebuilt successfully!");
        }
    }
}
#endif
