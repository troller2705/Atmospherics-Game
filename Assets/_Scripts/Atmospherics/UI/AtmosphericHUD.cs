using UnityEngine;
using UnityEngine.UI;
using Atmospherics.Core;
using TMPro;

namespace Atmospherics.UI
{
    public class AtmosphericHUD : MonoBehaviour
    {
        [Header("References")]
        public AtmosphericHazards hazardMonitor;

        [Header("UI Elements - Legacy UI")]
        public Text statusText;
        public Text pressureText;
        public Text temperatureText;
        public Text oxygenText;
        public Text co2Text;
        public Image warningPanel;

        [Header("UI Elements - TextMeshPro")]
        public TextMeshProUGUI tmpStatusText;
        public TextMeshProUGUI tmpPressureText;
        public TextMeshProUGUI tmpTemperatureText;
        public TextMeshProUGUI tmpOxygenText;
        public TextMeshProUGUI tmpCO2Text;

        [Header("Settings")]
        public bool useTextMeshPro = true;
        public float updateInterval = 0.5f;

        private float updateTimer = 0f;

        private void Update()
        {
            if (hazardMonitor == null) return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateDisplay();
                updateTimer = 0f;
            }
        }

        private void UpdateDisplay()
        {
            var readings = hazardMonitor.GetReadings();
            Color hazardColor = hazardMonitor.GetHazardColor();

            string statusStr = hazardMonitor.currentHazardLevel.ToString().ToUpper();
            string pressureStr = $"{readings.pressure:F1} kPa";
            string tempStr = $"{readings.TemperatureCelsius:F1}°C ({readings.temperature:F1}K)";
            string oxygenStr = $"O₂: {readings.oxygenPercent:F1}%";
            string co2Str = $"CO₂: {readings.co2Percent:F2}%";

            if (useTextMeshPro)
            {
                if (tmpStatusText != null)
                {
                    tmpStatusText.text = statusStr;
                    tmpStatusText.color = hazardColor;
                }
                if (tmpPressureText != null) tmpPressureText.text = pressureStr;
                if (tmpTemperatureText != null) tmpTemperatureText.text = tempStr;
                if (tmpOxygenText != null)
                {
                    tmpOxygenText.text = oxygenStr;
                    tmpOxygenText.color = GetOxygenColor(readings.oxygenPercent);
                }
                if (tmpCO2Text != null)
                {
                    tmpCO2Text.text = co2Str;
                    tmpCO2Text.color = GetCO2Color(readings.co2Percent);
                }
            }
            else
            {
                if (statusText != null)
                {
                    statusText.text = statusStr;
                    statusText.color = hazardColor;
                }
                if (pressureText != null) pressureText.text = pressureStr;
                if (temperatureText != null) temperatureText.text = tempStr;
                if (oxygenText != null)
                {
                    oxygenText.text = oxygenStr;
                    oxygenText.color = GetOxygenColor(readings.oxygenPercent);
                }
                if (co2Text != null)
                {
                    co2Text.text = co2Str;
                    co2Text.color = GetCO2Color(readings.co2Percent);
                }
            }

            if (warningPanel != null)
            {
                warningPanel.color = new Color(hazardColor.r, hazardColor.g, hazardColor.b, 0.3f);
            }
        }

        private Color GetOxygenColor(float oxygenPercent)
        {
            if (oxygenPercent < 10f) return Color.red;
            if (oxygenPercent < 18f) return new Color(1f, 0.5f, 0f);
            if (oxygenPercent > 25f) return Color.yellow;
            return Color.green;
        }

        private Color GetCO2Color(float co2Percent)
        {
            if (co2Percent > 5f) return Color.red;
            if (co2Percent > 3f) return new Color(1f, 0.5f, 0f);
            if (co2Percent > 1f) return Color.yellow;
            return Color.green;
        }
    }
}
