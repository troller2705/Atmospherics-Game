using UnityEngine;
using System.Collections.Generic;
using Atmospherics.Core;

namespace Atmospherics.Voxel
{
    public class VoxelAtmosphericBridge : MonoBehaviour
    {
        [Header("Voxel Grid Settings")]
        public Vector3Int gridSize = new Vector3Int(10, 10, 10);
        public float voxelSize = 1f;
        public Vector3 gridOrigin = Vector3.zero;

        [Header("Atmospheric Settings")]
        public float volumePerVoxel = 1f;
        public bool autoCreateNodes = true;
        public bool mergeAdjacentVoxels = true;
        public int minVoxelsForNode = 8;

        [Header("Update Settings")]
        public bool dynamicUpdates = true;
        public float updateInterval = 1f;

        [Header("Debug")]
        public bool showDebugGizmos = true;
        public bool logNodeCreation = false;

        private Dictionary<Vector3Int, VoxelData> voxelGrid = new Dictionary<Vector3Int, VoxelData>();
        private Dictionary<Vector3Int, AtmosphericNode> voxelToNode = new Dictionary<Vector3Int, AtmosphericNode>();
        private Dictionary<AtmosphericNode, List<Vector3Int>> nodeToVoxels = new Dictionary<AtmosphericNode, List<Vector3Int>>();
        private float updateTimer = 0f;

        public Dictionary<Vector3Int, VoxelData> VoxelGrid => voxelGrid;
        public Dictionary<AtmosphericNode, List<Vector3Int>> NodeToVoxels => nodeToVoxels;

        public enum VoxelType
        {
            Empty = 0,
            Solid = 1,
            Partial = 2
        }

        [System.Serializable]
        public class VoxelData
        {
            public VoxelType type;
            public float density;
            public bool isSealed;

            public VoxelData(VoxelType voxelType, float voxelDensity = 1f, bool sealedBool = false)
            {
                type = voxelType;
                density = Mathf.Clamp01(voxelDensity);
                isSealed = sealedBool;
            }

            public bool IsPassable()
            {
                return type == VoxelType.Empty || (type == VoxelType.Partial && density < 0.5f);
            }
        }

        private void Start()
        {
            if (autoCreateNodes)
            {
                RebuildAtmosphericZones();
            }
        }

        private void Update()
        {
            if (!dynamicUpdates) return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateAtmosphericConnections();
                updateTimer = 0f;
            }
        }

        public void SetVoxel(Vector3Int position, VoxelType type, float density = 1f, bool sealedBool = false)
        {
            if (!IsValidPosition(position)) return;

            voxelGrid[position] = new VoxelData(type, density, sealedBool);

            if (dynamicUpdates)
            {
                UpdateVoxelAtmosphere(position);
            }
        }

        public void SetVoxel(int x, int y, int z, VoxelType type, float density = 1f, bool sealedBool = false)
        {
            SetVoxel(new Vector3Int(x, y, z), type, density, sealedBool);
        }

        public VoxelData GetVoxel(Vector3Int position)
        {
            if (voxelGrid.ContainsKey(position))
                return voxelGrid[position];
            
            return new VoxelData(VoxelType.Empty);
        }

        public VoxelData GetVoxel(int x, int y, int z)
        {
            return GetVoxel(new Vector3Int(x, y, z));
        }

        public void ClearVoxel(Vector3Int position)
        {
            SetVoxel(position, VoxelType.Empty, 0f, false);
        }

        public void RebuildAtmosphericZones()
        {
            ClearExistingNodes();

            List<List<Vector3Int>> zones = FindConnectedZones();

            foreach (var zone in zones)
            {
                if (zone.Count >= minVoxelsForNode)
                {
                    CreateNodeForZone(zone);
                }
            }

            CreatePipesForAdjacentZones();

            if (logNodeCreation)
            {
                Debug.Log($"Created {nodeToVoxels.Count} atmospheric nodes from {zones.Count} voxel zones");
            }
        }

        private List<List<Vector3Int>> FindConnectedZones()
        {
            List<List<Vector3Int>> zones = new List<List<Vector3Int>>();
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

            foreach (var kvp in voxelGrid)
            {
                if (visited.Contains(kvp.Key)) continue;
                if (!kvp.Value.IsPassable()) continue;

                List<Vector3Int> zone = new List<Vector3Int>();
                FloodFill(kvp.Key, visited, zone);

                if (zone.Count > 0)
                {
                    zones.Add(zone);
                }
            }

            return zones;
        }

