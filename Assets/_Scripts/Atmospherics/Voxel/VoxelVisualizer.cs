using UnityEngine;
using System.Collections.Generic;

namespace Atmospherics.Voxel
{
    [RequireComponent(typeof(VoxelAtmosphericBridge))]
    public class VoxelVisualizer : MonoBehaviour
    {
        [Header("References")]
        public VoxelAtmosphericBridge bridge;

        [Header("Visualization Settings")]
        public bool showSolidVoxels = true;
        public bool showEmptyVoxels = false;
        public bool showPartialVoxels = true;

        [Header("Colors")]
        public Color solidColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        public Color emptyColor = new Color(0, 1, 0, 0.1f);
        public Color partialColor = new Color(1, 1, 0, 0.5f);

        [Header("Refresh")]
        public bool autoRefresh = false;
        public float refreshInterval = 2f;

        private GameObject voxelContainer;
        private float refreshTimer;

        private void Awake()
        {
            if (bridge == null)
            {
                bridge = GetComponent<VoxelAtmosphericBridge>();
            }
        }

        private void Start()
        {
            if (bridge == null)
            {
                Debug.LogError("VoxelVisualizer: No VoxelAtmosphericBridge found!");
                enabled = false;
                return;
            }

            Invoke(nameof(DelayedRebuild), 0.5f);
        }

        private void DelayedRebuild()
        {
            RebuildVisualization();
        }

        private void Update()
        {
            if (!autoRefresh) return;

            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshInterval)
            {
                RebuildVisualization();
                refreshTimer = 0f;
            }
        }

        public void RebuildVisualization()
        {
            ClearVisualization();

            voxelContainer = new GameObject("Voxel Meshes");
            voxelContainer.transform.SetParent(transform);
            voxelContainer.transform.localPosition = Vector3.zero;

            int voxelCount = 0;

            foreach (var kvp in bridge.VoxelGrid)
            {
                Vector3Int pos = kvp.Key;
                var voxelData = kvp.Value;

                bool shouldShow = false;
                Color color = Color.white;

                switch (voxelData.type)
                {
                    case VoxelAtmosphericBridge.VoxelType.Solid:
                        shouldShow = showSolidVoxels;
                        color = solidColor;
                        break;
                    case VoxelAtmosphericBridge.VoxelType.Empty:
                        shouldShow = showEmptyVoxels;
                        color = emptyColor;
                        break;
                    case VoxelAtmosphericBridge.VoxelType.Partial:
                        shouldShow = showPartialVoxels;
                        color = new Color(partialColor.r, partialColor.g, partialColor.b, partialColor.a * voxelData.density);
                        break;
                }

                if (shouldShow)
                {
                    CreateVoxelMesh(pos, color);
                    voxelCount++;
                }
            }

            Debug.Log($"VoxelVisualizer: Created {voxelCount} voxel meshes");
        }

        private void CreateVoxelMesh(Vector3Int voxelPos, Color color)
        {
            GameObject voxelObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            voxelObj.name = $"Voxel_{voxelPos.x}_{voxelPos.y}_{voxelPos.z}";
            voxelObj.transform.SetParent(voxelContainer.transform);

            Vector3 worldPos = bridge.VoxelToWorldPosition(voxelPos);
            voxelObj.transform.position = worldPos + Vector3.one * bridge.voxelSize * 0.5f;
            voxelObj.transform.localScale = Vector3.one * bridge.voxelSize * 0.95f;

            MeshRenderer renderer = voxelObj.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            mat.color = color;
            
            renderer.material = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void ClearVisualization()
        {
            if (voxelContainer != null)
            {
                DestroyImmediate(voxelContainer);
            }
        }

        private void OnDestroy()
        {
            ClearVisualization();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && voxelContainer != null)
            {
                RebuildVisualization();
            }
        }
    }
}
