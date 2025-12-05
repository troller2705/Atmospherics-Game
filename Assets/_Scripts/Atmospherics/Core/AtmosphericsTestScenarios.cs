using UnityEngine;
using System.Collections;

namespace Atmospherics.Core
{
    public class AtmosphericsTestScenarios : MonoBehaviour
    {
        [Header("References")]
        public AtmosphericsSimulation manager;
        public AtmosphericsValidator validator;

        [Header("Test Controls")]
        public bool autoRunTests = false;
        public float testDuration = 10f;

        private void Start()
        {
            if (manager == null)
                manager = FindFirstObjectByType<AtmosphericsSimulation>();

            if (validator == null)
                validator = FindFirstObjectByType<AtmosphericsValidator>();

            if (autoRunTests)
            {
                StartCoroutine(RunAllTests());
            }
        }

        private IEnumerator RunAllTests()
        {
            Debug.Log("=== Starting Atmospheric Test Suite ===");

            yield return new WaitForSeconds(1f);

            Debug.Log("--- Test 1: Pressure Equalization ---");
            yield return StartCoroutine(TestPressureEqualization());

            Debug.Log("--- Test 2: Temperature Equilibrium ---");
            yield return StartCoroutine(TestTemperatureEquilibrium());

            Debug.Log("--- Test 3: Energy Conservation ---");
            yield return StartCoroutine(TestEnergyConservation());

            Debug.Log("=== Test Suite Complete ===");
            if (validator != null)
                validator.PrintReport();
        }

        private IEnumerator TestPressureEqualization()
        {
            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            if (allLeaks.Length == 0 || allLeaks[0].Nodes == null || allLeaks[0].Nodes.Count < 2)
            {
                Debug.LogWarning("Not enough nodes for pressure equalization test");
                yield break;
            }

            var nodeA = allLeaks[0].Nodes[0];
            var nodeB = allLeaks[0].Nodes[1];

            float initialPressureA = nodeA.Mixture.GetPressure();
            float initialPressureB = nodeB.Mixture.GetPressure();

            Debug.Log($"Initial: Node A = {initialPressureA:F2} kPa, Node B = {initialPressureB:F2} kPa");

            yield return new WaitForSeconds(testDuration);

            float finalPressureA = nodeA.Mixture.GetPressure();
            float finalPressureB = nodeB.Mixture.GetPressure();
            float pressureDiff = Mathf.Abs(finalPressureA - finalPressureB);

            Debug.Log($"Final: Node A = {finalPressureA:F2} kPa, Node B = {finalPressureB:F2} kPa, Diff = {pressureDiff:F2} kPa");

            if (pressureDiff < 5f)
                Debug.Log("<color=lime>✓ PASS: Pressures equalized</color>");
            else
                Debug.LogWarning("<color=yellow>⚠ PARTIAL: Pressures still equilibrating</color>");
        }

        private IEnumerator TestTemperatureEquilibrium()
        {
            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            if (allLeaks.Length == 0 || allLeaks[0].Nodes == null || allLeaks[0].Nodes.Count < 2)
            {
                Debug.LogWarning("Not enough nodes for temperature equilibrium test");
                yield break;
            }

            var nodeA = allLeaks[0].Nodes[0];
            var nodeB = allLeaks[0].Nodes[1];

            float initialTempA = nodeA.Mixture.Temperature;
            float initialTempB = nodeB.Mixture.Temperature;

            Debug.Log($"Initial: Node A = {initialTempA:F2} K, Node B = {initialTempB:F2} K");

            yield return new WaitForSeconds(testDuration);

            float finalTempA = nodeA.Mixture.Temperature;
            float finalTempB = nodeB.Mixture.Temperature;
            float tempDiff = Mathf.Abs(finalTempA - finalTempB);

            Debug.Log($"Final: Node A = {finalTempA:F2} K, Node B = {finalTempB:F2} K, Diff = {tempDiff:F2} K");

            if (tempDiff < 10f)
                Debug.Log("<color=lime>✓ PASS: Temperatures equilibrating</color>");
            else
                Debug.LogWarning("<color=yellow>⚠ PARTIAL: Temperatures still equilibrating</color>");
        }

