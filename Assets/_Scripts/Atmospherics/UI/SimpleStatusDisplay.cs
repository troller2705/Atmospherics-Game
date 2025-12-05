using UnityEngine;
using TMPro;
using Atmospherics.Core;

namespace Atmospherics.UI
{
    public class SimpleStatusDisplay : MonoBehaviour
    {
        [Header("Target")]
        public AtmosphericNode targetNode;
        public AtmosphericHazards targetHazards;

        [Header("Display")]
        public TextMeshProUGUI displayText;
        public bool showInWorldSpace = true;
        public float updateInterval = 0.5f;

        [Header("Format")]
        public bool showPressure = true;
        public bool showTemperature = true;
        public bool showOxygen = true;
        public bool showCO2 = true;
        public bool showHazardLevel = true;

        private float updateTimer = 0f;
        private Canvas canvas;

        private void Start()
        {
            if (targetNode == null)
            {
                targetNode = GetComponentInParent<AtmosphericNode>();
            }

            if (targetHazards == null && targetNode != null)
            {
                targetHazards = targetNode.GetComponent<AtmosphericHazards>();
            }

            if (showInWorldSpace)
            {
                SetupWorldSpaceCanvas();
            }
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateDisplay();
                updateTimer = 0f;
            }
        }

        private void SetupWorldSpaceCanvas()
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Status Canvas");
                canvasObj.transform.SetParent(transform.parent);
                canvasObj.transform.localPosition = Vector3.zero;

                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;

                RectTransform rect = canvasObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(2, 1);
                rect.localScale = Vector3.one * 0.01f;

                if (displayText != null)
                {
                    displayText.transform.SetParent(canvasObj.transform, false);
                }
            }
        }

        private void UpdateDisplay()
        {
            if (displayText == null || targetNode == null || targetNode.Mixture == null)
                return;

            string text = "";

            if (targetNode != null)
            {
                text += $"<b>{targetNode.NodeName}</b>\n";
            }

            if (showPressure)
            {
                float pressure = targetNode.Mixture.GetPressure();
                text += $"P: {pressure:F1} kPa\n";
            }

            if (showTemperature)
            {
                float tempC = targetNode.Mixture.Temperature - 273.15f;
                text += $"T: {tempC:F1}°C\n";
            }

            if (showOxygen)
            {
                float o2Percent = GetGasPercent("O2");
                text += $"O₂: {o2Percent:F1}%\n";
            }

            if (showCO2)
            {
                float co2Percent = GetGasPercent("CO2");
                text += $"CO₂: {co2Percent:F2}%\n";
            }

            if (showHazardLevel && targetHazards != null)
            {
                text += $"<color={GetHazardColor()}>{targetHazards.currentHazardLevel}</color>";
            }

            displayText.text = text;
        }

        private float GetGasPercent(string gasName)
        {
            if (targetNode == null || targetNode.Mixture == null)
                return 0f;

            float totalMoles = targetNode.Mixture.TotalMoles();
            if (totalMoles <= 0f) return 0f;

            if (!targetNode.Mixture.Moles.ContainsKey(gasName))
                return 0f;

            return (targetNode.Mixture.Moles[gasName] / totalMoles) * 100f;
        }

        private string GetHazardColor()
        {
            if (targetHazards == null) return "white";

            switch (targetHazards.currentHazardLevel)
            {
                case AtmosphericHazards.HazardLevel.Safe:
                    return "green";
                case AtmosphericHazards.HazardLevel.Caution:
                    return "yellow";
                case AtmosphericHazards.HazardLevel.Warning:
                    return "orange";
                case AtmosphericHazards.HazardLevel.Danger:
                    return "#FF6600";
                case AtmosphericHazards.HazardLevel.Critical:
                    return "red";
                default:
                    return "white";
            }
        }
    }
}