        private void FloodFill(Vector3Int start, HashSet<Vector3Int> visited, List<Vector3Int> zone)
        {
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();
                zone.Add(current);

                foreach (Vector3Int neighbor in GetNeighbors(current))
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!IsValidPosition(neighbor)) continue;

                    VoxelData voxelData = GetVoxel(neighbor);
                    if (!voxelData.IsPassable()) continue;

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        private List<Vector3Int> GetNeighbors(Vector3Int pos)
        {
            return new List<Vector3Int>
            {
                pos + Vector3Int.right,
                pos + Vector3Int.left,
                pos + Vector3Int.up,
                pos + Vector3Int.down,
                pos + Vector3Int.forward,
                pos + Vector3Int.back
            };
        }

        private void CreateNodeForZone(List<Vector3Int> zone)
        {
            Vector3 centerPos = CalculateZoneCenter(zone);
            float volume = zone.Count * volumePerVoxel;

            GameObject nodeObj = new GameObject($"Voxel Zone [{zone.Count} voxels]");
            nodeObj.transform.SetParent(transform);
            nodeObj.transform.position = centerPos;

            AtmosphericNode node = nodeObj.AddComponent<AtmosphericNode>();
            node.Initialize($"VoxelZone_{nodeToVoxels.Count}", 101.3f, 293f, volume);

            node.Mixture.Moles["O2"] = volume * 0.21f;
            node.Mixture.Moles["N2"] = volume * 0.78f;
            node.Mixture.Moles["CO2"] = volume * 0.0004f;

            node.Thermal.InternalEnergyJ = node.Mixture.TotalMoles() * 29.0f * node.Mixture.Temperature;

            var visualizer = nodeObj.AddComponent<Atmospherics.Visualization.AtmosphericZoneVisualizer>();
            visualizer.showZoneBoundary = true;
            visualizer.sphereRadius = Mathf.Pow(volume, 0.33f) * 0.5f;

            nodeToVoxels[node] = zone;
            foreach (Vector3Int voxelPos in zone)
            {
                voxelToNode[voxelPos] = node;
            }
        }

