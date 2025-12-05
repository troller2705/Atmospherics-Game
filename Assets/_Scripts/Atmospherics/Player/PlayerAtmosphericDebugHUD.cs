using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Player
{
    public class PlayerAtmosphericDebugHUD : MonoBehaviour
    {
        [Header("References")]
        public PlayerAtmosphericNeeds playerNeeds;

        [Header("Display Settings")]
        public bool showHUD = true;
        public bool showDetailedReadings = true;
        public int fontSize = 14;
        public Color normalColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color dangerColor = Color.red;

        private GUIStyle statusStyle;
        private GUIStyle detailStyle;

        private void OnGUI()
        {
            if (!showHUD || playerNeeds == null) return;

            InitializeStyles();

            float yPos = 10f;
            float padding = 5f;

            DrawStatusBox(ref yPos, padding);

            if (showDetailedReadings)
            {
                yPos += 10f;
                DrawDetailedReadings(ref yPos, padding);
            }
        }

        private void InitializeStyles()
        {
            if (statusStyle == null)
            {
                statusStyle = new GUIStyle(GUI.skin.box);
                statusStyle.fontSize = fontSize + 2;
                statusStyle.fontStyle = FontStyle.Bold;
                statusStyle.alignment = TextAnchor.MiddleLeft;
                statusStyle.normal.textColor = Color.white;
                statusStyle.padding = new RectOffset(10, 10, 10, 10);
            }

            if (detailStyle == null)
            {
                detailStyle = new GUIStyle(GUI.skin.box);
                detailStyle.fontSize = fontSize;
                detailStyle.alignment = TextAnchor.UpperLeft;
                detailStyle.normal.textColor = Color.white;
                detailStyle.padding = new RectOffset(10, 10, 10, 10);
            }
        }

        private void DrawStatusBox(ref float yPos, float padding)
        {
            Color statusColor = GetStatusColor();
            statusStyle.normal.textColor = statusColor;

            string statusText = $"STATUS: {playerNeeds.currentStatus}\n" +
                              $"Health: {playerNeeds.currentHealth:F0}/{playerNeeds.maxHealth:F0} " +
                              $"({playerNeeds.GetHealthPercent() * 100f:F0}%)\n" +
                              $"Stamina: {playerNeeds.currentStamina:F0}/{playerNeeds.maxStamina:F0} " +
                              $"({playerNeeds.GetStaminaPercent() * 100f:F0}%)";

            GUIContent content = new GUIContent(statusText);
            Vector2 size = statusStyle.CalcSize(content);

            GUI.Box(new Rect(padding, yPos, size.x + 20f, size.y + 20f), statusText, statusStyle);
            yPos += size.y + 30f;
        }

        private void DrawDetailedReadings(ref float yPos, float padding)
        {
            var readings = playerNeeds.GetCurrentReadings();

            string nodeInfo = playerNeeds.currentNode != null 
                ? $"Zone: {playerNeeds.currentNode.NodeName}" 
                : "Zone: NONE (Vacuum)";

            Color o2Color = GetOxygenColor(readings.oxygenPercent);
            Color co2Color = GetCO2Color(readings.co2Percent);

            string detailText = $"{nodeInfo}\n\n" +
                              $"Pressure: {readings.pressure:F1} kPa\n" +
                              $"Temperature: {readings.TemperatureCelsius:F1}°C ({readings.temperature:F1}K)\n" +
                              $"Total Moles: {readings.totalMoles:F2}\n\n" +
                              $"Oxygen (O₂): {readings.oxygenPercent:F1}%\n" +
                              $"Carbon Dioxide (CO₂): {readings.co2Percent:F2}%\n" +
                              $"Nitrogen (N₂): {readings.nitrogenPercent:F1}%\n\n" +
                              $"Hazard Level: {readings.hazardLevel}";

            GUIContent content = new GUIContent(detailText);
            Vector2 size = detailStyle.CalcSize(content);

            GUI.Box(new Rect(padding, yPos, size.x + 20f, size.y + 20f), detailText, detailStyle);

            DrawColorIndicator(padding + size.x + 30f, yPos + 110f, "O₂", o2Color);
            DrawColorIndicator(padding + size.x + 30f, yPos + 130f, "CO₂", co2Color);
        }

        private void DrawColorIndicator(float x, float y, string label, Color color)
        {
            GUI.color = color;
            GUI.Box(new Rect(x, y, 50f, 15f), label);
            GUI.color = Color.white;
        }

        private Color GetStatusColor()
        {
            if (playerNeeds.isSuffocating || playerNeeds.currentHealth < 30f)
                return dangerColor;
            if (playerNeeds.isTakingDamage || playerNeeds.currentHealth < 60f)
                return warningColor;
            return normalColor;
        }

        private Color GetOxygenColor(float percent)
        {
            if (percent < 10f) return dangerColor;
            if (percent < 18f) return warningColor;
            return normalColor;
        }

        private Color GetCO2Color(float percent)
        {
            if (percent > 5f) return dangerColor;
            if (percent > 3f) return warningColor;
            if (percent > 1f) return Color.yellow;
            return normalColor;
        }
    }
}
