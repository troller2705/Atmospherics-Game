using UnityEngine;
using Atmospherics.Core;

public class HeatTransfer : MonoBehaviour
{
    [Header("Settings")]
    public float pipeThermalConductance = 10f;

    [Header("Auto-Discovery")]
    public bool autoDiscovery = true;

    private AtmosphericNode[] nodes;
    private GasConnection[] connections;

    private const float SPECIFIC_HEAT_CP = 29.0f;

    private void Start()
    {
        RefreshComponents();
    }

    private void RefreshComponents()
    {
        nodes = FindObjectsByType<AtmosphericNode>(FindObjectsSortMode.None);
        connections = FindObjectsByType<GasConnection>(FindObjectsSortMode.None);
    }

    public void StepHeat(float dt)
    {
        if (autoDiscovery)
            RefreshComponents();

        if (nodes == null) return;

        foreach (var n in nodes)
        {
            if (n == null) continue;
            SyncEnergyFromTemperature(n);
        }

        foreach (var n in nodes)
        {
            if (n == null) continue;
            float q = -n.Thermal.HeatLossCoefficient * (n.Mixture.Temperature - n.Thermal.ExternalTempK) * dt;
            n.Thermal.InternalEnergyJ += q;
        }

        if (connections != null)
        {
            foreach (var conn in connections)
            {
                if (conn == null || !conn.IsValid) continue;
                float Ta = conn.NodeA.Mixture.Temperature;
                float Tb = conn.NodeB.Mixture.Temperature;
                float dQ = pipeThermalConductance * (Ta - Tb) * dt;
                conn.NodeA.Thermal.InternalEnergyJ -= dQ;
                conn.NodeB.Thermal.InternalEnergyJ += dQ;
            }
        }

        foreach (var n in nodes)
        {
            if (n == null) continue;
            SyncTemperatureFromEnergy(n);
        }
    }

    private void SyncEnergyFromTemperature(AtmosphericNode node)
    {
        if (node == null || node.Mixture == null) return;

        float moles = node.Mixture.TotalMoles();
        if (moles > 0f)
        {
            node.Thermal.InternalEnergyJ = moles * SPECIFIC_HEAT_CP * node.Mixture.Temperature;
        }
    }

    private void SyncTemperatureFromEnergy(AtmosphericNode node)
    {
        if (node == null || node.Mixture == null) return;

        float moles = node.Mixture.TotalMoles();
        if (moles > 0f)
        {
            node.Mixture.Temperature = node.Thermal.InternalEnergyJ / (SPECIFIC_HEAT_CP * moles);
            node.Mixture.Temperature = Mathf.Max(node.Mixture.Temperature, 0.1f);
        }
    }
}
