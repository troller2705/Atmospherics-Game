using UnityEngine;
using System.Collections.Generic;

namespace Atmospherics.Core
{
    public class AtmosphericHazards : MonoBehaviour
    {
        [Header("Target Node")]
        public AtmosphericNode monitoredNode;

        [Header("Pressure Thresholds (kPa)")]
        public float minSafePressure = 50f;
        public float maxSafePressure = 150f;
        public float criticalLowPressure = 20f;
        public float criticalHighPressure = 200f;

        [Header("Temperature Thresholds (K)")]
        public float minSafeTemperature = 273f;
        public float maxSafeTemperature = 310f;
        public float criticalLowTemperature = 250f;
        public float criticalHighTemperature = 340f;

        [Header("Oxygen Thresholds (%)")]
        public float minSafeOxygen = 18f;
        public float optimalOxygen = 21f;
        public float maxSafeOxygen = 25f;
        public float criticalLowOxygen = 10f;

        [Header("CO2 Thresholds (%)")]
        public float maxSafeCO2 = 1f;
        public float dangerousCO2 = 3f;
        public float criticalCO2 = 5f;

        [Header("Status")]
        public HazardLevel currentHazardLevel = HazardLevel.Safe;
        public List<string> activeWarnings = new List<string>();

        [Header("Events")]
        public UnityEngine.Events.UnityEvent onDangerDetected;
        public UnityEngine.Events.UnityEvent onCriticalDetected;
        public UnityEngine.Events.UnityEvent onReturnToSafe;

        public enum HazardLevel
        {
            Safe,
            Caution,
            Warning,
            Danger,
            Critical
        }

        private HazardLevel previousHazardLevel = HazardLevel.Safe;

        private void Start()
        {
            if (monitoredNode == null)
            {
                Debug.LogWarning($"[AtmosphericHazards] {gameObject.name} has no monitored node!");
            }
        }

        public void UpdateHazards()
        {
            if (monitoredNode == null || monitoredNode.Mixture == null)
            {
                currentHazardLevel = HazardLevel.Critical;
                activeWarnings.Clear();
                activeWarnings.Add("No atmosphere detected!");
                return;
            }

            activeWarnings.Clear();
            HazardLevel newLevel = HazardLevel.Safe;

            float pressure = monitoredNode.Mixture.GetPressure();
            float temperature = monitoredNode.Mixture.Temperature;
            float oxygenPercent = GetGasPercent("O2");
            float co2Percent = GetGasPercent("CO2");

            newLevel = CheckPressureHazards(pressure, newLevel);
            newLevel = CheckTemperatureHazards(temperature, newLevel);
            newLevel = CheckOxygenHazards(oxygenPercent, newLevel);
            newLevel = CheckCO2Hazards(co2Percent, newLevel);

            currentHazardLevel = newLevel;

            if (currentHazardLevel != previousHazardLevel)
            {
                OnHazardLevelChanged(previousHazardLevel, currentHazardLevel);
                previousHazardLevel = currentHazardLevel;
            }
        }

        private HazardLevel CheckPressureHazards(float pressure, HazardLevel current)
        {
            if (pressure < criticalLowPressure)
            {
                activeWarnings.Add($"CRITICAL: Vacuum conditions ({pressure:F1} kPa)");
                return MaxLevel(current, HazardLevel.Critical);
            }
            if (pressure > criticalHighPressure)
            {
                activeWarnings.Add($"CRITICAL: Extreme pressure ({pressure:F1} kPa)");
                return MaxLevel(current, HazardLevel.Critical);
            }
            if (pressure < minSafePressure)
            {
                activeWarnings.Add($"WARNING: Low pressure ({pressure:F1} kPa)");
                return MaxLevel(current, HazardLevel.Warning);
            }
            if (pressure > maxSafePressure)
            {
                activeWarnings.Add($"WARNING: High pressure ({pressure:F1} kPa)");
                return MaxLevel(current, HazardLevel.Warning);
            }

            return current;
        }

        private HazardLevel CheckTemperatureHazards(float temperature, HazardLevel current)
        {
            float tempC = temperature - 273.15f;

            if (temperature < criticalLowTemperature)
            {
                activeWarnings.Add($"CRITICAL: Extreme cold ({tempC:F1}°C)");
                return MaxLevel(current, HazardLevel.Critical);
            }
            if (temperature > criticalHighTemperature)
            {
                activeWarnings.Add($"CRITICAL: Extreme heat ({tempC:F1}°C)");
                return MaxLevel(current, HazardLevel.Critical);
            }
            if (temperature < minSafeTemperature)
            {
                activeWarnings.Add($"WARNING: Cold environment ({tempC:F1}°C)");
                return MaxLevel(current, HazardLevel.Warning);
            }
            if (temperature > maxSafeTemperature)
            {
                activeWarnings.Add($"WARNING: Hot environment ({tempC:F1}°C)");
                return MaxLevel(current, HazardLevel.Warning);
            }

            return current;
        }

