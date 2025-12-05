using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Devices
{
    public class CO2Scrubber : MonoBehaviour
    {
        [Header("References")]
        public AtmosphericNode targetNode;

        [Header("Scrubber Settings")]
        [Tooltip("Moles of CO2 removed per second when active")]
        public float scrubRate = 0.1f;

        [Tooltip("Does the scrubber produce O2 as a byproduct?")]
        public bool producesOxygen = true;

        [Tooltip("Moles of O2 produced per mole of CO2 removed")]
        public float oxygenConversionRatio = 0.5f;

        [Header("Power")]
        public bool requiresPower = true;
        public bool isPowered = true;
        public float powerConsumptionWatts = 100f;

        [Header("Status")]
        public bool isActive = true;
        public bool isOperational = true;
        public float totalCO2Scrubbed = 0f;
        public float totalO2Produced = 0f;

        [Header("Audio (Optional)")]
        public AudioSource scrubberSound;

        private const float SPECIFIC_HEAT_CP = 29.0f;

        private void Start()
        {
            if (targetNode == null)
            {
                Debug.LogWarning($"[CO2Scrubber] {gameObject.name} has no target node assigned!");
            }
        }

        public void UpdateScrubber(float dt)
        {
            if (!isActive) { return; }
            if (targetNode == null || targetNode.Mixture == null) { isOperational = false; return; }
            if (requiresPower && !isPowered) { return; }

            if (!targetNode.Mixture.Moles.ContainsKey("CO2")) { isOperational = false; return; }

            float currentCO2 = targetNode.Mixture.Moles["CO2"];
            if (currentCO2 <= 0f) { isOperational = false; return; }

            float co2ToRemove = Mathf.Min(scrubRate * dt, currentCO2);
            if (co2ToRemove <= 0f) { isOperational = false; return; }

            targetNode.Mixture.Moles["CO2"] -= co2ToRemove;
            totalCO2Scrubbed += co2ToRemove;

            if (producesOxygen)
            {
                float o2ToAdd = co2ToRemove * oxygenConversionRatio;

                if (!targetNode.Mixture.Moles.ContainsKey("O2"))
                    targetNode.Mixture.Moles["O2"] = 0f;

                targetNode.Mixture.Moles["O2"] += o2ToAdd;
                totalO2Produced += o2ToAdd;
            }

            float newTotalMoles = targetNode.Mixture.TotalMoles();
            if (newTotalMoles > 0f)
            {
                targetNode.Mixture.Temperature = targetNode.Thermal.InternalEnergyJ / (newTotalMoles * SPECIFIC_HEAT_CP);
                targetNode.Mixture.Temperature = Mathf.Max(targetNode.Mixture.Temperature, 0.1f);
            }
        }

        public void SetPowered(bool powered)
        {
            isPowered = powered;
            if (scrubberSound != null)
            {
                if (isPowered && isActive)
                {
                    if (!scrubberSound.isPlaying)
                        scrubberSound.Play();
                }
                else
                {
                    scrubberSound.Stop();
                }
            }
        }

        public void ToggleActive()
        {
            isActive = !isActive;
            SetPowered(isPowered);
        }

        public float GetCO2PercentInNode()
        {
            if (targetNode == null || targetNode.Mixture == null) return 0f;

            float totalMoles = targetNode.Mixture.TotalMoles();
            if (totalMoles <= 0f) return 0f;

            if (!targetNode.Mixture.Moles.ContainsKey("CO2")) return 0f;

            return (targetNode.Mixture.Moles["CO2"] / totalMoles) * 100f;
        }

        public string GetStatusText()
        {
            if (!isActive) return "Inactive";
            if (requiresPower && !isPowered) return "No Power";
            if (targetNode == null) return "No Node";

            float co2Percent = GetCO2PercentInNode();
            return $"Active | CO₂: {co2Percent:F2}%";
        }

        private void OnDrawGizmosSelected()
        {
            if (targetNode != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetNode.transform.position);
                Gizmos.DrawWireSphere(targetNode.transform.position, 0.3f);
            }
        }
    }
}

