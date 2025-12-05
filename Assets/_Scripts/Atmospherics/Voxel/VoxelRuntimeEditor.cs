using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Voxel
{
    public class VoxelRuntimeEditor : MonoBehaviour
    {
        [Header("References")]
        public VoxelAtmosphericBridge bridge;
        public VoxelTerraformingIntegration terraforming;

        [Header("Input Settings")]
        public KeyCode destroyKey = KeyCode.Mouse0;
        public KeyCode createKey = KeyCode.Mouse1;
        public float raycastDistance = 100f;

        [Header("Edit Settings")]
        public VoxelAtmosphericBridge.VoxelType createType = VoxelAtmosphericBridge.VoxelType.Solid;
        public bool showTargetVoxel = true;
        public Color targetVoxelColor = Color.yellow;
        public Color createPreviewColor = Color.green;

        [Header("UI")]
        public bool showOnScreenInfo = true;
        public int infoFontSize = 14;

        private Vector3Int? targetVoxel = null;
        private Vector3Int? createTargetVoxel = null;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;

            if (bridge == null)
            {
                bridge = GetComponent<VoxelAtmosphericBridge>();
            }

            if (terraforming == null)
            {
                terraforming = GetComponent<VoxelTerraformingIntegration>();
            }

            if (bridge == null)
            {
                Debug.LogWarning("VoxelRuntimeEditor: No VoxelAtmosphericBridge found!");
                enabled = false;
            }
        }

        private void Update()
        {
            UpdateTargetVoxel();
            HandleInput();
        }

        private void UpdateTargetVoxel()
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            Vector3Int? hitVoxel;
            Vector3 hitNormal;
            RaycastVoxelGrid(ray, out hitVoxel, out hitNormal);
            
            targetVoxel = hitVoxel;
            
            if (hitVoxel.HasValue)
            {
                Vector3Int adjacentPos = hitVoxel.Value + Vector3Int.RoundToInt(hitNormal);
                
                if (IsValidVoxelPosition(adjacentPos))
                {
                    var adjacentVoxel = bridge.GetVoxel(adjacentPos);
                    if (adjacentVoxel.type == VoxelAtmosphericBridge.VoxelType.Empty)
                    {
                        createTargetVoxel = adjacentPos;
                    }
                    else
                    {
                        createTargetVoxel = null;
                    }
                }
                else
                {
                    createTargetVoxel = null;
                }
            }
            else
            {
                createTargetVoxel = null;
            }
        }

        private void RaycastVoxelGrid(Ray ray, out Vector3Int? hitVoxel, out Vector3 hitNormal)
        {
            hitVoxel = null;
            hitNormal = Vector3.zero;
            
            float maxDistance = raycastDistance;
            float step = bridge.voxelSize * 0.25f;
            Vector3 previousPoint = ray.origin;
            
            for (float distance = 0; distance < maxDistance; distance += step)
            {
                Vector3 point = ray.origin + ray.direction * distance;
                Vector3Int voxelPos = bridge.WorldToVoxelPosition(point);
                
                if (IsValidVoxelPosition(voxelPos))
                {
                    var voxelData = bridge.GetVoxel(voxelPos);
                    if (voxelData.type == VoxelAtmosphericBridge.VoxelType.Solid)
                    {
                        hitVoxel = voxelPos;
                        
                        Vector3 voxelCenter = bridge.VoxelToWorldPosition(voxelPos) + Vector3.one * bridge.voxelSize * 0.5f;
                        Vector3 directionToHit = (previousPoint - voxelCenter).normalized;
                        
                        float maxComponent = Mathf.Max(
                            Mathf.Abs(directionToHit.x),
                            Mathf.Abs(directionToHit.y),
                            Mathf.Abs(directionToHit.z)
                        );
                        
                        if (Mathf.Abs(directionToHit.x) == maxComponent)
                            hitNormal = new Vector3(Mathf.Sign(directionToHit.x), 0, 0);
                        else if (Mathf.Abs(directionToHit.y) == maxComponent)
                            hitNormal = new Vector3(0, Mathf.Sign(directionToHit.y), 0);
                        else
                            hitNormal = new Vector3(0, 0, Mathf.Sign(directionToHit.z));
                        
                        return;
                    }
                }
                
                previousPoint = point;
            }
        }

        private bool IsValidVoxelPosition(Vector3Int pos)
        {
            return pos.x >= 0 && pos.x < bridge.gridSize.x &&
                   pos.y >= 0 && pos.y < bridge.gridSize.y &&
                   pos.z >= 0 && pos.z < bridge.gridSize.z;
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(destroyKey) && targetVoxel.HasValue)
            {
                DestroyVoxel(targetVoxel.Value);
            }

            if (Input.GetKeyDown(createKey) && createTargetVoxel.HasValue)
            {
                CreateVoxel(createTargetVoxel.Value);
            }
        }

        private void DestroyVoxel(Vector3Int voxelPos)
        {
            if (terraforming != null)
            {
                terraforming.OnVoxelDestroyed(voxelPos);
            }
            else
            {
                bridge.SetVoxel(voxelPos, VoxelAtmosphericBridge.VoxelType.Empty);
            }

            RefreshVisualization();
            Debug.Log($"Destroyed voxel at {voxelPos}");
        }

        private void CreateVoxel(Vector3Int voxelPos)
        {
            if (terraforming != null)
            {
                terraforming.OnVoxelCreated(voxelPos, createType);
            }
            else
            {
                bridge.SetVoxel(voxelPos, createType);
            }

            RefreshVisualization();
            Debug.Log($"Created {createType} voxel at {voxelPos}");
        }

        private void RefreshVisualization()
        {
            var visualizer = GetComponent<VoxelVisualizer>();
            if (visualizer != null)
            {
                visualizer.RebuildVisualization();
            }
        }

        private void OnGUI()
        {
            if (!showOnScreenInfo) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = infoFontSize;
            style.normal.textColor = Color.white;

            string info = "=== Voxel Editor ===\n";
            info += $"{destroyKey}: Destroy Voxel\n";
            info += $"{createKey}: Create Voxel\n\n";

            if (targetVoxel.HasValue)
            {
                info += $"Target: {targetVoxel.Value}\n";
                var voxelData = bridge.GetVoxel(targetVoxel.Value);
                info += $"Type: {voxelData.type}\n";
                info += $"Density: {voxelData.density:F2}\n";

                AtmosphericNode node = bridge.GetNodeAtVoxel(targetVoxel.Value);
                if (node != null)
                {
                    info += $"\nNode: {node.NodeName}\n";
                    if (node.Mixture != null)
                    {
                        info += $"P: {node.Mixture.GetPressure():F1} kPa\n";
                        info += $"T: {node.Mixture.Temperature - 273.15f:F1}°C\n";
                    }
                }
            }
            else
            {
                info += "No target voxel";
            }

            GUI.Label(new Rect(10, 10, 300, 300), info, style);
        }

        private void OnDrawGizmos()
        {
            if (!showTargetVoxel || bridge == null) return;

            if (targetVoxel.HasValue)
            {
                Gizmos.color = targetVoxelColor;
                Vector3 worldPos = bridge.VoxelToWorldPosition(targetVoxel.Value);
                Gizmos.DrawWireCube(worldPos + Vector3.one * bridge.voxelSize * 0.5f, Vector3.one * bridge.voxelSize);
            }

            if (createTargetVoxel.HasValue)
            {
                Gizmos.color = createPreviewColor;
                Vector3 worldPos = bridge.VoxelToWorldPosition(createTargetVoxel.Value);
                Gizmos.DrawWireCube(worldPos + Vector3.one * bridge.voxelSize * 0.5f, Vector3.one * bridge.voxelSize * 0.9f);
            }
        }
    }
}
