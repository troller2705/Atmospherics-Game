using UnityEngine;

namespace Atmospherics.Core
{
    public class AtmosphericNode : MonoBehaviour
    {
        [Header("Identification")]
        public string NodeName;

        [Header("Gas Mixture")]
        public GasMixture Mixture;

        [Header("Thermal Properties")]
        public ThermalProperties Thermal;

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public float Volume => Mixture?.Volume ?? 0f;

        public float Pressure => Mixture?.GetPressure() ?? 0f;

        public float Temperature => Mixture?.Temperature ?? 0f;

        private void Awake()
        {
            if (Mixture == null)
            {
                Initialize(
                    string.IsNullOrEmpty(NodeName) ? gameObject.name : NodeName,
                    101.3f,
                    293f,
                    1f
                );
            }
            else
            {
                if (float.IsNaN(Thermal.InternalEnergyJ) || Thermal.InternalEnergyJ == 0f)
                {
                    RecalculateInternalEnergy();
                }
            }
        }

        public void Initialize(string name, float pressure = 101.3f, float temperature = 293f, float volume = 1f)
        {
            NodeName = name;
            Mixture = new GasMixture(pressure, temperature, volume);
            Thermal = ThermalProperties.Default();
            RecalculateInternalEnergy();
        }

        public void RecalculateInternalEnergy()
        {
            if (Mixture != null)
            {
                Thermal.RecalculateEnergy(Mixture.TotalMoles(), Mixture.Temperature);
            }
        }

        public void ClampToSafeValues()
        {
            if (Mixture == null) return;

            Mixture.Volume = Mathf.Max(Mixture.Volume, 0.0001f);
            Mixture.Temperature = Mathf.Max(Mixture.Temperature, 0.1f);

            foreach (var key in new System.Collections.Generic.List<string>(Mixture.Moles.Keys))
                Mixture.Moles[key] = Mathf.Max(Mixture.Moles[key], 0f);

            float p = Mixture.GetPressure();
            if (float.IsNaN(p) || float.IsInfinity(p) || p < 0.01f)
                Mixture.SetPressure(0.01f);
        }

        public void DebugStatus()
        {
            if (Mixture == null)
            {
                Debug.Log($"{NodeName} | NO MIXTURE");
                return;
            }

            string gases = "";
            foreach (var kv in Mixture.Moles)
                gases += $"{kv.Key}: {kv.Value:F2} ";

            Debug.Log($"{NodeName} | Pressure: {Pressure:F1} kPa | Temp: {Temperature:F1}K | Vol: {Volume:F2} m³ | Gases: {gases}");
        }
    }
}
