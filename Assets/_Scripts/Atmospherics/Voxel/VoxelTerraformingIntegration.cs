using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Voxel
{
    public class VoxelTerraformingIntegration : MonoBehaviour
    {
        [Header("References")]
        public VoxelAtmosphericBridge atmosphericBridge;
        public VoxelZoneDetector zoneDetector;

        [Header("Terraforming Effects")]
        public bool enableAtmosphericLeaks = true;
        public float leakRatePerVoxel = 0.01f;
        public bool enablePressureBasedBreaking = true;
        public float breakPressureThreshold = 500f;

        [Header("Gas Release Settings")]
        public bool releaseGasOnVoxelDestruction = true;
        public float gasReleaseMultiplier = 1f;

        [Header("Environmental Effects")]
        public bool enableTemperatureTransfer = true;
        public float heatConductivity = 0.1f;
        public bool enableVacuumDamage = true;

        private void Start()
        {
            if (atmosphericBridge == null)
            {
                atmosphericBridge = GetComponent<VoxelAtmosphericBridge>();
            }

            if (zoneDetector == null)
            {
                zoneDetector = GetComponent<VoxelZoneDetector>();
            }

            if (atmosphericBridge == null)
            {
                Debug.LogWarning("VoxelTerraformingIntegration: No VoxelAtmosphericBridge found!");
            }
        }

        public void OnVoxelDestroyed(Vector3Int voxelPosition)
        {
            if (atmosphericBridge == null) return;

            var voxelData = atmosphericBridge.GetVoxel(voxelPosition);
            
            if (releaseGasOnVoxelDestruction && voxelData.type == VoxelAtmosphericBridge.VoxelType.Solid)
            {
                ReleaseTrappedGas(voxelPosition);
            }

            atmosphericBridge.SetVoxel(voxelPosition, VoxelAtmosphericBridge.VoxelType.Empty);

            if (zoneDetector != null)
            {
                zoneDetector.CheckForBreach(voxelPosition);
            }
        }

        public void OnVoxelCreated(Vector3Int voxelPosition, VoxelAtmosphericBridge.VoxelType type = VoxelAtmosphericBridge.VoxelType.Solid)
        {
            if (atmosphericBridge == null) return;

            atmosphericBridge.SetVoxel(voxelPosition, type, 1f, true);

            if (zoneDetector != null && type == VoxelAtmosphericBridge.VoxelType.Solid)
            {
                zoneDetector.CheckForSealing(voxelPosition);
            }
        }

        public void OnVoxelDamaged(Vector3Int voxelPosition, float damageAmount)
        {
            if (atmosphericBridge == null) return;

            var voxelData = atmosphericBridge.GetVoxel(voxelPosition);
            
            if (voxelData.type == VoxelAtmosphericBridge.VoxelType.Solid)
            {
                float newDensity = voxelData.density - (damageAmount * 0.1f);
                
                if (newDensity <= 0f)
                {
                    OnVoxelDestroyed(voxelPosition);
                }
                else if (newDensity < 0.5f)
                {
                    atmosphericBridge.SetVoxel(voxelPosition, VoxelAtmosphericBridge.VoxelType.Partial, newDensity, false);
                    CreateLeak(voxelPosition);
                }
                else
                {
                    atmosphericBridge.SetVoxel(voxelPosition, VoxelAtmosphericBridge.VoxelType.Solid, newDensity, true);
                }
            }
        }

        private void ReleaseTrappedGas(Vector3Int voxelPosition)
        {
            AtmosphericNode nearbyNode = FindNearestNode(voxelPosition);
            if (nearbyNode == null || nearbyNode.Mixture == null) return;

            float gasAmount = atmosphericBridge.volumePerVoxel * gasReleaseMultiplier * 0.1f;

            if (!nearbyNode.Mixture.Moles.ContainsKey("CO2"))
                nearbyNode.Mixture.Moles["CO2"] = 0f;

            nearbyNode.Mixture.Moles["CO2"] += gasAmount;

            float totalMoles = nearbyNode.Mixture.TotalMoles();
            if (totalMoles > 0f)
            {
                nearbyNode.Mixture.Temperature = nearbyNode.Thermal.InternalEnergyJ / (totalMoles * 29.0f);
            }
        }

        private void CreateLeak(Vector3Int voxelPosition)
        {
            if (!enableAtmosphericLeaks) return;

            AtmosphericNode node = atmosphericBridge.GetNodeAtVoxel(voxelPosition);
            if (node == null) return;

            LeakBehavior zone = node.GetComponentInParent<LeakBehavior>();
            if (zone != null)
            {
                zone.IsSealed = false;
                zone.LeakRateFractionPerSec = Mathf.Max(zone.LeakRateFractionPerSec, leakRatePerVoxel);
            }
        }

        private AtmosphericNode FindNearestNode(Vector3Int voxelPosition)
        {
            AtmosphericNode directNode = atmosphericBridge.GetNodeAtVoxel(voxelPosition);
            if (directNode != null) return directNode;

            Vector3 worldPos = atmosphericBridge.VoxelToWorldPosition(voxelPosition);
            AtmosphericNode[] allNodes = FindObjectsByType<AtmosphericNode>(FindObjectsSortMode.None);
            
            AtmosphericNode nearest = null;
            float minDistance = float.MaxValue;

            foreach (AtmosphericNode node in allNodes)
            {
                float distance = Vector3.Distance(worldPos, node.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = node;
                }
            }

            return nearest;
        }

        public bool CheckPressureDamage(Vector3Int voxelPosition, out float damage)
        {
            damage = 0f;

            if (!enablePressureBasedBreaking) return false;

            AtmosphericNode node = atmosphericBridge.GetNodeAtVoxel(voxelPosition);
            if (node == null || node.Mixture == null) return false;

            float pressure = node.Mixture.GetPressure();
            
            if (pressure > breakPressureThreshold)
            {
                damage = (pressure - breakPressureThreshold) / breakPressureThreshold;
                return true;
            }

            return false;
        }

        public void ApplyTemperatureToVoxel(Vector3Int voxelPosition, float temperature)
        {
            if (!enableTemperatureTransfer) return;

            AtmosphericNode node = atmosphericBridge.GetNodeAtVoxel(voxelPosition);
            if (node == null || node.Mixture == null) return;

            float tempDifference = temperature - node.Mixture.Temperature;
            float heatTransfer = tempDifference * heatConductivity * Time.deltaTime;

            float totalMoles = node.Mixture.TotalMoles();
            if (totalMoles > 0f)
            {
                node.Thermal.InternalEnergyJ += heatTransfer * totalMoles * 29.0f;
                node.Mixture.Temperature = node.Thermal.InternalEnergyJ / (totalMoles * 29.0f);
                node.Mixture.Temperature = Mathf.Max(node.Mixture.Temperature, 0.1f);
            }
        }

        public float GetAtmosphericPressure(Vector3 worldPosition)
        {
            AtmosphericNode node = atmosphericBridge.GetNodeAtPosition(worldPosition);
            if (node == null || node.Mixture == null)
                return 0f;

            return node.Mixture.GetPressure();
        }

        public float GetAtmosphericTemperature(Vector3 worldPosition)
        {
            AtmosphericNode node = atmosphericBridge.GetNodeAtPosition(worldPosition);
            if (node == null || node.Mixture == null)
                return 0f;

            return node.Mixture.Temperature;
        }

        public bool IsPositionBreathable(Vector3 worldPosition)
        {
            AtmosphericNode node = atmosphericBridge.GetNodeAtPosition(worldPosition);
            if (node == null || node.Mixture == null)
                return false;

            float totalMoles = node.Mixture.TotalMoles();
            if (totalMoles <= 0f) return false;

            float o2Percent = 0f;
            if (node.Mixture.Moles.ContainsKey("O2"))
            {
                o2Percent = (node.Mixture.Moles["O2"] / totalMoles) * 100f;
            }

            float pressure = node.Mixture.GetPressure();

            return o2Percent >= 16f && pressure >= 20f && pressure <= 200f;
        }
    }
}