        private HazardLevel CheckOxygenHazards(float oxygenPercent, HazardLevel current)
        {
            if (oxygenPercent < criticalLowOxygen)
            {
                activeWarnings.Add($"CRITICAL: Hypoxia - O₂ at {oxygenPercent:F1}%");
                return MaxLevel(current, HazardLevel.Critical);
            }
            if (oxygenPercent < minSafeOxygen)
            {
                activeWarnings.Add($"DANGER: Low oxygen - {oxygenPercent:F1}%");
                return MaxLevel(current, HazardLevel.Danger);
            }
            if (oxygenPercent > maxSafeOxygen)
            {
                activeWarnings.Add($"WARNING: High oxygen - {oxygenPercent:F1}%");
                return MaxLevel(current, HazardLevel.Warning);
            }

            return current;
        }

        private HazardLevel CheckCO2Hazards(float co2Percent, HazardLevel current)
        {
            if (co2Percent > criticalCO2)
            {
                activeWarnings.Add($"CRITICAL: CO₂ toxicity - {co2Percent:F1}%");
                return MaxLevel(current, HazardLevel.Critical);
            }
            if (co2Percent > dangerousCO2)
            {
                activeWarnings.Add($"DANGER: High CO₂ - {co2Percent:F1}%");
                return MaxLevel(current, HazardLevel.Danger);
            }
            if (co2Percent > maxSafeCO2)
            {
                activeWarnings.Add($"WARNING: Elevated CO₂ - {co2Percent:F1}%");
                return MaxLevel(current, HazardLevel.Warning);
            }

            return current;
        }

        private HazardLevel MaxLevel(HazardLevel a, HazardLevel b)
        {
            return (int)a > (int)b ? a : b;
        }

        private float GetGasPercent(string gasName)
        {
            if (monitoredNode == null || monitoredNode.Mixture == null)
                return 0f;

            float totalMoles = monitoredNode.Mixture.TotalMoles();
            if (totalMoles <= 0f) return 0f;

            if (!monitoredNode.Mixture.Moles.ContainsKey(gasName))
                return 0f;

            return (monitoredNode.Mixture.Moles[gasName] / totalMoles) * 100f;
        }

        private void OnHazardLevelChanged(HazardLevel oldLevel, HazardLevel newLevel)
        {
            Debug.Log($"[Atmospheric Hazards] Level changed: {oldLevel} → {newLevel}");

            if (newLevel == HazardLevel.Critical)
            {
                onCriticalDetected?.Invoke();
            }
            else if (newLevel == HazardLevel.Danger)
            {
                onDangerDetected?.Invoke();
            }
            else if (newLevel == HazardLevel.Safe && oldLevel != HazardLevel.Safe)
            {
                onReturnToSafe?.Invoke();
            }
        }

        public bool IsSafeForHumans()
        {
            return currentHazardLevel == HazardLevel.Safe || currentHazardLevel == HazardLevel.Warning;
        }

        public string GetHazardSummary()
        {
            if (activeWarnings.Count == 0)
                return "Atmosphere: SAFE";

            string summary = $"Atmosphere: {currentHazardLevel.ToString().ToUpper()}\n";
            foreach (var warning in activeWarnings)
            {
                summary += $"• {warning}\n";
            }
            return summary.TrimEnd();
        }

        public Color GetHazardColor()
        {
            switch (currentHazardLevel)
            {
                case HazardLevel.Safe:
                    return Color.green;
                case HazardLevel.Warning:
                    return Color.yellow;
                case HazardLevel.Danger:
                    return new Color(1f, 0.5f, 0f);
                case HazardLevel.Critical:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        public AtmosphericReadings GetReadings()
        {
            if (monitoredNode == null || monitoredNode.Mixture == null)
                return new AtmosphericReadings();

            return new AtmosphericReadings
            {
                pressure = monitoredNode.Mixture.GetPressure(),
                temperature = monitoredNode.Mixture.Temperature,
                oxygenPercent = GetGasPercent("O2"),
                co2Percent = GetGasPercent("CO2"),
                nitrogenPercent = GetGasPercent("N2"),
                totalMoles = monitoredNode.Mixture.TotalMoles(),
                hazardLevel = currentHazardLevel
            };
        }
    }

    [System.Serializable]
    public struct AtmosphericReadings
    {
        public float pressure;
        public float temperature;
        public float oxygenPercent;
        public float co2Percent;
        public float nitrogenPercent;
        public float totalMoles;
        public AtmosphericHazards.HazardLevel hazardLevel;

        public float TemperatureCelsius => temperature - 273.15f;

        public override string ToString()
        {
            return $"P: {pressure:F1}kPa | T: {TemperatureCelsius:F1}°C | O₂: {oxygenPercent:F1}% | CO₂: {co2Percent:F1}%";
        }
    }
}
