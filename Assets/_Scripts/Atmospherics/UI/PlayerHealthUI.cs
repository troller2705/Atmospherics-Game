using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Atmospherics.Core;

namespace Atmospherics.Player
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        public PlayerAtmosphericNeeds playerNeeds;

        [Header("Health Bar")]
        public Image healthBarFill;
        public Image staminaBarFill;

        [Header("Status Text - Legacy UI")]
        public Text statusText;
        public Text atmosphereText;
        public Text warningText;

        [Header("Status Text - TextMeshPro")]
        public TextMeshProUGUI tmpStatusText;
        public TextMeshProUGUI tmpAtmosphereText;
        public TextMeshProUGUI tmpWarningText;

        [Header("Warning Panel")]
        public GameObject warningPanel;
        public Image warningBackground;

        [Header("Settings")]
        public bool useTextMeshPro = true;
        public float updateInterval = 0.2f;
        public Color healthyColor = Color.green;
        public Color warnColor = Color.yellow;
        public Color dangerColor = Color.red;

        private float updateTimer = 0f;

        private void Update()
        {
            if (playerNeeds == null) return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateUI();
                updateTimer = 0f;
            }
        }

        private void UpdateUI()
        {
            float healthPercent = playerNeeds.GetHealthPercent();
            float staminaPercent = playerNeeds.GetStaminaPercent();

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercent;
                healthBarFill.color = GetHealthColor(healthPercent);
            }

            if (staminaBarFill != null)
            {
                staminaBarFill.fillAmount = staminaPercent;
                staminaBarFill.color = GetStaminaColor(staminaPercent);
            }

            string statusStr = playerNeeds.currentStatus;
            string atmosphereStr = GetAtmosphereString();
            string warningStr = GetWarningString();

            if (useTextMeshPro)
            {
                if (tmpStatusText != null)
                {
                    tmpStatusText.text = statusStr;
                    tmpStatusText.color = playerNeeds.isTakingDamage ? dangerColor : healthyColor;
                }
                if (tmpAtmosphereText != null)
                {
                    tmpAtmosphereText.text = atmosphereStr;
                }
                if (tmpWarningText != null)
                {
                    tmpWarningText.text = warningStr;
                    tmpWarningText.color = dangerColor;
                }
            }
            else
            {
                if (statusText != null)
                {
                    statusText.text = statusStr;
                    statusText.color = playerNeeds.isTakingDamage ? dangerColor : healthyColor;
                }
                if (atmosphereText != null)
                {
                    atmosphereText.text = atmosphereStr;
                }
                if (warningText != null)
                {
                    warningText.text = warningStr;
                    warningText.color = dangerColor;
                }
            }

            bool showWarning = playerNeeds.isSuffocating || playerNeeds.isTakingDamage;
            if (warningPanel != null)
            {
                warningPanel.SetActive(showWarning);
            }

            if (warningBackground != null && showWarning)
            {
                float alpha = 0.3f + 0.2f * Mathf.Sin(Time.time * 3f);
                warningBackground.color = new Color(1f, 0f, 0f, alpha);
            }
        }

        private string GetAtmosphereString()
        {
            if (playerNeeds.currentNode == null)
            {
                return "NO ATMOSPHERE";
            }

            var readings = playerNeeds.GetCurrentReadings();
            return $"O₂: {readings.oxygenPercent:F1}% | CO₂: {readings.co2Percent:F1}%\n" +
                   $"P: {readings.pressure:F1}kPa | T: {readings.TemperatureCelsius:F1}°C";
        }

        private string GetWarningString()
        {
            if (playerNeeds.isSuffocating)
            {
                return "⚠ SUFFOCATION ⚠";
            }
            if (playerNeeds.isTakingDamage)
            {
                return "⚠ TAKING DAMAGE ⚠";
            }
            return "";
        }

        private Color GetHealthColor(float percent)
        {
            if (percent > 0.6f) return healthyColor;
            if (percent > 0.3f) return warnColor;
            return dangerColor;
        }

        private Color GetStaminaColor(float percent)
        {
            if (percent > 0.3f) return Color.cyan;
            return warnColor;
        }
    }
}
