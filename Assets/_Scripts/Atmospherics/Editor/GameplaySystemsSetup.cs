#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Atmospherics.Core;
using Atmospherics.Devices;

namespace Atmospherics.Editor
{
    public class GameplaySystemsSetup : EditorWindow
    {
        private AtmosphericNode targetNode;
        private AtmosphericsSimulation manager;

        [MenuItem("Atmospherics/Gameplay Systems Setup")]
        public static void ShowWindow()
        {
            GetWindow<GameplaySystemsSetup>("Gameplay Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Atmospheric Gameplay Systems Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            manager = EditorGUILayout.ObjectField("Atmospherics Manager", manager, typeof(AtmosphericsSimulation), true) as AtmosphericsSimulation;
            targetNode = EditorGUILayout.ObjectField("Target Node", targetNode, typeof(AtmosphericNode), true) as AtmosphericNode;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);

            if (GUILayout.Button("Create CO₂ Scrubber"))
            {
                CreateCO2Scrubber();
            }

            if (GUILayout.Button("Create Gas Canister"))
            {
                CreateGasCanister();
            }

            if (GUILayout.Button("Create Hazard Monitor"))
            {
                CreateHazardMonitor();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup Complete Life Support System"))
            {
                SetupLifeSupport();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Select a target node and manager before creating systems.", MessageType.Info);
        }

        private void CreateCO2Scrubber()
        {
            if (targetNode == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target node first!", "OK");
                return;
            }

            GameObject scrubberGO = new GameObject($"CO2 Scrubber ({targetNode.NodeName})");
            scrubberGO.transform.position = targetNode.transform.position + Vector3.up * 2f;

            CO2Scrubber scrubber = scrubberGO.AddComponent<CO2Scrubber>();
            scrubber.targetNode = targetNode;
            scrubber.scrubRate = 0.1f;
            scrubber.producesOxygen = true;
            scrubber.oxygenConversionRatio = 0.5f;
            scrubber.requiresPower = true;
            scrubber.isPowered = true;

            if (manager != null)
            {
                scrubberGO.transform.SetParent(manager.transform);
                EditorUtility.SetDirty(manager);
            }

            Selection.activeGameObject = scrubberGO;
            Debug.Log($"Created CO₂ Scrubber for {targetNode.NodeName}");
        }

        private void CreateGasCanister()
        {
            GameObject canisterGO = new GameObject("Gas Canister");
            canisterGO.transform.position = targetNode != null ? targetNode.transform.position + Vector3.right * 2f : Vector3.zero;

            GasCanister canister = canisterGO.AddComponent<GasCanister>();
            canister.canisterName = "O2 Canister";
            canister.maxPressure = 5000f;
            canister.volume = 0.1f;
            canister.transferRate = 1f;

            canister.storedGas = new GasMixture(1000f, 293f, 0.1f);
            canister.storedGas.Moles.Clear();
            canister.storedGas.Moles["O2"] = 20f;

            if (targetNode != null)
            {
                canister.ConnectToNode(targetNode);
                canister.mode = GasCanister.TransferMode.Manual;
            }

            if (manager != null)
            {
                canisterGO.transform.SetParent(manager.transform);
                EditorUtility.SetDirty(manager);
            }

            Selection.activeGameObject = canisterGO;
            Debug.Log("Created Gas Canister");
        }

        private void CreateHazardMonitor()
        {
            if (targetNode == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target node first!", "OK");
                return;
            }

            GameObject hazardGO = new GameObject($"Hazard Monitor ({targetNode.NodeName})");
            hazardGO.transform.position = targetNode.transform.position;

            AtmosphericHazards hazards = hazardGO.AddComponent<AtmosphericHazards>();
            hazards.monitoredNode = targetNode;

            hazards.minSafePressure = 50f;
            hazards.maxSafePressure = 150f;
            hazards.criticalLowPressure = 20f;
            hazards.criticalHighPressure = 200f;

            hazards.minSafeTemperature = 273f;
            hazards.maxSafeTemperature = 310f;
            hazards.criticalLowTemperature = 250f;
            hazards.criticalHighTemperature = 340f;

            hazards.minSafeOxygen = 18f;
            hazards.maxSafeOxygen = 25f;
            hazards.criticalLowOxygen = 10f;

            hazards.maxSafeCO2 = 1f;
            hazards.dangerousCO2 = 3f;
            hazards.criticalCO2 = 5f;

            if (manager != null)
            {
                hazardGO.transform.SetParent(manager.transform);
                EditorUtility.SetDirty(manager);
            }

            Selection.activeGameObject = hazardGO;
            Debug.Log($"Created Hazard Monitor for {targetNode.NodeName}");
        }

        private void SetupLifeSupport()
        {
            if (targetNode == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target node first!", "OK");
                return;
            }

            GameObject lifeSupportGO = new GameObject($"Life Support System ({targetNode.NodeName})");
            lifeSupportGO.transform.position = targetNode.transform.position;

            if (manager != null)
                lifeSupportGO.transform.SetParent(manager.transform);

            GameObject scrubberGO = new GameObject("CO2 Scrubber");
            scrubberGO.transform.SetParent(lifeSupportGO.transform);
            scrubberGO.transform.position = targetNode.transform.position + Vector3.up * 2f;

            CO2Scrubber scrubber = scrubberGO.AddComponent<CO2Scrubber>();
            scrubber.targetNode = targetNode;
            scrubber.scrubRate = 0.1f;
            scrubber.producesOxygen = true;
            scrubber.oxygenConversionRatio = 0.5f;
            scrubber.requiresPower = true;
            scrubber.isPowered = true;

            GameObject hazardGO = new GameObject("Hazard Monitor");
            hazardGO.transform.SetParent(lifeSupportGO.transform);
            hazardGO.transform.position = targetNode.transform.position;

            AtmosphericHazards hazards = hazardGO.AddComponent<AtmosphericHazards>();
            hazards.monitoredNode = targetNode;

            GameObject canisterGO = new GameObject("Emergency O2");
            canisterGO.transform.SetParent(lifeSupportGO.transform);
            canisterGO.transform.position = targetNode.transform.position + Vector3.right * 2f;

            GasCanister canister = canisterGO.AddComponent<GasCanister>();
            canister.canisterName = "Emergency O2";
            canister.maxPressure = 5000f;
            canister.volume = 0.1f;
            canister.storedGas = new GasMixture(2000f, 293f, 0.1f);
            canister.storedGas.Moles.Clear();
            canister.storedGas.Moles["O2"] = 50f;
            canister.connectedNode = targetNode;
            canister.isConnected = false;
            canister.mode = GasCanister.TransferMode.Manual;

            if (manager != null)
            {
                EditorUtility.SetDirty(manager);
            }

            Selection.activeGameObject = lifeSupportGO;
            Debug.Log($"Created complete Life Support System for {targetNode.NodeName}");
        }
    }
}
#endif
