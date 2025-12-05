#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Atmospherics.Core;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(AtmosphericNode))]
public class VolumeNodeGasEditor : Editor
{
    AtmosphericNode node;

    void OnEnable()
    {
        node = (AtmosphericNode)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Node", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        node.NodeName = EditorGUILayout.TextField("Name", node.NodeName);
        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(node);

        if (node.Mixture == null)
        {
            if (GUILayout.Button("Initialize Mixture"))
            {
                node.Initialize(node.NodeName, 101.3f, 293f, 1f);
                EditorUtility.SetDirty(node);
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Presets:", EditorStyles.boldLabel);
            if (GUILayout.Button("Initialize with Earth Air"))
            {
                node.Initialize(node.NodeName, 101.3f, 293f, 50f);
                node.Mixture.Moles["O2"] = 100f;
                node.Mixture.Moles["N2"] = 380f;
                node.Mixture.Moles["CO2"] = 0.2f;
                node.Thermal.InternalEnergyJ = node.Mixture.TotalMoles() * 29.0f * node.Mixture.Temperature;
                EditorUtility.SetDirty(node);
            }
            if (GUILayout.Button("Initialize as Vacuum"))
            {
                node.Initialize(node.NodeName, 0.001f, 2.7f, 1000f);
                EditorUtility.SetDirty(node);
            }
            serializedObject.ApplyModifiedProperties();
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gas Mixture", EditorStyles.boldLabel);

        // Pressure
        EditorGUI.BeginChangeCheck();
        float oldPressure = node.Mixture.GetPressure();
        float newPressure = EditorGUILayout.FloatField("Pressure (kPa)", oldPressure);
        if (EditorGUI.EndChangeCheck() && newPressure > 0f)
        {
            float scale = newPressure / Mathf.Max(0.0001f, oldPressure);
            foreach (var key in node.Mixture.Moles.Keys.ToList())
                node.Mixture.Moles[key] *= scale;
            EditorUtility.SetDirty(node);
        }

        // Temperature
        EditorGUI.BeginChangeCheck();
        float newTemp = EditorGUILayout.FloatField("Temperature (K)", node.Mixture.Temperature);
        if (EditorGUI.EndChangeCheck())
        {
            node.Mixture.Temperature = newTemp; // optional: clamp only if negative
            if (node.Mixture.Temperature <= 0f)
                node.Mixture.Temperature = 0.1f;

            EditorUtility.SetDirty(node); // tell Unity object changed
        }

        // Volume
        EditorGUI.BeginChangeCheck();
        float newVolume = EditorGUILayout.FloatField("Volume (m³)", node.Mixture.Volume);
        if (EditorGUI.EndChangeCheck())
        {
            node.Mixture.Volume = Mathf.Max(0.0001f, newVolume); // prevent zero/negative
            EditorUtility.SetDirty(node);
        }


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gas Fractions", EditorStyles.boldLabel);

        // --- Fractions editor with renaming and auto-normalization ---
        var keys = node.Mixture.Moles.Keys.ToList();
        float totalMoles = node.Mixture.TotalMoles();
        if (totalMoles <= 0f) totalMoles = 1f;
        Dictionary<string, float> fractions = keys.ToDictionary(k => k, k => node.Mixture.Moles[k] / totalMoles);

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i];

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.TextField(key, GUILayout.MaxWidth(100));
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName) && newName != key)
            {
                if (!node.Mixture.Moles.ContainsKey(newName))
                {
                    node.Mixture.RenameGas(key, newName);
                    
                    if (fractions.ContainsKey(key))
                    {
                        fractions[newName] = fractions[key];
                        fractions.Remove(key);
                    }
                    keys[i] = newName;
                    key = newName;
                    EditorUtility.SetDirty(node);
                }
                else
                {
                    Debug.LogWarning($"Gas '{newName}' already exists!");
                }
            }

            EditorGUI.BeginChangeCheck();
            float oldFraction = fractions[key];
            float newFraction = EditorGUILayout.Slider(oldFraction, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                float delta = newFraction - oldFraction;
                float remaining = 1f - oldFraction;

                if (remaining > 0f)
                {
                    foreach (var k2 in keys)
                    {
                        if (k2 == key) continue;
                        fractions[k2] -= fractions[k2] / remaining * delta;
                        fractions[k2] = Mathf.Clamp01(fractions[k2]);
                    }
                }

                fractions[key] = Mathf.Clamp01(newFraction);

                float sum = fractions.Values.Sum();
                if (sum > 0f)
                {
                    foreach (var k2 in fractions.Keys.ToList())
                        fractions[k2] /= sum;
                }

                EditorUtility.SetDirty(node);
            }

