using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Atmospherics.Core;
using Atmospherics.Player;

namespace Atmospherics.UI
{
    public class AtmosphericUIManager : MonoBehaviour
    {
        [Header("Player Reference")]
        public PlayerAtmosphericNeeds playerNeeds;

        [Header("Health & Stamina Bars")]
        public Image healthBarFill;
        public Image staminaBarFill;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI staminaText;

        [Header("Atmospheric Readout Panel")]
        public TextMeshProUGUI zoneNameText;
        public TextMeshProUGUI pressureText;
        public TextMeshProUGUI temperatureText;
        public TextMeshProUGUI oxygenText;
        public TextMeshProUGUI co2Text;
        public TextMeshProUGUI nitrogenText;

        [Header("Status Panel")]
        public TextMeshProUGUI statusText;
        public Image statusIcon;
        public GameObject statusPanel;

        [Header("Warning Panel")]
        public GameObject warningPanel;
        public TextMeshProUGUI warningText;
        public Image warningBackground;
        public float warningFlashSpeed = 3f;

        [Header("Hazard Indicator")]
        public Image hazardIndicator;
        public TextMeshProUGUI hazardLevelText;

        [Header("Colors")]
        public Color healthyColor = Color.green;
        public Color cautionColor = Color.yellow;
        public Color warningColorValue = new Color(1f, 0.5f, 0f);
        public Color dangerColor = Color.red;
        public Color criticalColor = new Color(0.8f, 0f, 0f);

        [Header("Settings")]
        public float updateInterval = 0.1f;

