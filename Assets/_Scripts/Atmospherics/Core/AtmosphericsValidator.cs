using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Atmospherics.Core
{
    public class AtmosphericsValidator : MonoBehaviour
    {
        [Header("References")]
        public AtmosphericsSimulation manager;

        [Header("Validation Settings")]
        public bool enableValidation = true;
        public bool logEnergyDrift = true;
        public float energyDriftThreshold = 1000f;
        public float validationInterval = 1f;

        [Header("Debug Display")]
        public bool showDebugGUI = true;
        public bool showDetailedStats = false;

        private float totalEnergyLastFrame;
        private float totalMolesLastFrame;
        private float validationTimer;
        private List<string> validationWarnings = new List<string>();

        private const float SPECIFIC_HEAT_CP = 29.0f;

        private void Start()
        {
            if (manager == null)
                manager = GetComponent<AtmosphericsSimulation>();

            totalEnergyLastFrame = CalculateTotalEnergy();
            totalMolesLastFrame = CalculateTotalMoles();
        }

        private void Update()
        {
            if (!enableValidation || manager == null) return;

            validationTimer += Time.deltaTime;

            if (validationTimer >= validationInterval)
            {
                ValidateSystem();
                validationTimer = 0f;
            }
        }

        private void ValidateSystem()
        {
            validationWarnings.Clear();

            float currentTotalEnergy = CalculateTotalEnergy();
            float currentTotalMoles = CalculateTotalMoles();

            float energyDrift = Mathf.Abs(currentTotalEnergy - totalEnergyLastFrame);
            float moleDrift = Mathf.Abs(currentTotalMoles - totalMolesLastFrame);

            if (energyDrift > energyDriftThreshold && logEnergyDrift)
            {
                string warning = $"Energy drift detected: {energyDrift:F1} J over {validationInterval}s (Δ{energyDrift / validationInterval:F1} J/s)";
                validationWarnings.Add(warning);
                Debug.LogWarning($"[Atmospherics Validator] {warning}");
            }

            ValidateNodeStates();

            totalEnergyLastFrame = currentTotalEnergy;
            totalMolesLastFrame = currentTotalMoles;
        }

        private void ValidateNodeStates()
        {
            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            foreach (var leak in allLeaks)
            {
                if (leak == null || leak.Nodes == null) continue;

                foreach (var node in leak.Nodes)
                {
                    if (node == null || node.Mixture == null) continue;

                    if (float.IsNaN(node.Mixture.Temperature))
                    {
                        validationWarnings.Add($"{node.NodeName}: Temperature is NaN!");
                        Debug.LogError($"[Validator] {node.NodeName} has NaN temperature!");
                    }

                    if (float.IsNaN(node.Thermal.InternalEnergyJ))
                    {
                        validationWarnings.Add($"{node.NodeName}: Internal Energy is NaN!");
                        Debug.LogError($"[Validator] {node.NodeName} has NaN internal energy!");
                    }

                    if (node.Mixture.Temperature < 0f)
                    {
                        validationWarnings.Add($"{node.NodeName}: Negative temperature {node.Mixture.Temperature:F1}K!");
                    }

                    if (node.Thermal.InternalEnergyJ < 0f)
                    {
                        validationWarnings.Add($"{node.NodeName}: Negative energy {node.Thermal.InternalEnergyJ:F1}J!");
                    }

                    float calculatedEnergy = node.Mixture.TotalMoles() * SPECIFIC_HEAT_CP * node.Mixture.Temperature;
                    float energyMismatch = Mathf.Abs(calculatedEnergy - node.Thermal.InternalEnergyJ);

                    if (energyMismatch > 100f && node.Mixture.TotalMoles() > 0.1f)
                    {
                        validationWarnings.Add($"{node.NodeName}: Energy/Temp mismatch {energyMismatch:F1}J (E={node.Thermal.InternalEnergyJ:F1}, calc={calculatedEnergy:F1})");
                    }
                }
            }
        }

        private float CalculateTotalEnergy()
        {
            float total = 0f;

            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            foreach (var leak in allLeaks)
            {
                if (leak == null || leak.Nodes == null) continue;

                foreach (var node in leak.Nodes)
                {
                    if (node == null) continue;
                    total += node.Thermal.InternalEnergyJ;
                }
            }

            return total;
        }

        private float CalculateTotalMoles()
        {
            float total = 0f;

            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            foreach (var leak in allLeaks)
            {
                if (leak == null || leak.Nodes == null) continue;

                foreach (var node in leak.Nodes)
                {
                    if (node == null || node.Mixture == null) continue;
                    total += node.Mixture.TotalMoles();
                }
            }

            return total;
        }

        private void OnGUI()
        {
            if (!showDebugGUI) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 12;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;

            GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label("<b>Atmospheric System Monitor</b>", labelStyle);
            GUILayout.Space(5);

            float totalEnergy = CalculateTotalEnergy();
            float totalMoles = CalculateTotalMoles();
            float avgTemp = totalMoles > 0f ? totalEnergy / (totalMoles * SPECIFIC_HEAT_CP) : 0f;

            GUILayout.Label($"Total Energy: <color=cyan>{totalEnergy:F1} J</color>", labelStyle);
            GUILayout.Label($"Total Moles: <color=cyan>{totalMoles:F2} mol</color>", labelStyle);
            GUILayout.Label($"Avg Temperature: <color=cyan>{avgTemp:F1} K</color>", labelStyle);

            GUILayout.Space(5);

            if (validationWarnings.Count > 0)
            {
                GUILayout.Label($"<color=red><b>⚠ Warnings ({validationWarnings.Count})</b></color>", labelStyle);
                foreach (var warning in validationWarnings)
                {
                    GUILayout.Label($"<color=yellow>• {warning}</color>", labelStyle);
                }
            }
            else
            {
                GUILayout.Label("<color=lime>✓ System Stable</color>", labelStyle);
            }

            if (showDetailedStats)
            {
                GUILayout.Space(10);
                GUILayout.Label("<b>Node Details:</b>", labelStyle);

                var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
                foreach (var leak in allLeaks)
                {
                    if (leak == null || leak.Nodes == null) continue;

                    foreach (var node in leak.Nodes)
                    {
                        if (node == null || node.Mixture == null) continue;

                        float pressure = node.Mixture.GetPressure();
                        float temp = node.Mixture.Temperature;
                        float moles = node.Mixture.TotalMoles();
                        float energy = node.Thermal.InternalEnergyJ;

                        GUILayout.Label($"{node.NodeName}: {pressure:F1}kPa, {temp:F1}K, {moles:F2}mol, {energy:F0}J", labelStyle);
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public string GenerateReport()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== Atmospheric System Validation Report ===");
            report.AppendLine($"Time: {Time.time:F2}s");
            report.AppendLine();

            float totalEnergy = CalculateTotalEnergy();
            float totalMoles = CalculateTotalMoles();

            report.AppendLine($"Total System Energy: {totalEnergy:F2} J");
            report.AppendLine($"Total System Moles: {totalMoles:F4} mol");
            report.AppendLine();

            report.AppendLine("Node Details:");
            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            foreach (var leak in allLeaks)
            {
                if (leak == null || leak.Nodes == null) continue;

                foreach (var node in leak.Nodes)
                {
                    if (node == null || node.Mixture == null) continue;

                    report.AppendLine($"  {node.NodeName}:");
                    report.AppendLine($"    Pressure: {node.Mixture.GetPressure():F2} kPa");
                    report.AppendLine($"    Temperature: {node.Mixture.Temperature:F2} K");
                    report.AppendLine($"    Volume: {node.Mixture.Volume:F2} m³");
                    report.AppendLine($"    Total Moles: {node.Mixture.TotalMoles():F4} mol");
                    report.AppendLine($"    Internal Energy: {node.Thermal.InternalEnergyJ:F2} J");

                    report.AppendLine("    Gas Composition:");
                    var fractions = node.Mixture.GetFractions();
                    foreach (var kv in fractions)
                    {
                        report.AppendLine($"      {kv.Key}: {kv.Value * 100:F2}%");
                    }
                }
            }

            if (validationWarnings.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("Warnings:");
                foreach (var warning in validationWarnings)
                {
                    report.AppendLine($"  • {warning}");
                }
            }

            return report.ToString();
        }

        [ContextMenu("Print Validation Report")]
        public void PrintReport()
        {
            Debug.Log(GenerateReport());
        }

        [ContextMenu("Reset Energy Baseline")]
        public void ResetBaseline()
        {
            totalEnergyLastFrame = CalculateTotalEnergy();
            totalMolesLastFrame = CalculateTotalMoles();
            validationWarnings.Clear();
            Debug.Log("[Validator] Baseline reset");
        }
    }
}
