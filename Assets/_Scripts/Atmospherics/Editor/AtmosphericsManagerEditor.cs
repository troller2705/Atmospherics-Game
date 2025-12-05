using UnityEditor;
using UnityEngine;
using Atmospherics.Core;

[CustomEditor(typeof(AtmosphericsSimulation))]
public class AtmosphericsManagerEditor : Editor
{
    private AtmosphericsSimulation manager;
    private float anim;

    private void OnEnable()
    {
        manager = (AtmosphericsSimulation)target;
        SceneView.duringSceneGui += DrawSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawSceneGUI;
    }

    private void DrawSceneGUI(SceneView view)
    {
        if (manager == null)
            return;

        anim += 0.01f;
        DrawNodes();
        DrawPipes();
        DrawPumps();

        view.Repaint();
    }

    private void DrawNodes()
    {
        var allLeaks = FindObjectsByType<LeakBehavior>(FindObjectsSortMode.None);
        foreach (var leak in allLeaks)
        {
            if (leak == null || leak.Nodes == null) continue;

            foreach (var node in leak.Nodes)
            {
                if (node == null) continue;

                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(node.Position, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(node.transform, "Move Node");
                    node.Position = newPos;
                    EditorUtility.SetDirty(node);
                }

                float radius = Mathf.Clamp(0.2f + node.Mixture.Volume * 0.1f, 0.2f, 1f);
                radius = radius / 2;
                Handles.color = Color.white;
                Handles.SphereHandleCap(0, node.Position, Quaternion.identity, radius, EventType.Repaint);

                // Draw gas fractions label
                string gases = "";
                var fractions = node.Mixture.GetFractions();
                foreach (var kv in fractions)
                    gases += $"{kv.Key}: {(kv.Value * 100):F1}%\n";

                UnityEditor.Handles.Label(node.Position + Vector3.up * (radius + 0.25f),
                    $"{node.NodeName}\nPressure: {node.Mixture.GetPressure():F1} kPa\nTemp: {node.Mixture.Temperature:F1}K\nVol: {node.Mixture.Volume} m³\n{gases}");
            }
        }
    }

    private void DrawPipes()
    {
        var allPipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);
        foreach (var pipe in allPipes)
        {
            if (pipe == null) return;

            Vector3 a = pipe.NodeA.Position;
            Vector3 b = pipe.NodeB.Position;

            Handles.color = Color.yellow;
            Handles.DrawLine(a, b);

            float t = Mathf.PingPong(anim, 1f);
            Vector3 dot = Vector3.Lerp(a, b, t);
            Handles.SphereHandleCap(0, dot, Quaternion.identity, 0.05f, EventType.Repaint);
        }
    }

    private void DrawPumps()
    {
        var allPumps = FindObjectsByType<Pump>(FindObjectsSortMode.None);
        foreach (var pump in allPumps)
        {
            Vector3 a = pump.SourceNode.Position;
            Vector3 b = pump.TargetNode.Position;

            Vector3 mid = (a + b) * 0.5f;
            Handles.CubeHandleCap(0, mid, Quaternion.identity, 0.2f, EventType.Repaint);

            float t = Mathf.PingPong(anim * 2, 1f);
            Vector3 dot = Vector3.Lerp(a, b, t);
            Handles.SphereHandleCap(0, dot, Quaternion.identity, 0.05f, EventType.Repaint);
        }
    }
}
