using System;
using System.Collections.Generic;
using UnityEngine;

namespace Atmospherics.Core
{
    [Serializable]
    public class GasMixture : ISerializationCallbackReceiver
    {
        public float Temperature;  // Kelvin
        public float Volume = 1;       // m^3

        public const float R = 8.314f;

        // Serialized lists for Unity
        [SerializeField] private List<string> gasKeys = new List<string>();
        [SerializeField] private List<float> gasValues = new List<float>();

        // Runtime dictionary
        [NonSerialized] public Dictionary<string, float> Moles = new Dictionary<string, float>();

        public GasMixture(float pressure = 101.3f, float temperature = 293f, float volume = 1f)
        {
            Temperature = Mathf.Max(temperature, 0.1f);
            Volume = Mathf.Max(volume, 0.0001f);

            // default gases
            Moles = new Dictionary<string, float>
            {
                { "O2", 0.21f },
                { "N2", 0.78f },
                { "CO2", 0.01f }
            };

            float totalMoles = (pressure * Volume * 1000f) / (R * Temperature);
            NormalizeToTotalMoles(totalMoles);
        }

        // Unity Serialization
        public void OnBeforeSerialize()
        {
            if (gasKeys != null && gasValues != null && Moles != null)
            {
                gasKeys.Clear();
                gasValues.Clear();

                foreach (var kv in Moles)
                {
                    gasKeys.Add(kv.Key);
                    gasValues.Add(kv.Value);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            if (Moles == null) Moles = new Dictionary<string, float>();
            Moles.Clear();
            for (int i = 0; i < Mathf.Min(gasKeys.Count, gasValues.Count); i++)
                Moles[gasKeys[i]] = gasValues[i];
        }

        // Physics
        public float TotalMoles()
        {
            float sum = 0;
            foreach (var m in Moles.Values) sum += Mathf.Max(0f, m);
            return Mathf.Max(sum, 1e-12f);
        }

        public float GetPressure()
        {
            return (TotalMoles() * R * Temperature) / (Volume * 1000f);
        }

        public void SetPressure(float targetPressure)
        {
            float nTarget = (targetPressure * Volume * 1000f) / (R * Temperature);
            NormalizeToTotalMoles(nTarget);
        }

        public Dictionary<string, float> GetFractions()
        {
            float total = TotalMoles();
            var fractions = new Dictionary<string, float>();
            foreach (var kv in Moles) fractions[kv.Key] = kv.Value / total;
            return fractions;
        }

        public void NormalizeToTotalMoles(float targetMoles)
        {
            float current = TotalMoles();
            if (current < 1e-12f) current = 1e-12f;
            float factor = targetMoles / current;

            foreach (var key in new List<string>(Moles.Keys))
                Moles[key] = Mathf.Max(0f, Moles[key] * factor);
        }

        // Editor helper: rename gas
        public void RenameGas(string oldName, string newName)
        {
            if (!Moles.ContainsKey(oldName) || string.IsNullOrEmpty(newName)) return;
            float val = Moles[oldName];
            Moles.Remove(oldName);
            Moles[newName] = val;
        }

        // ----  Editing & Transfer  ---- //

        public void AddGas(string type, float moles)
        {
            if (moles <= 0f) return;

            if (!Moles.ContainsKey(type))
                Moles[type] = 0f;

            Moles[type] += moles;
        }

        public void RemoveGas(string type, float moles)
        {
            if (!Moles.ContainsKey(type)) return;
            if (moles <= 0f) return;

            Moles[type] = Mathf.Max(0f, Moles[type] - moles);
        }
    }
}
