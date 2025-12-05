using System;
using UnityEngine;

namespace Atmospherics.Core
{
    [Serializable]
    public struct ThermalProperties
    {
        [Header("Energy")]
        [Tooltip("Internal energy in Joules")]
        public float InternalEnergyJ;

        [Header("Heat Transfer")]
        [Tooltip("External/space temperature in Kelvin")]
        public float ExternalTempK;

        [Tooltip("Heat loss coefficient (W/K)")]
        public float HeatLossCoefficient;

        [Tooltip("Thermal capacity (J/K)")]
        public float ThermalCapacityJPerK;

        public const float SPECIFIC_HEAT_CP = 29.0f;

        public static ThermalProperties Default()
        {
            return new ThermalProperties
            {
                InternalEnergyJ = 0f,
                ExternalTempK = 220f,
                HeatLossCoefficient = 1f,
                ThermalCapacityJPerK = 1000f
            };
        }

        public void RecalculateEnergy(float totalMoles, float temperature)
        {
            InternalEnergyJ = totalMoles * SPECIFIC_HEAT_CP * temperature;
        }

        public float CalculateTemperature(float totalMoles)
        {
            if (totalMoles <= 0f) return 0.1f;
            return Mathf.Max(0.1f, InternalEnergyJ / (totalMoles * SPECIFIC_HEAT_CP));
        }
    }
}
