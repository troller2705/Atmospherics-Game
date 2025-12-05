using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Visualization
{
    [RequireComponent(typeof(Pipe))]
    public class PipeVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        public bool showPipe = true;
        public bool showFlowDirection = true;
        public bool animateFlow = true;

        [Header("Visual Style")]
        public float pipeThickness = 0.1f;
        public int pipeSegments = 8;
        public Color pipeColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        public Color flowColor = new Color(0, 1, 1, 0.8f);

        [Header("Animation")]
        public float flowSpeed = 1f;
        public int flowIndicatorCount = 3;

        private Pipe pipe;
        private GameObject pipeVisual;
        private LineRenderer lineRenderer;
        private GameObject[] flowIndicators;

        private void Start()
        {
            pipe = GetComponent<Pipe>();
            CreatePipeVisual();
            CreateFlowIndicators();
        }

        private void CreatePipeVisual()
        {
            pipeVisual = new GameObject("Pipe Visual");
            pipeVisual.transform.SetParent(transform);
            pipeVisual.transform.localPosition = Vector3.zero;

            lineRenderer = pipeVisual.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lineRenderer.material.color = pipeColor;
            lineRenderer.startWidth = pipeThickness;
            lineRenderer.endWidth = pipeThickness;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;

            Material mat = lineRenderer.material;
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_Blend", 0);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;

            UpdatePipePositions();
        }

        private void CreateFlowIndicators()
        {
            if (!showFlowDirection) return;

            flowIndicators = new GameObject[flowIndicatorCount];

            for (int i = 0; i < flowIndicatorCount; i++)
            {
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                indicator.name = $"Flow Indicator {i}";
                indicator.transform.SetParent(pipeVisual.transform);
                indicator.transform.localScale = Vector3.one * (pipeThickness * 1.5f);

                MeshRenderer renderer = indicator.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = flowColor;
                mat.SetFloat("_Surface", 1);
                mat.SetFloat("_Blend", 0);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3001;
                renderer.material = mat;

                Destroy(indicator.GetComponent<Collider>());

                flowIndicators[i] = indicator;
            }
        }

        private void Update()
        {
            if (!showPipe && pipeVisual != null)
            {
                pipeVisual.SetActive(false);
                return;
            }

            if (pipeVisual != null && !pipeVisual.activeSelf)
                pipeVisual.SetActive(true);

            UpdatePipePositions();

            if (animateFlow && showFlowDirection)
            {
                AnimateFlowIndicators();
            }
        }

        private void UpdatePipePositions()
        {
            if (pipe == null || lineRenderer == null) return;
            if (pipe.NodeA == null || pipe.NodeB == null) return;

            Vector3 startPos = pipe.NodeA.transform.position;
            Vector3 endPos = pipe.NodeB.transform.position;

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        private void AnimateFlowIndicators()
        {
            if (pipe == null || pipe.NodeA == null || pipe.NodeB == null) return;
            if (flowIndicators == null || flowIndicators.Length == 0) return;

            Vector3 startPos = pipe.NodeA.transform.position;
            Vector3 endPos = pipe.NodeB.transform.position;

            for (int i = 0; i < flowIndicators.Length; i++)
            {
                if (flowIndicators[i] == null) continue;

                float offset = (float)i / flowIndicatorCount;
                float t = Mathf.Repeat((Time.time * flowSpeed) + offset, 1f);

                flowIndicators[i].transform.position = Vector3.Lerp(startPos, endPos, t);
            }
        }

        private void OnDestroy()
        {
            if (pipeVisual != null)
                Destroy(pipeVisual);
        }
    }
}
