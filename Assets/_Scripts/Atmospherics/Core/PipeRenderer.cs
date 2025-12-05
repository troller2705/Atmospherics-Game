// Assets/_Scripts/Atmospherics/PipeRenderer.cs
using UnityEngine;
using Atmospherics.Core;
using System.Collections.Generic;

public class PipeRenderer : MonoBehaviour
{
    public AtmosphericsSimulation manager;
    public GameObject pipePrefab;

    Dictionary<Pipe, GameObject> visuals = new Dictionary<Pipe, GameObject>();

    void Update()
    {
        if (manager == null) return;

        var allPipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);

        foreach (var p in allPipes)
        {
            if (p == null) continue;
            if (!visuals.ContainsKey(p) && pipePrefab != null)
            {
                var g = Instantiate(pipePrefab, transform);
                g.name = p.ConnectionName;
                visuals[p] = g;
            }
        }

        var toRemove = new List<Pipe>();
        foreach (var kv in visuals)
        {
            bool found = false;
            foreach (var pipe in allPipes)
            {
                if (pipe == kv.Key)
                {
                    found = true;
                    break;
                }
            }
            if (!found) toRemove.Add(kv.Key);
        }
        foreach (var r in toRemove) { DestroyImmediate(visuals[r]); visuals.Remove(r); }

        foreach (var kv in visuals)
        {
            var p = kv.Key;
            var g = kv.Value;
            Vector3 a = p.NodeA.Position;
            Vector3 b = p.NodeB.Position;
            Vector3 mid = (a + b) * 0.5f;
            g.transform.position = mid;
            Vector3 dir = (b - a);
            float length = dir.magnitude;
            if (length <= 0.0001f) continue;
            g.transform.rotation = Quaternion.LookRotation(dir.normalized);
            Vector3 s = g.transform.localScale;
            s.y = length;
            g.transform.localScale = s;
        }
    }
}