            EditorGUILayout.EndHorizontal();
        }

        // Apply fractions back to moles
        foreach (var kv in fractions)
            node.Mixture.Moles[kv.Key] = kv.Value * totalMoles;

        if (GUILayout.Button("Normalize Fractions"))
        {
            float sum = fractions.Values.Sum();
            if (sum > 0f)
            {
                foreach (var k in fractions.Keys.ToList())
                    fractions[k] /= sum;
                foreach (var kv in fractions)
                    node.Mixture.Moles[kv.Key] = kv.Value * totalMoles;
                EditorUtility.SetDirty(node);
            }
        }

        if (GUILayout.Button("Add Gas Type"))
        {
            string newGas = "GAS" + node.Mixture.Moles.Count;
            node.Mixture.Moles[newGas] = 0f;
            EditorUtility.SetDirty(node);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Earth Air"))
        {
            node.Mixture.Moles.Clear();
            node.Mixture.Moles["O2"] = 100f;
            node.Mixture.Moles["N2"] = 380f;
            node.Mixture.Moles["CO2"] = 0.2f;
            node.Mixture.Temperature = 293f;
            node.Thermal.InternalEnergyJ = node.Mixture.TotalMoles() * 29.0f * node.Mixture.Temperature;
            EditorUtility.SetDirty(node);
        }
        if (GUILayout.Button("Low O₂"))
        {
            node.Mixture.Moles.Clear();
            node.Mixture.Moles["O2"] = 40f;
            node.Mixture.Moles["N2"] = 380f;
            node.Mixture.Moles["CO2"] = 8f;
            node.Mixture.Temperature = 293f;
            node.Thermal.InternalEnergyJ = node.Mixture.TotalMoles() * 29.0f * node.Mixture.Temperature;
            EditorUtility.SetDirty(node);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Vacuum"))
        {
            node.Mixture.Moles.Clear();
            node.Mixture.Moles["O2"] = 0.001f;
            node.Mixture.Temperature = 2.7f;
            node.Thermal.InternalEnergyJ = node.Mixture.TotalMoles() * 29.0f * node.Mixture.Temperature;
            EditorUtility.SetDirty(node);
        }
        if (GUILayout.Button("CO₂ Rich"))
        {
            node.Mixture.Moles.Clear();
            node.Mixture.Moles["O2"] = 10f;
            node.Mixture.Moles["N2"] = 200f;
            node.Mixture.Moles["CO2"] = 50f;
            node.Mixture.Temperature = 293f;
            node.Thermal.InternalEnergyJ = node.Mixture.TotalMoles() * 29.0f * node.Mixture.Temperature;
            EditorUtility.SetDirty(node);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Open Quick Scene Editor"))
            GasEditorWindow.Open(node);

        serializedObject.ApplyModifiedProperties();
    }

    // Quick popup editor (same live normalization + renaming)
    public class GasEditorWindow : EditorWindow
    {
        AtmosphericNode targetNode;

        public static void Open(AtmosphericNode n)
        {
            var w = CreateInstance<GasEditorWindow>();
            w.titleContent = new GUIContent("Gas Editor");
            w.targetNode = n;
            w.ShowUtility();
        }

        void OnGUI()
        {
            if (targetNode == null) { Close(); return; }

            GUILayout.Label(targetNode.NodeName, EditorStyles.boldLabel);

            // Pressure
            EditorGUI.BeginChangeCheck();
            float oldPressure = targetNode.Mixture.GetPressure();
            float newPressure = EditorGUILayout.FloatField("Pressure (kPa)", oldPressure);
            if (EditorGUI.EndChangeCheck() && newPressure > 0f)
            {
                float scale = newPressure / Mathf.Max(0.0001f, oldPressure);
                foreach (var key in targetNode.Mixture.Moles.Keys.ToList())
                    targetNode.Mixture.Moles[key] *= scale;
                EditorUtility.SetDirty(targetNode);
            }

            // Temperature
            EditorGUI.BeginChangeCheck();
            float newTemp = EditorGUILayout.FloatField("Temperature (K)", targetNode.Mixture.Temperature);
            if (EditorGUI.EndChangeCheck())
            {
                targetNode.Mixture.Temperature = Mathf.Max(0.1f, newTemp);
                EditorUtility.SetDirty(targetNode);
            }

            GUILayout.Space(6);
            GUILayout.Label("Gas Fractions", EditorStyles.boldLabel);

            var keys = targetNode.Mixture.Moles.Keys.ToList();
            float totalMoles = targetNode.Mixture.TotalMoles();
            if (totalMoles <= 0f) totalMoles = 1f;
            Dictionary<string, float> fractions = keys.ToDictionary(k => k, k => targetNode.Mixture.Moles[k] / totalMoles);

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField(key, GUILayout.MaxWidth(100));
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName) && newName != key)
                {
                    if (!targetNode.Mixture.Moles.ContainsKey(newName))
                    {
                        targetNode.Mixture.RenameGas(key, newName);
                        
                        if (fractions.ContainsKey(key))
                        {
                            fractions[newName] = fractions[key];
                            fractions.Remove(key);
                        }
                        keys[i] = newName;
                        key = newName;
                        EditorUtility.SetDirty(targetNode);
                    }
                    else
                    {
                        Debug.LogWarning($"Gas '{newName}' already exists!");
                    }
                }

                EditorGUI.BeginChangeCheck();
                float oldFraction = fractions[key];
                float newFraction = EditorGUILayout.Slider(oldFraction, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float delta = newFraction - oldFraction;
                    float remaining = 1f - oldFraction;
                    if (remaining > 0f)
                    {
                        foreach (var k2 in keys)
                            if (k2 != key) fractions[k2] = Mathf.Max(0f, fractions[k2] - (fractions[k2] / remaining) * delta);
                    }
                    fractions[key] = newFraction;
                    EditorUtility.SetDirty(targetNode);
                }

                EditorGUILayout.EndHorizontal();
            }

            foreach (var kv in fractions)
                targetNode.Mixture.Moles[kv.Key] = kv.Value * totalMoles;

            if (GUILayout.Button("Normalize"))
            {
                float sum = fractions.Values.Sum();
                if (sum > 0f)
                {
                    foreach (var k in fractions.Keys.ToList())
                        fractions[k] /= sum;
                    foreach (var kv in fractions)
                        targetNode.Mixture.Moles[kv.Key] = kv.Value * totalMoles;
                    EditorUtility.SetDirty(targetNode);
                }
            }

            if (GUILayout.Button("Add Gas Type"))
            {
                string newGas = "GAS" + targetNode.Mixture.Moles.Count;
                targetNode.Mixture.Moles[newGas] = 0f;
                EditorUtility.SetDirty(targetNode);
            }

            if (GUILayout.Button("Close")) Close();
        }
    }
}
#endif
