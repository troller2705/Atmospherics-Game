#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Atmospherics.Player;

namespace Atmospherics.Editor
{
    public class PlayerAtmosphericSetup : EditorWindow
    {
        private GameObject playerObject;
        private bool useRadiusDetection = true;
        private bool useTriggerDetection = false;
        private bool addDebugHUD = true;

        [MenuItem("Atmospherics/Player Setup")]
        public static void ShowWindow()
        {
            GetWindow<PlayerAtmosphericSetup>("Player Atmospheric Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Player Atmospheric System Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            playerObject = EditorGUILayout.ObjectField("Player GameObject", playerObject, typeof(GameObject), true) as GameObject;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Detection Method", EditorStyles.boldLabel);

            useRadiusDetection = EditorGUILayout.Toggle("Use Radius Detection", useRadiusDetection);
            useTriggerDetection = EditorGUILayout.Toggle("Use Trigger Detection", useTriggerDetection);

            EditorGUILayout.Space();
            addDebugHUD = EditorGUILayout.Toggle("Add Debug HUD", addDebugHUD);

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup Player Atmospheric Systems"))
            {
                SetupPlayer();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This will add atmospheric interaction components to your player.", MessageType.Info);

            if (playerObject != null)
            {
                EditorGUILayout.HelpBox($"Target: {playerObject.name}", MessageType.None);
            }
        }

        private void SetupPlayer()
        {
            if (playerObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a player GameObject first!", "OK");
                return;
            }

            PlayerAtmosphericNeeds needs = playerObject.GetComponent<PlayerAtmosphericNeeds>();
            if (needs == null)
            {
                needs = playerObject.AddComponent<PlayerAtmosphericNeeds>();
                Debug.Log($"Added PlayerAtmosphericNeeds to {playerObject.name}");
            }

            needs.oxygenConsumptionRate = 0.01f;
            needs.co2ProductionRate = 0.008f;
            needs.maxHealth = 100f;
            needs.currentHealth = 100f;
            needs.maxStamina = 100f;
            needs.currentStamina = 100f;

            if (useRadiusDetection)
            {
                PlayerZoneDetector detector = playerObject.GetComponent<PlayerZoneDetector>();
                if (detector == null)
                {
                    detector = playerObject.AddComponent<PlayerZoneDetector>();
                    Debug.Log($"Added PlayerZoneDetector to {playerObject.name}");
                }

                detector.atmosphericNeeds = needs;
                detector.detectionRadius = 2f;
                detector.showDebugGizmos = true;
                EditorUtility.SetDirty(detector);
            }

            if (addDebugHUD)
            {
                PlayerAtmosphericDebugHUD hud = playerObject.GetComponent<PlayerAtmosphericDebugHUD>();
                if (hud == null)
                {
                    hud = playerObject.AddComponent<PlayerAtmosphericDebugHUD>();
                    Debug.Log($"Added PlayerAtmosphericDebugHUD to {playerObject.name}");
                }

                hud.playerNeeds = needs;
                hud.showHUD = true;
                hud.showDetailedReadings = true;
                EditorUtility.SetDirty(hud);
            }

            if (!playerObject.CompareTag("Player"))
            {
                bool setTag = EditorUtility.DisplayDialog("Set Player Tag?", 
                    "The selected GameObject doesn't have the 'Player' tag. Set it now?", 
                    "Yes", "No");

                if (setTag)
                {
                    playerObject.tag = "Player";
                }
            }

            EditorUtility.SetDirty(playerObject);
            EditorUtility.SetDirty(needs);

            Debug.Log($"Player atmospheric systems setup complete on {playerObject.name}!");
            EditorUtility.DisplayDialog("Success", 
                $"Player atmospheric systems added to {playerObject.name}!\n\nComponents:\n- PlayerAtmosphericNeeds\n" +
                (useRadiusDetection ? "- PlayerZoneDetector\n" : "") +
                (addDebugHUD ? "- PlayerAtmosphericDebugHUD" : ""), 
                "OK");
        }
    }
}
#endif