        private Vector3 CalculateZoneCenter(List<Vector3Int> zone)
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3Int pos in zone)
            {
                sum += VoxelToWorldPosition(pos);
            }
            return sum / zone.Count;
        }

        private void CreatePipesForAdjacentZones()
        {
            HashSet<(AtmosphericNode, AtmosphericNode)> processed = new HashSet<(AtmosphericNode, AtmosphericNode)>();

            foreach (var kvp in nodeToVoxels)
            {
                AtmosphericNode nodeA = kvp.Key;
                List<Vector3Int> voxelsA = kvp.Value;

                foreach (Vector3Int voxel in voxelsA)
                {
                    foreach (Vector3Int neighbor in GetNeighbors(voxel))
                    {
                        if (!voxelToNode.ContainsKey(neighbor)) continue;

                        AtmosphericNode nodeB = voxelToNode[neighbor];
                        if (nodeA == nodeB) continue;

                        var pair = nodeA.GetInstanceID() < nodeB.GetInstanceID() 
                            ? (nodeA, nodeB) 
                            : (nodeB, nodeA);

                        if (processed.Contains(pair)) continue;

                        CreatePipeBetweenNodes(nodeA, nodeB);
                        processed.Add(pair);
                    }
                }
            }
        }

        private void CreatePipeBetweenNodes(AtmosphericNode nodeA, AtmosphericNode nodeB)
        {
            GameObject pipeObj = new GameObject($"Pipe {nodeA.NodeName} <-> {nodeB.NodeName}");
            pipeObj.transform.SetParent(transform);
            pipeObj.transform.position = (nodeA.transform.position + nodeB.transform.position) / 2f;

            Pipe pipe = pipeObj.AddComponent<Pipe>();
            pipe.Initialize($"VoxelPipe_{nodeA.NodeName}_{nodeB.NodeName}", nodeA, nodeB, 0.1f);

            var pipeVis = pipeObj.AddComponent<Atmospherics.Visualization.PipeVisualizer>();
            pipeVis.showPipe = true;
            pipeVis.showFlowDirection = true;
            pipeVis.pipeThickness = 0.15f;
        }

        private void UpdateVoxelAtmosphere(Vector3Int position)
        {
            if (!voxelToNode.ContainsKey(position)) return;

            AtmosphericNode oldNode = voxelToNode[position];
            VoxelData voxelData = GetVoxel(position);

            if (!voxelData.IsPassable())
            {
                RemoveVoxelFromNode(position, oldNode);
            }
            else
            {
                RebuildAtmosphericZones();
            }
        }

        private void RemoveVoxelFromNode(Vector3Int position, AtmosphericNode node)
        {
            if (!nodeToVoxels.ContainsKey(node)) return;

            nodeToVoxels[node].Remove(position);
            voxelToNode.Remove(position);

            if (nodeToVoxels[node].Count < minVoxelsForNode)
            {
                DestroyNode(node);
            }
            else
            {
                UpdateNodeVolume(node);
            }
        }

        private void UpdateNodeVolume(AtmosphericNode node)
        {
            if (!nodeToVoxels.ContainsKey(node)) return;

            float newVolume = nodeToVoxels[node].Count * volumePerVoxel;
            node.Mixture.Volume = newVolume;
        }

        private void DestroyNode(AtmosphericNode node)
        {
            if (nodeToVoxels.ContainsKey(node))
            {
                foreach (Vector3Int voxel in nodeToVoxels[node])
                {
                    voxelToNode.Remove(voxel);
                }
                nodeToVoxels.Remove(node);
            }

            if (node != null && node.gameObject != null)
            {
                Destroy(node.gameObject);
            }
        }

        private void ClearExistingNodes()
        {
            foreach (var node in nodeToVoxels.Keys)
            {
                if (node != null && node.gameObject != null)
                {
                    Destroy(node.gameObject);
                }
            }

            voxelToNode.Clear();
            nodeToVoxels.Clear();
        }

        private void UpdateAtmosphericConnections()
        {
        }

        public Vector3 VoxelToWorldPosition(Vector3Int voxelPos)
        {
            return gridOrigin + new Vector3(
                voxelPos.x * voxelSize,
                voxelPos.y * voxelSize,
                voxelPos.z * voxelSize
            );
        }

        public Vector3Int WorldToVoxelPosition(Vector3 worldPos)
        {
            Vector3 relative = worldPos - gridOrigin;
            return new Vector3Int(
                Mathf.FloorToInt(relative.x / voxelSize),
                Mathf.FloorToInt(relative.y / voxelSize),
                Mathf.FloorToInt(relative.z / voxelSize)
            );
        }

        public AtmosphericNode GetNodeAtPosition(Vector3 worldPos)
        {
            Vector3Int voxelPos = WorldToVoxelPosition(worldPos);
            if (voxelToNode.ContainsKey(voxelPos))
                return voxelToNode[voxelPos];
            return null;
        }

        public AtmosphericNode GetNodeAtVoxel(Vector3Int voxelPos)
        {
            if (voxelToNode.ContainsKey(voxelPos))
                return voxelToNode[voxelPos];
            return null;
        }

        public void RegisterVoxelToNode(Vector3Int voxelPos, AtmosphericNode node)
        {
            voxelToNode[voxelPos] = node;
            
            if (!nodeToVoxels.ContainsKey(node))
            {
                nodeToVoxels[node] = new List<Vector3Int>();
            }
            
            if (!nodeToVoxels[node].Contains(voxelPos))
            {
                nodeToVoxels[node].Add(voxelPos);
            }
        }

        public bool IsValidPosition(Vector3Int pos)
        {
            return pos.x >= 0 && pos.x < gridSize.x &&
                   pos.y >= 0 && pos.y < gridSize.y &&
                   pos.z >= 0 && pos.z < gridSize.z;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            foreach (var kvp in voxelGrid)
            {
                Vector3 worldPos = VoxelToWorldPosition(kvp.Key);
                
                switch (kvp.Value.type)
                {
                    case VoxelType.Empty:
                        Gizmos.color = new Color(0, 1, 0, 0.1f);
                        break;
                    case VoxelType.Solid:
                        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                        break;
                    case VoxelType.Partial:
                        Gizmos.color = new Color(1, 1, 0, 0.3f * kvp.Value.density);
                        break;
                }

                Gizmos.DrawCube(worldPos + Vector3.one * voxelSize * 0.5f, Vector3.one * voxelSize * 0.9f);
            }

            Gizmos.color = Color.cyan;
            foreach (var kvp in nodeToVoxels)
            {
                if (kvp.Key != null)
                {
                    Gizmos.DrawWireSphere(kvp.Key.transform.position, 0.3f);
                }
            }
        }
    }
}
