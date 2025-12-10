using UnityEngine;
using System.Collections.Generic;
using Atmospherics.Core;

namespace Atmospherics.Voxel
{
    [RequireComponent(typeof(VoxelAtmosphericBridge))]
    public class VoxelAtmosphericVisualizer : MonoBehaviour
    {
        public enum VisualizationMode
        {
            None,
            Pressure,
            Oxygen,
            Temperature,
            Breathability
        }

        [Header("Visualization Settings")]
        public VisualizationMode currentMode = VisualizationMode.None;
        public bool showOnlyPassableVoxels = true;
        public float updateInterval = 0.1f;

        [Header("Pressure Visualization")]
        public float minPressure = 0f;
        public float maxPressure = 200f;
        public Gradient pressureGradient;

        [Header("Oxygen Visualization")]
        public float minOxygenPercent = 0f;
        public float maxOxygenPercent = 30f;
        public Gradient oxygenGradient;

        [Header("Temperature Visualization")]
        public float minTemperature = 200f;
        public float maxTemperature = 350f;
        public Gradient temperatureGradient;

        [Header("Breathability Visualization")]
        public Color breathableColor = new Color(0f, 1f, 0f, 0.3f);
        public Color lowOxygenColor = new Color(1f, 1f, 0f, 0.3f);
        public Color noOxygenColor = new Color(1f, 0f, 0f, 0.3f);
        public float breathableOxygenMin = 19f;
        public float breathableOxygenMax = 23f;

        [Header("Rendering")]
        public Material overlayMaterial;
        public bool useTransparency = true;
        [Range(0f, 1f)]
        public float overlayAlpha = 0.5f;

        private VoxelAtmosphericBridge bridge;
        private Dictionary<Vector3Int, GameObject> overlayVoxels = new Dictionary<Vector3Int, GameObject>();
        private float lastUpdateTime;

        private void Awake()
        {
            bridge = GetComponent<VoxelAtmosphericBridge>();
            InitializeGradients();
        }

        private void Start()
        {
            if (overlayMaterial == null)
            {
                overlayMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                overlayMaterial.SetFloat("_Surface", 1);
                overlayMaterial.SetFloat("_Blend", 0);
                overlayMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                overlayMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                overlayMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                overlayMaterial.SetInt("_ZWrite", 0);
                overlayMaterial.renderQueue = 3000;
            }
        }

        private void InitializeGradients()
        {
            if (pressureGradient == null || pressureGradient.colorKeys.Length == 0)
            {
                pressureGradient = new Gradient();
                pressureGradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.blue, 0f),
                        new GradientColorKey(Color.cyan, 0.25f),
                        new GradientColorKey(Color.green, 0.5f),
                        new GradientColorKey(Color.yellow, 0.75f),
                        new GradientColorKey(Color.red, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(overlayAlpha, 0f),
                        new GradientAlphaKey(overlayAlpha, 1f)
                    }
                );
            }

