using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Visualization
{
    [RequireComponent(typeof(AtmosphericNode))]
    public class AtmosphericZoneVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        public bool showZoneBoundary = true;
        public bool showPressureColor = true;
        public bool showTemperatureColor = false;

        [Header("Visual Style")]
        public MeshType meshType = MeshType.WireframeSphere;
        public float sphereRadius = 1f;
        public float wireframeThickness = 0.05f;

        [Header("Colors")]
        public Gradient pressureGradient;
        public Gradient temperatureGradient;
        public Color defaultColor = new Color(0, 1, 1, 0.3f);

        [Header("Update")]
        public float updateInterval = 0.5f;

        private AtmosphericNode volumeNode;
        private GameObject visualMesh;
        private MeshRenderer meshRenderer;
        private Material visualMaterial;
        private float updateTimer;

        public enum MeshType
        {
            WireframeSphere,
            TransparentSphere,
            WireframeCube,
            TransparentCube
        }

        private void Start()
        {
            volumeNode = GetComponent<AtmosphericNode>();
            InitializePressureGradient();
            InitializeTemperatureGradient();
            CreateVisualMesh();
        }

        private void InitializePressureGradient()
        {
            if (pressureGradient == null || pressureGradient.colorKeys.Length == 0)
            {
                pressureGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(Color.red, 0f);      // Vacuum
                colorKeys[1] = new GradientColorKey(Color.yellow, 0.25f); // Low
                colorKeys[2] = new GradientColorKey(Color.green, 0.5f);   // Normal
                colorKeys[3] = new GradientColorKey(Color.cyan, 0.75f);   // High
                colorKeys[4] = new GradientColorKey(Color.magenta, 1f);   // Extreme

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(0.3f, 0f);
                alphaKeys[1] = new GradientAlphaKey(0.5f, 1f);

                pressureGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        private void InitializeTemperatureGradient()
        {
            if (temperatureGradient == null || temperatureGradient.colorKeys.Length == 0)
            {
                temperatureGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0] = new GradientColorKey(Color.blue, 0f);    // Cold
                colorKeys[1] = new GradientColorKey(Color.white, 0.5f); // Normal
                colorKeys[2] = new GradientColorKey(Color.red, 1f);     // Hot

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(0.3f, 0f);
                alphaKeys[1] = new GradientAlphaKey(0.3f, 1f);

                temperatureGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        private void CreateVisualMesh()
        {
            visualMesh = new GameObject("Zone Visual");
            visualMesh.transform.SetParent(transform);
            visualMesh.transform.localPosition = Vector3.zero;

            MeshFilter meshFilter = visualMesh.AddComponent<MeshFilter>();
            meshRenderer = visualMesh.AddComponent<MeshRenderer>();

            switch (meshType)
            {
                case MeshType.WireframeSphere:
                case MeshType.TransparentSphere:
                    meshFilter.mesh = CreateSphereMesh();
                    visualMesh.transform.localScale = Vector3.one * sphereRadius;
                    break;
                case MeshType.WireframeCube:
                case MeshType.TransparentCube:
                    meshFilter.mesh = CreateCubeMesh();
                    visualMesh.transform.localScale = Vector3.one * sphereRadius;
                    break;
            }

            visualMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            visualMaterial.SetFloat("_Surface", 1);
            visualMaterial.SetFloat("_Blend", 0);
            visualMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            visualMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            visualMaterial.SetFloat("_ZWrite", 0);
            visualMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            visualMaterial.renderQueue = 3000;

            meshRenderer.material = visualMaterial;

            UpdateVisuals();
        }

        private Mesh CreateSphereMesh()
        {
            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh mesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempSphere);
            return mesh;
        }

        private Mesh CreateCubeMesh()
        {
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempCube);
            return mesh;
        }

        private void Update()
        {
            if (!showZoneBoundary)
            {
                if (visualMesh != null && visualMesh.activeSelf)
                    visualMesh.SetActive(false);
                return;
            }

            if (visualMesh != null && !visualMesh.activeSelf)
                visualMesh.SetActive(true);

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateVisuals();
                updateTimer = 0f;
            }
        }

        private void UpdateVisuals()
        {
            if (volumeNode == null || volumeNode.Mixture == null || visualMaterial == null)
                return;

            Color color = defaultColor;

            if (showPressureColor)
            {
                float pressure = volumeNode.Mixture.GetPressure();
                float t = Mathf.InverseLerp(0f, 200f, pressure);
                color = pressureGradient.Evaluate(t);
            }
            else if (showTemperatureColor)
            {
                float temp = volumeNode.Mixture.Temperature;
                float t = Mathf.InverseLerp(200f, 400f, temp);
                color = temperatureGradient.Evaluate(t);
            }

            visualMaterial.color = color;
        }

        private void OnDestroy()
        {
            if (visualMesh != null)
                Destroy(visualMesh);
            if (visualMaterial != null)
                Destroy(visualMaterial);
        }
    }
}
