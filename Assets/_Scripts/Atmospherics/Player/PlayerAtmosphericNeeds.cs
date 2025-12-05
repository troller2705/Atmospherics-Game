using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Player
{
    public class PlayerAtmosphericNeeds : MonoBehaviour
    {
        [Header("Current Atmosphere")]
        public AtmosphericNode currentNode;
        public AtmosphericHazards currentHazards;

        [Header("Breathing Rates (moles/sec)")]
        public float oxygenConsumptionRate = 0.01f;
        public float co2ProductionRate = 0.008f;

        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        
        [Header("Damage Rates (HP/sec)")]
        public float hypoxiaDamage = 5f;
        public float co2ToxicityDamage = 10f;
        public float extremeColdDamage = 15f;
        public float extremeHeatDamage = 15f;
        public float vacuumDamage = 20f;

        [Header("Stamina Settings")]
        public float maxStamina = 100f;
        public float currentStamina = 100f;
        public float staminaRegenRate = 10f;
        public float lowOxygenStaminaPenalty = 20f;

        [Header("Temperature Tolerance")]
        public float comfortableMinTemp = 273f;
        public float comfortableMaxTemp = 310f;
        public float extremeColdTemp = 250f;
        public float extremeHeatTemp = 340f;

        [Header("Status")]
        public bool isBreathing = true;
        public bool isSuffocating = false;
        public bool isTakingDamage = false;
        public string currentStatus = "Normal";

        [Header("Events")]
        public UnityEngine.Events.UnityEvent onStartSuffocating;
        public UnityEngine.Events.UnityEvent onStopSuffocating;
        public UnityEngine.Events.UnityEvent onDeath;

        private const float SPECIFIC_HEAT_CP = 29.0f;
        private bool wasSuffocating = false;

        private void Start()
        {
            currentHealth = maxHealth;
            currentStamina = maxStamina;
        }

        private void Update()
        {
            if (currentNode == null || !isBreathing)
            {
                UpdateVacuumEffects(Time.deltaTime);
                return;
            }

            UpdateBreathing(Time.deltaTime);
            UpdateHealthEffects(Time.deltaTime);
            UpdateStaminaEffects(Time.deltaTime);
        }

        private void UpdateBreathing(float dt)
        {
            if (currentNode.Mixture == null) return;

            float o2Available = 0f;
            if (currentNode.Mixture.Moles.ContainsKey("O2"))
                o2Available = currentNode.Mixture.Moles["O2"];

            float o2ToConsume = oxygenConsumptionRate * dt;
            o2ToConsume = Mathf.Min(o2ToConsume, o2Available);

            if (o2ToConsume > 0f)
            {
                currentNode.Mixture.Moles["O2"] -= o2ToConsume;

                if (!currentNode.Mixture.Moles.ContainsKey("CO2"))
                    currentNode.Mixture.Moles["CO2"] = 0f;
                
                currentNode.Mixture.Moles["CO2"] += co2ProductionRate * dt;

                float totalMoles = currentNode.Mixture.TotalMoles();
                if (totalMoles > 0f)
                {
                    currentNode.Mixture.Temperature = currentNode.Thermal.InternalEnergyJ / (totalMoles * SPECIFIC_HEAT_CP);
                    currentNode.Mixture.Temperature = Mathf.Max(currentNode.Mixture.Temperature, 0.1f);
                }
            }
        }

        private void UpdateHealthEffects(float dt)
        {
            if (currentNode == null || currentNode.Mixture == null)
            {
                UpdateVacuumEffects(dt);
                return;
            }

            isTakingDamage = false;
            isSuffocating = false;
            currentStatus = "Normal";

            float totalMoles = currentNode.Mixture.TotalMoles();
            if (totalMoles <= 0f)
            {
                UpdateVacuumEffects(dt);
                return;
            }

            float o2Percent = GetGasPercent("O2");
            float co2Percent = GetGasPercent("CO2");
            float temperature = currentNode.Mixture.Temperature;
            float pressure = currentNode.Mixture.GetPressure();

            if (pressure < 20f)
            {
                ApplyDamage(vacuumDamage * dt);
                currentStatus = "VACUUM";
                isSuffocating = true;
            }
            else if (o2Percent < 10f)
            {
                ApplyDamage(hypoxiaDamage * dt);
                currentStatus = "HYPOXIA";
                isSuffocating = true;
            }
            else if (o2Percent < 18f)
            {
                ApplyDamage(hypoxiaDamage * 0.3f * dt);
                currentStatus = "Low Oxygen";
                isSuffocating = true;
            }

            if (co2Percent > 5f)
            {
                ApplyDamage(co2ToxicityDamage * dt);
                currentStatus = "CO₂ POISONING";
                isTakingDamage = true;
            }
            else if (co2Percent > 3f)
            {
                ApplyDamage(co2ToxicityDamage * 0.5f * dt);
                currentStatus = "High CO₂";
                isTakingDamage = true;
            }

            if (temperature < extremeColdTemp)
            {
                ApplyDamage(extremeColdDamage * dt);
                currentStatus = "FREEZING";
                isTakingDamage = true;
            }
            else if (temperature > extremeHeatTemp)
            {
                ApplyDamage(extremeHeatDamage * dt);
                currentStatus = "BURNING";
                isTakingDamage = true;
            }

            if (isSuffocating && !wasSuffocating)
            {
                onStartSuffocating?.Invoke();
            }
            else if (!isSuffocating && wasSuffocating)
            {
                onStopSuffocating?.Invoke();
            }

            wasSuffocating = isSuffocating;

            if (!isTakingDamage && !isSuffocating)
            {
                currentStatus = "Normal";
            }
        }

        private void UpdateVacuumEffects(float dt)
        {
            ApplyDamage(vacuumDamage * dt);
            currentStatus = "VACUUM - NO ATMOSPHERE";
            isSuffocating = true;
            isTakingDamage = true;

            if (!wasSuffocating)
            {
                onStartSuffocating?.Invoke();
                wasSuffocating = true;
            }
        }

        private void UpdateStaminaEffects(float dt)
        {
            if (currentNode == null || currentNode.Mixture == null)
            {
                currentStamina = Mathf.Max(0f, currentStamina - lowOxygenStaminaPenalty * dt);
                return;
            }

            float o2Percent = GetGasPercent("O2");

            if (o2Percent < 18f)
            {
                currentStamina = Mathf.Max(0f, currentStamina - lowOxygenStaminaPenalty * dt);
            }
            else
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * dt);
            }
        }

        private void ApplyDamage(float damage)
        {
            currentHealth -= damage;
            isTakingDamage = true;

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                OnPlayerDeath();
            }
        }

        private void OnPlayerDeath()
        {
            Debug.Log("[Player] Death by atmospheric conditions!");
            onDeath?.Invoke();
        }

        private float GetGasPercent(string gasName)
        {
            if (currentNode == null || currentNode.Mixture == null)
                return 0f;

            float totalMoles = currentNode.Mixture.TotalMoles();
            if (totalMoles <= 0f) return 0f;

            if (!currentNode.Mixture.Moles.ContainsKey(gasName))
                return 0f;

            return (currentNode.Mixture.Moles[gasName] / totalMoles) * 100f;
        }

        public void SetCurrentNode(AtmosphericNode node)
        {
            currentNode = node;
            
            if (node != null)
            {
                currentHazards = node.GetComponent<AtmosphericHazards>();
            }
            else
            {
                currentHazards = null;
            }
        }

        public AtmosphericReadings GetCurrentReadings()
        {
            if (currentNode == null || currentNode.Mixture == null)
            {
                return new AtmosphericReadings
                {
                    pressure = 0f,
                    temperature = 0f,
                    oxygenPercent = 0f,
                    co2Percent = 0f,
                    nitrogenPercent = 0f,
                    totalMoles = 0f,
                    hazardLevel = AtmosphericHazards.HazardLevel.Critical
                };
            }

            return new AtmosphericReadings
            {
                pressure = currentNode.Mixture.GetPressure(),
                temperature = currentNode.Mixture.Temperature,
                oxygenPercent = GetGasPercent("O2"),
                co2Percent = GetGasPercent("CO2"),
                nitrogenPercent = GetGasPercent("N2"),
                totalMoles = currentNode.Mixture.TotalMoles(),
                hazardLevel = currentHazards != null ? currentHazards.currentHazardLevel : AtmosphericHazards.HazardLevel.Critical
            };
        }

        public string GetStatusText()
        {
            return $"{currentStatus}\nHealth: {currentHealth:F0}/{maxHealth:F0}\nStamina: {currentStamina:F0}/{maxStamina:F0}";
        }

        public float GetHealthPercent()
        {
            return currentHealth / maxHealth;
        }

        public float GetStaminaPercent()
        {
            return currentStamina / maxStamina;
        }
    }
}