        private float updateTimer = 0f;
        private bool isInitialized = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (playerNeeds == null)
            {
                playerNeeds = FindFirstObjectByType<PlayerAtmosphericNeeds>();
            }

            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized || playerNeeds == null) return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateAllUI();
                updateTimer = 0f;
            }

            UpdateWarningFlash();
        }

        private void UpdateAllUI()
        {
            UpdateHealthStamina();
            UpdateAtmosphericReadout();
            UpdateStatus();
            UpdateWarnings();
            UpdateHazardIndicator();
        }

        private void UpdateHealthStamina()
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

            if (healthText != null)
            {
                healthText.text = $"{playerNeeds.currentHealth:F0}/{playerNeeds.maxHealth:F0}";
                //healthText.color = GetHealthColor(healthPercent);
            }

            if (staminaText != null)
            {
                staminaText.text = $"{playerNeeds.currentStamina:F0}/{playerNeeds.maxStamina:F0}";
                //staminaText.color = GetStaminaColor(staminaPercent);
            }
        }

        private void UpdateAtmosphericReadout()
        {
            var readings = playerNeeds.GetCurrentReadings();

            if (zoneNameText != null)
            {
                string zoneName = playerNeeds.currentNode != null 
                    ? playerNeeds.currentNode.NodeName 
                    : "NO ATMOSPHERE";
                zoneNameText.text = zoneName;
                zoneNameText.color = playerNeeds.currentNode != null ? Color.white : dangerColor;
            }

            if (pressureText != null)
            {
                pressureText.text = $"P: {readings.pressure:F1} kPa";
                pressureText.color = GetPressureColor(readings.pressure);
            }

            if (temperatureText != null)
            {
                temperatureText.text = $"T: {readings.TemperatureCelsius:F1}°C";
                temperatureText.color = GetTemperatureColor(readings.temperature);
            }

            if (oxygenText != null)
            {
                oxygenText.text = $"O₂: {readings.oxygenPercent:F1}%";
                oxygenText.color = GetOxygenColor(readings.oxygenPercent);
            }

            if (co2Text != null)
            {
                co2Text.text = $"CO₂: {readings.co2Percent:F2}%";
                co2Text.color = GetCO2Color(readings.co2Percent);
            }

            if (nitrogenText != null)
            {
                nitrogenText.text = $"N₂: {readings.nitrogenPercent:F1}%";
            }
        }

        private void UpdateStatus()
        {
            if (statusText != null)
            {
                statusText.text = playerNeeds.currentStatus;
                
                if (playerNeeds.isSuffocating || playerNeeds.currentHealth < 30f)
                    statusText.color = criticalColor;
                else if (playerNeeds.isTakingDamage || playerNeeds.currentHealth < 60f)
                    statusText.color = warningColorValue;
                else
                    statusText.color = healthyColor;
            }

            if (statusPanel != null)
            {
                statusPanel.SetActive(playerNeeds.isTakingDamage || playerNeeds.isSuffocating);
            }

            if (statusIcon != null)
            {
                if (playerNeeds.isSuffocating)
                    statusIcon.color = criticalColor;
                else if (playerNeeds.isTakingDamage)
                    statusIcon.color = warningColorValue;
                else
                    statusIcon.color = healthyColor;
            }
        }

        private void UpdateWarnings()
        {
            bool showWarning = playerNeeds.isSuffocating || playerNeeds.isTakingDamage;

            if (warningPanel != null)
            {
                warningPanel.SetActive(showWarning);
            }

            if (warningText != null && showWarning)
            {
                if (playerNeeds.isSuffocating)
                    warningText.text = "⚠ OXYGEN CRITICAL ⚠";
                else if (playerNeeds.currentStatus.Contains("CO₂"))
                    warningText.text = "⚠ CO₂ TOXICITY ⚠";
                else if (playerNeeds.currentStatus.Contains("FREEZING"))
                    warningText.text = "⚠ EXTREME COLD ⚠";
                else if (playerNeeds.currentStatus.Contains("BURNING"))
                    warningText.text = "⚠ EXTREME HEAT ⚠";
                else
                    warningText.text = "⚠ WARNING ⚠";
            }
        }

        private void UpdateWarningFlash()
        {
            if (warningBackground != null && warningPanel != null && warningPanel.activeSelf)
            {
                float alpha = 0.3f + 0.2f * Mathf.Sin(Time.time * warningFlashSpeed);
                Color flashColor = playerNeeds.isSuffocating ? criticalColor : warningColorValue;
                flashColor.a = alpha;
                warningBackground.color = flashColor;
            }
        }

        private void UpdateHazardIndicator()
        {
            var readings = playerNeeds.GetCurrentReadings();

            if (hazardIndicator != null)
            {
                hazardIndicator.color = GetHazardLevelColor(readings.hazardLevel);
            }

            if (hazardLevelText != null)
            {
                hazardLevelText.text = readings.hazardLevel.ToString().ToUpper();
                hazardLevelText.color = GetHazardLevelColor(readings.hazardLevel);
            }
        }

        private Color GetHealthColor(float percent)
        {
            if (percent > 0.6f) return healthyColor;
            if (percent > 0.3f) return cautionColor;
            return dangerColor;
        }

        private Color GetStaminaColor(float percent)
        {
            if (percent > 0.3f) return new Color(0.3f, 0.8f, 1f);
            return cautionColor;
        }

        private Color GetPressureColor(float pressure)
        {
            if (pressure < 20f) return dangerColor;
            if (pressure < 80f || pressure > 120f) return cautionColor;
            return healthyColor;
        }

        private Color GetTemperatureColor(float temperatureK)
        {
            if (temperatureK < 250f || temperatureK > 340f) return dangerColor;
            if (temperatureK < 273f || temperatureK > 310f) return cautionColor;
            return healthyColor;
        }

        private Color GetOxygenColor(float percent)
        {
            if (percent < 10f) return dangerColor;
            if (percent < 18f) return cautionColor;
            return healthyColor;
        }

        private Color GetCO2Color(float percent)
        {
            if (percent > 5f) return dangerColor;
            if (percent > 3f) return warningColorValue;
            if (percent > 1f) return cautionColor;
            return healthyColor;
        }

        private Color GetHazardLevelColor(AtmosphericHazards.HazardLevel level)
        {
            switch (level)
            {
                case AtmosphericHazards.HazardLevel.Safe:
                    return healthyColor;
                case AtmosphericHazards.HazardLevel.Caution:
                    return cautionColor;
                case AtmosphericHazards.HazardLevel.Warning:
                    return warningColorValue;
                case AtmosphericHazards.HazardLevel.Danger:
                    return dangerColor;
                case AtmosphericHazards.HazardLevel.Critical:
                    return criticalColor;
                default:
                    return Color.white;
            }
        }

        public void SetPlayer(PlayerAtmosphericNeeds player)
        {
            playerNeeds = player;
        }
    }
}
