// Assets/_Scripts/Atmospherics/Editor/PumpInspector.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Atmospherics.Core;

[CustomEditor(typeof(Pump))]
public class PumpInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Pump pump = (Pump)target;
        pump.FlowRate = EditorGUILayout.Slider("Max Flow Rate (frac/s)", pump.FlowRate, 0f, 5f);
        //pump.IsActive = EditorGUILayout.Toggle("Active", pump.IsActive);
        if (GUI.changed) EditorUtility.SetDirty(pump);
    }
}
#endif
