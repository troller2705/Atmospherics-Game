using System.Collections.Generic;
using UnityEngine;

namespace Atmospherics.Core
{
    public class LeakBehavior : MonoBehaviour
    {
        [Header("Leak Settings")]
        [Tooltip("Is this volume sealed from exterior?")]
        public bool IsSealed = true;

        [Tooltip("Fraction of total moles lost per second when unsealed")]
        [Range(0f, 1f)]
        public float LeakRateFractionPerSec = 0.01f;

        [Header("References")]
        [Tooltip("Nodes that share this leak volume")]
        public List<AtmosphericNode> Nodes = new List<AtmosphericNode>();

        [Tooltip("Exterior node to leak to (usually space/outside)")]
        public AtmosphericNode ExteriorNode;

        private const float MIN_MOLES = 1e-7f;
        private const float SPECIFIC_HEAT_CP = 29.0f;

        public void StepLeak(float deltaTime)
        {
            if (IsSealed) return;
            if (Nodes.Count == 0 || ExteriorNode == null) return;

            float leakFraction = Mathf.Clamp01(LeakRateFractionPerSec * deltaTime);

            var allGases = new HashSet<string>();
            foreach (var node in Nodes)
            {
                if (node != null && node.Mixture != null)
                    allGases.UnionWith(node.Mixture.Moles.Keys);
            }
            if (ExteriorNode.Mixture != null)
                allGases.UnionWith(ExteriorNode.Mixture.Moles.Keys);

            float totalMoles = 0f;
            float totalEnergy = 0f;
            var totalMolesPerGas = new Dictionary<string, float>();
            foreach (var gas in allGases) totalMolesPerGas[gas] = 0f;

            foreach (var node in Nodes)
            {
                if (node == null || node.Mixture == null) continue;

                float n = node.Mixture.TotalMoles();
                totalMoles += n;
                totalEnergy += n * node.Mixture.Temperature;

                foreach (var gas in allGases)
                {
                    float amount = node.Mixture.Moles.ContainsKey(gas) ? node.Mixture.Moles[gas] : 0f;
                    totalMolesPerGas[gas] += amount;
                }
            }

            if (ExteriorNode.Mixture != null)
            {
                float extMoles = ExteriorNode.Mixture.TotalMoles();
                totalMoles += extMoles;
                totalEnergy += extMoles * ExteriorNode.Mixture.Temperature;

                foreach (var gas in allGases)
                {
                    float extAmount = ExteriorNode.Mixture.Moles.ContainsKey(gas) ? ExteriorNode.Mixture.Moles[gas] : 0f;
                    totalMolesPerGas[gas] += extAmount;
                }
            }

            if (totalMoles <= 0f) return;

            float avgTemperature = totalEnergy / totalMoles;

            foreach (var node in Nodes)
            {
                if (node == null || node.Mixture == null) continue;

                var mix = node.Mixture;
                float n = mix.TotalMoles();

                foreach (var gas in allGases)
                {
                    float current = mix.Moles.ContainsKey(gas) ? mix.Moles[gas] : 0f;
                    float target = totalMolesPerGas[gas] * (n / totalMoles);
                    mix.Moles[gas] = Mathf.Lerp(current, target, leakFraction);
                }

                mix.Temperature = Mathf.Lerp(mix.Temperature, avgTemperature, leakFraction);
                node.Thermal.RecalculateEnergy(mix.TotalMoles(), mix.Temperature);
            }

            if (ExteriorNode.Mixture != null)
            {
                var extMix = ExteriorNode.Mixture;
                float extTotal = extMix.TotalMoles();

                foreach (var gas in allGases)
                {
                    float current = extMix.Moles.ContainsKey(gas) ? extMix.Moles[gas] : 0f;
                    float target = totalMolesPerGas[gas] * (extTotal / totalMoles);
                    extMix.Moles[gas] = Mathf.Lerp(current, target, leakFraction);
                }

                extMix.Temperature = Mathf.Lerp(extMix.Temperature, avgTemperature, leakFraction);
                ExteriorNode.Thermal.RecalculateEnergy(extMix.TotalMoles(), extMix.Temperature);
            }

            foreach (var node in Nodes)
            {
                if (node == null || node.Mixture == null) continue;
                float n = node.Mixture.TotalMoles();
                node.Mixture.SetPressure((n * GasMixture.R * node.Mixture.Temperature) / (node.Mixture.Volume * 1000f));
            }

            if (ExteriorNode.Mixture != null)
            {
                var extMix = ExteriorNode.Mixture;
                extMix.SetPressure((extMix.TotalMoles() * GasMixture.R * extMix.Temperature) / (extMix.Volume * 1000f));
            }
        }

        public void ToggleSeal()
        {
            IsSealed = !IsSealed;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsSealed ? Color.green : Color.red;

            foreach (var node in Nodes)
            {
                if (node == null) continue;
                Gizmos.DrawWireSphere(node.Position, 0.5f);
            }

            if (ExteriorNode != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(ExteriorNode.Position, 0.3f);

                foreach (var node in Nodes)
                {
                    if (node == null) continue;
                    Gizmos.DrawLine(node.Position, ExteriorNode.Position);
                }
            }
        }
#endif
    }
}
