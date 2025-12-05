#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Atmospherics.Core;

[CustomEditor(typeof(Pipe))]
public class PipeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Pipe pipe = (Pipe)target;
        pipe.Conductance = EditorGUILayout.Slider("Conductance", pipe.Conductance, 0f, 5f);
        if (GUI.changed) EditorUtility.SetDirty(pipe);
    }
}
#endif