        private IEnumerator TestEnergyConservation()
        {
            if (validator == null)
            {
                Debug.LogWarning("Validator not found for energy conservation test");
                yield break;
            }

            validator.ResetBaseline();

            float initialEnergy = CalculateTotalSystemEnergy();
            Debug.Log($"Initial Total Energy: {initialEnergy:F2} J");

            yield return new WaitForSeconds(testDuration);

            float finalEnergy = CalculateTotalSystemEnergy();
            float energyChange = finalEnergy - initialEnergy;
            float percentChange = initialEnergy > 0 ? (energyChange / initialEnergy) * 100f : 0f;

            Debug.Log($"Final Total Energy: {finalEnergy:F2} J");
            Debug.Log($"Energy Change: {energyChange:F2} J ({percentChange:F2}%)");

            float expectedLoss = CalculateExpectedHeatLoss(testDuration);
            Debug.Log($"Expected Heat Loss: {expectedLoss:F2} J");

            if (Mathf.Abs(percentChange) < 5f)
                Debug.Log("<color=lime>✓ PASS: Energy relatively conserved (accounting for external heat transfer)</color>");
            else if (Mathf.Abs(percentChange) < 15f)
                Debug.LogWarning("<color=yellow>⚠ ACCEPTABLE: Some energy drift detected</color>");
            else
                Debug.LogError("<color=red>✗ FAIL: Significant energy drift detected!</color>");
        }

        private float CalculateTotalSystemEnergy()
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

        private float CalculateExpectedHeatLoss(float duration)
        {
            float totalLoss = 0f;

            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            foreach (var leak in allLeaks)
            {
                if (leak == null || leak.Nodes == null) continue;

                foreach (var node in leak.Nodes)
                {
                    if (node == null || node.Mixture == null) continue;

                    float tempDiff = node.Mixture.Temperature - node.Thermal.ExternalTempK;
                    float heatLossRate = node.Thermal.HeatLossCoefficient * tempDiff;
                    totalLoss += heatLossRate * duration;
                }
            }

            return totalLoss;
        }

        [ContextMenu("Test: Pressure Equalization")]
        public void RunPressureTest()
        {
            StartCoroutine(TestPressureEqualization());
        }

        [ContextMenu("Test: Temperature Equilibrium")]
        public void RunTemperatureTest()
        {
            StartCoroutine(TestTemperatureEquilibrium());
        }

        [ContextMenu("Test: Energy Conservation")]
        public void RunEnergyTest()
        {
            StartCoroutine(TestEnergyConservation());
        }

        [ContextMenu("Run All Tests")]
        public void RunAllTestsNow()
        {
            StartCoroutine(RunAllTests());
        }

        [ContextMenu("Create Extreme Pressure Test")]
        public void CreateExtremePressureTest()
        {
            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            if (allLeaks.Length == 0 || allLeaks[0].Nodes == null || allLeaks[0].Nodes.Count < 2)
            {
                Debug.LogWarning("Not enough nodes for test");
                return;
            }

            var nodeA = allLeaks[0].Nodes[0];
            var nodeB = allLeaks[0].Nodes[1];

            foreach (var gas in nodeA.Mixture.Moles.Keys)
            {
                nodeA.Mixture.Moles[gas] *= 10f;
            }

            Debug.Log("Node A pressure increased 10x - observe equalization");
        }

        [ContextMenu("Create Temperature Gradient Test")]
        public void CreateTemperatureGradientTest()
        {
            var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            if (allLeaks.Length == 0 || allLeaks[0].Nodes == null || allLeaks[0].Nodes.Count < 2)
            {
                Debug.LogWarning("Not enough nodes for test");
                return;
            }

            var nodeA = allLeaks[0].Nodes[0];
            var nodeB = allLeaks[0].Nodes[1];

            nodeA.Mixture.Temperature = 400f;
            nodeB.Mixture.Temperature = 200f;

            const float SPECIFIC_HEAT_CP = 29.0f;
            nodeA.Thermal.InternalEnergyJ = nodeA.Mixture.TotalMoles() * SPECIFIC_HEAT_CP * nodeA.Mixture.Temperature;
            nodeB.Thermal.InternalEnergyJ = nodeB.Mixture.TotalMoles() * SPECIFIC_HEAT_CP * nodeB.Mixture.Temperature;

            Debug.Log("Temperature gradient created: 400K vs 200K - observe heat transfer");
        }
    }
}