            if (oxygenGradient == null || oxygenGradient.colorKeys.Length == 0)
            {
                oxygenGradient = new Gradient();
                oxygenGradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.red, 0f),
                        new GradientColorKey(Color.yellow, 0.5f),
                        new GradientColorKey(Color.green, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(overlayAlpha, 0f),
                        new GradientAlphaKey(overlayAlpha, 1f)
                    }
                );
            }

            if (temperatureGradient == null || temperatureGradient.colorKeys.Length == 0)
            {
                temperatureGradient = new Gradient();
                temperatureGradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(new Color(0f, 0.5f, 1f, 1f), 0f),
                        new GradientColorKey(Color.cyan, 0.33f),
                        new GradientColorKey(Color.white, 0.5f),
                        new GradientColorKey(Color.yellow, 0.66f),
                        new GradientColorKey(Color.red, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(overlayAlpha, 0f),
                        new GradientAlphaKey(overlayAlpha, 1f)
                    }
                );
            }
        }

        private void Update()
        {
            if (currentMode == VisualizationMode.None)
            {
                ClearOverlays();
                return;
            }

            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateVisualization();
                lastUpdateTime = Time.time;
            }
        }

        public void UpdateVisualization()
        {
            if (currentMode == VisualizationMode.None)
            {
                ClearOverlays();
                return;
            }

            HashSet<Vector3Int> processedVoxels = new HashSet<Vector3Int>();

            foreach (var kvp in bridge.VoxelGrid)
            {
                Vector3Int voxelPos = kvp.Key;
                VoxelAtmosphericBridge.VoxelData voxelData = kvp.Value;

                if (showOnlyPassableVoxels && !voxelData.IsPassable())
                {
                    RemoveOverlay(voxelPos);
                    continue;
                }

                AtmosphericNode node = bridge.GetNodeAtVoxel(voxelPos);
                if (node == null || node.Mixture == null)
                {
                    RemoveOverlay(voxelPos);
                    continue;
                }

                Color color = GetColorForVoxel(node);
                CreateOrUpdateOverlay(voxelPos, color);
                processedVoxels.Add(voxelPos);
            }

            List<Vector3Int> toRemove = new List<Vector3Int>();
            foreach (var voxelPos in overlayVoxels.Keys)
            {
                if (!processedVoxels.Contains(voxelPos))
                {
                    toRemove.Add(voxelPos);
                }
            }

            foreach (var voxelPos in toRemove)
            {
                RemoveOverlay(voxelPos);
            }
        }

        private Color GetColorForVoxel(AtmosphericNode node)
        {
            switch (currentMode)
            {
                case VisualizationMode.Pressure:
                    return GetPressureColor(node.Pressure);

                case VisualizationMode.Oxygen:
                    return GetOxygenColor(node);

                case VisualizationMode.Temperature:
                    return GetTemperatureColor(node.Mixture.Temperature);

                case VisualizationMode.Breathability:
                    return GetBreathabilityColor(node);

                default:
                    return Color.white;
            }
        }

        private Color GetPressureColor(float pressure)
        {
            float t = Mathf.InverseLerp(minPressure, maxPressure, pressure);
            return pressureGradient.Evaluate(t);
        }

        private Color GetOxygenColor(AtmosphericNode node)
        {
            float oxygenPercent = GetOxygenPercent(node);
            float t = Mathf.InverseLerp(minOxygenPercent, maxOxygenPercent, oxygenPercent);
            return oxygenGradient.Evaluate(t);
        }

        private Color GetTemperatureColor(float temperature)
        {
            float t = Mathf.InverseLerp(minTemperature, maxTemperature, temperature);
            return temperatureGradient.Evaluate(t);
        }

        private Color GetBreathabilityColor(AtmosphericNode node)
        {
            float oxygenPercent = GetOxygenPercent(node);

            if (oxygenPercent >= breathableOxygenMin && oxygenPercent <= breathableOxygenMax)
            {
                float pressure = node.Pressure;
                if (pressure >= 80f && pressure <= 120f)
                {
                    return breathableColor;
                }
            }

            if (oxygenPercent < 5f)
            {
                return noOxygenColor;
            }

            return lowOxygenColor;
        }

        private float GetOxygenPercent(AtmosphericNode node)
        {
            if (node == null || node.Mixture == null) return 0f;

            float totalMoles = node.Mixture.TotalMoles();
            if (totalMoles <= 0f) return 0f;

            float o2Moles = node.Mixture.Moles.ContainsKey("O2") ? node.Mixture.Moles["O2"] : 0f;
            return (o2Moles / totalMoles) * 100f;
        }

        private void CreateOrUpdateOverlay(Vector3Int voxelPos, Color color)
        {
            if (!overlayVoxels.ContainsKey(voxelPos))
            {
                GameObject overlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
                overlay.name = $"Overlay_{voxelPos}";
                overlay.transform.SetParent(transform);
                overlay.transform.position = bridge.VoxelToWorldPosition(voxelPos) + Vector3.one * bridge.voxelSize * 0.5f;
                overlay.transform.localScale = Vector3.one * bridge.voxelSize * 0.98f;

                Destroy(overlay.GetComponent<Collider>());

                MeshRenderer renderer = overlay.GetComponent<MeshRenderer>();
                renderer.material = new Material(overlayMaterial);
                renderer.material.color = color;

                overlayVoxels[voxelPos] = overlay;
            }
            else
            {
                MeshRenderer renderer = overlayVoxels[voxelPos].GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
            }
        }

        private void RemoveOverlay(Vector3Int voxelPos)
        {
            if (overlayVoxels.ContainsKey(voxelPos))
            {
                Destroy(overlayVoxels[voxelPos]);
                overlayVoxels.Remove(voxelPos);
            }
        }

        public void ClearOverlays()
        {
            foreach (var overlay in overlayVoxels.Values)
            {
                if (overlay != null)
                {
                    Destroy(overlay);
                }
            }
            overlayVoxels.Clear();
        }

        public void SetVisualizationMode(VisualizationMode mode)
        {
            if (currentMode != mode)
            {
                currentMode = mode;
                UpdateVisualization();
            }
        }

        private void OnDestroy()
        {
            ClearOverlays();
        }
    }
}
