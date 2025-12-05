using UnityEngine;

namespace Atmospherics.Core
{
    public class AtmosphericsSimulation : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [Tooltip("Time between simulation steps (seconds)")]
        public float SimulationTick = 0.1f;

        [Header("Auto-Discovery")]
        [Tooltip("Automatically find all components each frame")]
        public bool AutoDiscovery = true;

        [Tooltip("How often to refresh component lists (seconds)")]
        public float DiscoveryInterval = 1f;

        private float tickTimer = 0f;
        private float discoveryTimer = 0f;

        private GasConnection[] connections;
        private LeakBehavior[] leaks;
        private AtmosphericNode[] nodes;
        private Devices.CO2Scrubber[] scrubbers;
        private Devices.GasCanister[] canisters;
        private AtmosphericHazards[] hazards;
        private HeatTransfer heatTransfer;

        [Header("Debug")]
        public bool ShowDebugInfo = false;
        public int ActiveConnections = 0;
        public int ActiveLeaks = 0;
        public int TotalNodes = 0;

        private void Start()
        {
            RefreshComponents();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (AutoDiscovery)
            {
                discoveryTimer += Time.deltaTime;
                if (discoveryTimer >= DiscoveryInterval)
                {
                    RefreshComponents();
                    discoveryTimer = 0f;
                }
            }

            tickTimer += Time.deltaTime;

            while (tickTimer >= SimulationTick)
            {
                StepSimulation(SimulationTick);
                tickTimer -= SimulationTick;
            }
        }

        private void RefreshComponents()
        {
            connections = FindObjectsByType<GasConnection>(FindObjectsSortMode.None);
            leaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
            nodes = FindObjectsByType<AtmosphericNode>(FindObjectsSortMode.None);
            scrubbers = FindObjectsByType<Devices.CO2Scrubber>(FindObjectsSortMode.None);
            canisters = FindObjectsByType<Devices.GasCanister>(FindObjectsSortMode.None);
            hazards = FindObjectsByType<AtmosphericHazards>(FindObjectsSortMode.None);
            heatTransfer = FindFirstObjectByType<HeatTransfer>();

            ActiveConnections = connections?.Length ?? 0;
            ActiveLeaks = leaks?.Length ?? 0;
            TotalNodes = nodes?.Length ?? 0;

            if (ShowDebugInfo)
            {
                Debug.Log($"[Atmospherics] Refreshed: {TotalNodes} nodes, {ActiveConnections} connections, {ActiveLeaks} leaks");
            }
        }

        private void StepSimulation(float deltaTime)
        {
            if (heatTransfer != null)
                heatTransfer.StepHeat(deltaTime);

            if (connections != null)
            {
                foreach (var connection in connections)
                {
                    if (connection != null)
                        connection.UpdateConnection(deltaTime);
                }
            }

            if (leaks != null)
            {
                foreach (var leak in leaks)
                {
                    if (leak != null)
                        leak.StepLeak(deltaTime);
                }
            }

            if (scrubbers != null)
            {
                foreach (var scrubber in scrubbers)
                {
                    if (scrubber != null)
                        scrubber.UpdateScrubber(deltaTime);
                }
            }

            if (canisters != null)
            {
                foreach (var canister in canisters)
                {
                    if (canister != null)
                        canister.UpdateCanister(deltaTime);
                }
            }

            if (hazards != null)
            {
                foreach (var hazard in hazards)
                {
                    if (hazard != null)
                        hazard.UpdateHazards();
                }
            }

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node != null)
                        node.ClampToSafeValues();
                }
            }
        }

        [ContextMenu("Force Refresh Components")]
        public void ForceRefresh()
        {
            RefreshComponents();
            Debug.Log($"Force refreshed: {TotalNodes} nodes, {ActiveConnections} connections, {ActiveLeaks} leaks");
        }

        [ContextMenu("Debug All Nodes")]
        public void DebugAllNodes()
        {
            Debug.Log("=== Atmospheric Nodes ===");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node != null)
                        node.DebugStatus();
                }
            }
            Debug.Log("========================");
        }

        [ContextMenu("Debug All Connections")]
        public void DebugAllConnections()
        {
            Debug.Log("=== Gas Connections ===");
            if (connections != null)
            {
                foreach (var conn in connections)
                {
                    if (conn != null)
                        conn.DebugStatus();
                }
            }
            Debug.Log("=======================");
        }
    }
}
