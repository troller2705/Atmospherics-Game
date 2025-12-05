using UnityEngine;

namespace Atmospherics.Voxel
{
    [RequireComponent(typeof(VoxelAtmosphericBridge))]
    public class AddVoxelVisualization : MonoBehaviour
    {
        private void Start()
        {
            VoxelAtmosphericBridge bridge = GetComponent<VoxelAtmosphericBridge>();
            
            if (GetComponent<VoxelVisualizer>() == null)
            {
                VoxelVisualizer visualizer = gameObject.AddComponent<VoxelVisualizer>();
                visualizer.bridge = bridge;
                visualizer.showSolidVoxels = true;
                visualizer.showEmptyVoxels = false;
                Debug.Log("Added VoxelVisualizer");
            }

            if (GetComponent<VoxelRuntimeEditor>() == null)
            {
                VoxelRuntimeEditor editor = gameObject.AddComponent<VoxelRuntimeEditor>();
                editor.bridge = bridge;
                editor.terraforming = GetComponent<VoxelTerraformingIntegration>();
                editor.showOnScreenInfo = true;
                Debug.Log("Added VoxelRuntimeEditor");
            }

            Destroy(this);
        }
    }
}
