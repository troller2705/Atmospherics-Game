#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Atmospherics.Core;

[CustomEditor(typeof(AtmosphericsSimulation))]
public class AtmosphericsSetupHelper : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AtmosphericsSimulation manager = (AtmosphericsSimulation)target;
        AtmosphericsValidator validator = manager.GetComponent<AtmosphericsValidator>();
        AtmosphericsTestScenarios tester = manager.GetComponent<AtmosphericsTestScenarios>();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Setup Tools", EditorStyles.boldLabel);

        if (!manager.GetComponent<HeatTransfer>())
        {
            if (GUILayout.Button("Add Heat Transfer System"))
            {
                manager.gameObject.AddComponent<HeatTransfer>();
                EditorUtility.SetDirty(manager);
            }
        }

        if (!manager.GetComponent<PipeRenderer>())
        {
            if (GUILayout.Button("Add Pipe Renderer"))
            {
                manager.gameObject.AddComponent<PipeRenderer>();
                EditorUtility.SetDirty(manager);
            }
        }

        if (validator == null)
        {
            if (GUILayout.Button("Add Validation System"))
            {
                validator = manager.gameObject.AddComponent<AtmosphericsValidator>();
                validator.manager = manager;
                EditorUtility.SetDirty(manager);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✓ Validation System installed", MessageType.Info);
        }

        if (tester == null)
        {
            if (GUILayout.Button("Add Test Scenarios"))
            {
                tester = manager.gameObject.AddComponent<AtmosphericsTestScenarios>();
                tester.manager = manager;
                tester.validator = manager.GetComponent<AtmosphericsValidator>();
                EditorUtility.SetDirty(manager);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✓ Test Scenarios installed", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Validation Tools", EditorStyles.boldLabel);

        if (validator != null)
        {
            if (GUILayout.Button("Print Validation Report"))
            {
                validator.PrintReport();
            }

            if (GUILayout.Button("Reset Energy Baseline"))
            {
                validator.ResetBaseline();
            }
        }

        if (tester != null)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("▶ Run All Tests (Play Mode)"))
            {
                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Enter Play mode to run tests", MessageType.Warning);
                }
                else
                {
                    tester.RunAllTestsNow();
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Stats", EditorStyles.boldLabel);

        var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
        var allPipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);
        var allPumps = FindObjectsByType<Pump>(FindObjectsSortMode.None);

        EditorGUILayout.LabelField($"Leak Behaviors: {allLeaks.Length}");
        EditorGUILayout.LabelField($"Pipes: {allPipes.Length}");
        EditorGUILayout.LabelField($"Pumps: {allPumps.Length}");

        int totalNodes = 0;
        foreach (var leak in allLeaks)
        {
            if (leak != null && leak.Nodes != null)
                totalNodes += leak.Nodes.Count;
        }
        EditorGUILayout.LabelField($"Total Nodes: {totalNodes}");

        if (Application.isPlaying && validator != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Stats", EditorStyles.boldLabel);

            float totalEnergy = 0f;
            float totalMoles = 0f;

            foreach (var leak in allLeaks)
            {
                if (leak == null || leak.Nodes == null) continue;
                foreach (var node in leak.Nodes)
                {
                    if (node == null) continue;
                    totalEnergy += node.Thermal.InternalEnergyJ;
                    if (node.Mixture != null)
                        totalMoles += node.Mixture.TotalMoles();
                }
            }

            EditorGUILayout.LabelField($"Total Energy: {totalEnergy:F1} J");
            EditorGUILayout.LabelField($"Total Moles: {totalMoles:F2} mol");

            if (totalMoles > 0)
            {
                float avgTemp = totalEnergy / (totalMoles * 29.0f);
                EditorGUILayout.LabelField($"Avg Temperature: {avgTemp:F1} K");
            }

            Repaint();
        }
    }
}
#endif
