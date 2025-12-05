using UnityEngine;

namespace Atmospherics.Core
{
    public class GlobalAtmosphere : MonoBehaviour
    {
        [Header("Global Atmosphere Settings")]
        [Tooltip("Pressure of the planet's atmosphere in kPa")]
        public float globalPressure = 0f;

        [Tooltip("Temperature of the planet's atmosphere in Kelvin")]
        public float globalTemperature = 273.15f;

        [Header("Gas Composition (mole fractions)")]
        public float nitrogen = 0f;
        public float oxygen = 0f;
        public float carbonDioxide = 0f;

        [Header("Presets")]
        public AtmospherePreset preset = AtmospherePreset.Vacuum;

        public enum AtmospherePreset
        {
            Vacuum,
            Mars,
            Earth,
            Venus,
            Custom
        }

        private void OnValidate()
        {
            ApplyPreset();
        }

        private void Awake()
        {
            ApplyPreset();
        }

        public void ApplyPreset()
        {
            switch (preset)
            {
                case AtmospherePreset.Vacuum:
                    globalPressure = 0f;
                    globalTemperature = 2.7f;
                    nitrogen = 0f;
                    oxygen = 0f;
                    carbonDioxide = 0f;
                    break;

                case AtmospherePreset.Mars:
                    globalPressure = 0.6f;
                    globalTemperature = 210f;
                    nitrogen = 0.027f;
                    oxygen = 0.0013f;
                    carbonDioxide = 0.95f;
                    break;

                case AtmospherePreset.Earth:
                    globalPressure = 101.325f;
                    globalTemperature = 288.15f;
                    nitrogen = 0.78f;
                    oxygen = 0.21f;
                    carbonDioxide = 0.0004f;
                    break;

                case AtmospherePreset.Venus:
                    globalPressure = 9200f;
                    globalTemperature = 735f;
                    nitrogen = 0.035f;
                    oxygen = 0f;
                    carbonDioxide = 0.965f;
                    break;

                case AtmospherePreset.Custom:
                    break;
            }
        }

        public void ApplyToNode(AtmosphericNode node, float volume)
        {
            if (node == null || node.Mixture == null) return;

            float totalMoles = CalculateTotalMoles(volume);

            node.Mixture.Moles.Clear();

            if (nitrogen > 0)
                node.Mixture.Moles["N2"] = totalMoles * nitrogen;

            if (oxygen > 0)
                node.Mixture.Moles["O2"] = totalMoles * oxygen;

            if (carbonDioxide > 0)
                node.Mixture.Moles["CO2"] = totalMoles * carbonDioxide;

            node.Mixture.Temperature = globalTemperature;
            node.Mixture.Volume = volume;

            float molesInNode = node.Mixture.TotalMoles();
            if (molesInNode > 0f)
            {
                node.Thermal.InternalEnergyJ = molesInNode * 29.0f * globalTemperature;
            }
        }

        public float CalculateTotalMoles(float volume)
        {
            if (globalPressure <= 0f || globalTemperature <= 0f) return 0f;

            const float R = 8.314f;
            return (globalPressure * volume) / (R * globalTemperature);
        }

        public bool IsVacuum()
        {
            return globalPressure <= 0.01f;
        }

        public bool IsBreathable()
        {
            return oxygen >= 0.18f && oxygen <= 0.25f && 
                   globalPressure >= 50f && globalPressure <= 120f &&
                   globalTemperature >= 260f && globalTemperature <= 310f;
        }
    }
}
