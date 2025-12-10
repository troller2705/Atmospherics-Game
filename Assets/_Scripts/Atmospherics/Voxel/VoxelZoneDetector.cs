using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Atmospherics.Core;

namespace Atmospherics.Voxel
{
    [RequireComponent(typeof(VoxelAtmosphericBridge))]
    public class VoxelZoneDetector : MonoBehaviour
    {
        [Header("Zone Detection Settings")]
        [Tooltip("Maximum number of voxels to check for enclosure (prevents infinite loops)")]
        public int maxFloodFillDepth = 10000;

        [Tooltip("If true, edges of the voxel grid are considered sealed (enclosed spaces must be fully walled)")]
        public bool gridEdgesAreSealed = false;

        [Header("Global Atmosphere")]
        public GlobalAtmosphere globalAtmosphere;

        [Header("Zone Visualization")]
        public bool showZoneGizmos = true;
        public Color enclosedZoneColor = Color.green;
        public Color globalZoneColor = Color.red;

        [Header("Debug")]
        public bool logZoneDetection = true;

        private VoxelAtmosphericBridge bridge;
        private Dictionary<int, ZoneInfo> detectedZones = new Dictionary<int, ZoneInfo>();
        private int nextZoneId = 0;

        public System.Action OnZonesChanged;

        public class ZoneInfo
        {
            public int zoneId;
            public HashSet<Vector3Int> voxels = new HashSet<Vector3Int>();
            public bool isEnclosed;
            public AtmosphericNode node;
            public LeakBehavior leakBehavior;
            public Vector3 centerPosition;

            public float CalculateVolume(float voxelSize)
            {
                return voxels.Count * (voxelSize * voxelSize * voxelSize);
            }
        }

        private void Awake()
        {
            bridge = GetComponent<VoxelAtmosphericBridge>();

            if (globalAtmosphere == null)
            {
                globalAtmosphere = FindFirstObjectByType<GlobalAtmosphere>();
            }
        }

        private void Start()
        {
            if (globalAtmosphere == null)
            {
                Debug.LogWarning("VoxelZoneDetector: No GlobalAtmosphere found! Creating one with Vacuum preset.");
                GameObject globalAtmosGO = new GameObject("Global Atmosphere");
                globalAtmosphere = globalAtmosGO.AddComponent<GlobalAtmosphere>();
                globalAtmosphere.preset = GlobalAtmosphere.AtmospherePreset.Vacuum;
            }
        }

        public void DetectAndCreateZones()
        {
            ClearExistingZones();
            detectedZones.Clear();
            nextZoneId = 0;

            HashSet<Vector3Int> visitedVoxels = new HashSet<Vector3Int>();

            foreach (var kvp in bridge.VoxelGrid)
            {
                Vector3Int voxelPos = kvp.Key;
                VoxelAtmosphericBridge.VoxelData voxelData = kvp.Value;

                if (visitedVoxels.Contains(voxelPos)) continue;
                if (!voxelData.IsPassable()) continue;

                ZoneInfo zone = FloodFillZone(voxelPos, visitedVoxels);

                if (zone != null && zone.voxels.Count > 0)
                {
                    zone.zoneId = nextZoneId++;
                    detectedZones[zone.zoneId] = zone;
                    CreateAtmosphericNode(zone);
                }
            }

            if (logZoneDetection)
            {
                int enclosedCount = detectedZones.Values.Count(z => z.isEnclosed);
                int globalCount = detectedZones.Values.Count(z => !z.isEnclosed);
                Debug.Log($"VoxelZoneDetector: Detected {detectedZones.Count} zones ({enclosedCount} enclosed, {globalCount} global)");
            }

            OnZonesChanged?.Invoke();
        }

        private ZoneInfo FloodFillZone(Vector3Int startPos, HashSet<Vector3Int> globalVisited)
        {
            ZoneInfo zone = new ZoneInfo();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            HashSet<Vector3Int> zoneVisited = new HashSet<Vector3Int>();

            queue.Enqueue(startPos);
            zoneVisited.Add(startPos);
            bool touchesEdge = false;
            int iterations = 0;

            while (queue.Count > 0 && iterations < maxFloodFillDepth)
            {
                iterations++;
                Vector3Int current = queue.Dequeue();
                zone.voxels.Add(current);
                globalVisited.Add(current);

                if (IsAtGridEdge(current) && !gridEdgesAreSealed)
                {
                    touchesEdge = true;
                    if (logZoneDetection)
                    {
                        Debug.Log($"Zone touches edge at voxel {current}");
                    }
                }

                foreach (Vector3Int neighbor in GetNeighbors(current))
                {
                    if (zoneVisited.Contains(neighbor)) continue;

                    if (!bridge.IsValidPosition(neighbor))
                    {
                        if (!gridEdgesAreSealed)
                        {
                            touchesEdge = true;
                            if (logZoneDetection)
                            {
                                Debug.Log($"Zone reaches grid boundary from {current} trying to reach {neighbor}");
                            }
                        }
                        continue;
                    }

                    var neighborData = bridge.GetVoxel(neighbor);
                    if (neighborData.IsPassable())
                    {
                        queue.Enqueue(neighbor);
                        zoneVisited.Add(neighbor);
                    }
                }
            }

            zone.isEnclosed = !touchesEdge;
            zone.centerPosition = CalculateZoneCenter(zone.voxels);

            if (logZoneDetection)
            {
                string edgeStatus = touchesEdge ? "touches edge (GLOBAL)" : "enclosed (SEALED)";
                Debug.Log($"FloodFill complete: {zone.voxels.Count} voxels, {edgeStatus}");
            }

            return zone;
        }

        private Vector3 CalculateZoneCenter(HashSet<Vector3Int> voxels)
        {
            if (voxels.Count == 0) return Vector3.zero;

            Vector3 sum = Vector3.zero;
            foreach (var voxel in voxels)
            {
                sum += bridge.VoxelToWorldPosition(voxel);
            }
            return sum / voxels.Count;
        }

        private void CreateAtmosphericNode(ZoneInfo zone)
        {
            GameObject nodeGO = new GameObject($"Zone_{zone.zoneId} [{zone.voxels.Count} voxels]");
            nodeGO.transform.SetParent(transform);
            nodeGO.transform.position = zone.centerPosition;

            zone.node = nodeGO.AddComponent<AtmosphericNode>();
            zone.node.NodeName = zone.isEnclosed ? $"Enclosed Zone {zone.zoneId}" : $"Global Zone {zone.zoneId}";

            float volume = zone.CalculateVolume(bridge.voxelSize);
            zone.node.Mixture.Volume = volume;

            if (zone.isEnclosed)
            {
                zone.leakBehavior = nodeGO.AddComponent<LeakBehavior>();
                zone.leakBehavior.IsSealed = true;
                zone.leakBehavior.LeakRateFractionPerSec = 0f;
                zone.leakBehavior.Nodes = new List<AtmosphericNode> { zone.node };

                InitializeEnclosedAtmosphere(zone);
            }
            else
            {
                zone.leakBehavior = nodeGO.AddComponent<LeakBehavior>();
                zone.leakBehavior.IsSealed = false;
                zone.leakBehavior.LeakRateFractionPerSec = 1f;
                zone.leakBehavior.Nodes = new List<AtmosphericNode> { zone.node };

                if (globalAtmosphere != null)
                {
                    globalAtmosphere.ApplyToNode(zone.node, volume);
                }
            }

            foreach (var voxel in zone.voxels)
            {
                bridge.RegisterVoxelToNode(voxel, zone.node);
            }

            if (logZoneDetection)
            {
                string zoneType = zone.isEnclosed ? "ENCLOSED" : "GLOBAL";
                Debug.Log($"Created {zoneType} zone {zone.zoneId}: {zone.voxels.Count} voxels, {volume:F1}m³");
            }
        }

        private void InitializeEnclosedAtmosphere(ZoneInfo zone)
        {
            zone.node.Mixture.Moles["N2"] = 40f;
            zone.node.Mixture.Moles["O2"] = 10f;
            zone.node.Mixture.Temperature = 293.15f;

            float totalMoles = zone.node.Mixture.TotalMoles();
            zone.node.Thermal.InternalEnergyJ = totalMoles * 29.0f * 293.15f;
        }

        public void ConvertZoneToGlobal(int zoneId)
        {
            if (!detectedZones.ContainsKey(zoneId)) return;

            ZoneInfo zone = detectedZones[zoneId];
            if (!zone.isEnclosed) return;

            zone.isEnclosed = false;

            if (zone.leakBehavior != null)
            {
                zone.leakBehavior.IsSealed = false;
                zone.leakBehavior.LeakRateFractionPerSec = 1f;
            }

            if (globalAtmosphere != null)
            {
                float volume = zone.CalculateVolume(bridge.voxelSize);
                globalAtmosphere.ApplyToNode(zone.node, volume);
            }

            if (zone.node != null)
            {
                zone.node.NodeName = $"Global Zone {zone.zoneId} (BREACHED)";
                zone.node.gameObject.name = $"Voxel Zone {zone.zoneId} [BREACHED - {zone.voxels.Count} voxels]";
            }

            if (logZoneDetection)
            {
                Debug.Log($"Zone {zoneId} BREACHED! Converted to global atmosphere.");
            }
        }

        public void ConvertZoneToEnclosed(int zoneId)
        {
            if (!detectedZones.ContainsKey(zoneId)) return;

            ZoneInfo zone = detectedZones[zoneId];
            if (zone.isEnclosed) return;

            zone.isEnclosed = true;

            if (zone.leakBehavior != null)
            {
                zone.leakBehavior.IsSealed = true;
                zone.leakBehavior.LeakRateFractionPerSec = 0f;
            }

            if (zone.node != null)
            {
                zone.node.NodeName = $"Enclosed Zone {zone.zoneId} (RESEALED)";
                zone.node.gameObject.name = $"Voxel Zone {zone.zoneId} [SEALED - {zone.voxels.Count} voxels]";
                InitializeEnclosedAtmosphere(zone);
            }

            if (logZoneDetection)
            {
                Debug.Log($"Zone {zoneId} RESEALED! Converted to enclosed atmosphere.");
            }
        }

        public ZoneInfo GetZoneAtVoxel(Vector3Int voxelPos)
        {
            foreach (var zone in detectedZones.Values)
            {
                if (zone.voxels.Contains(voxelPos))
                    return zone;
            }
            return null;
        }

        public void CheckForBreach(Vector3Int destroyedVoxelPos)
        {
            if (logZoneDetection)
            {
                Debug.Log($"CheckForBreach: Checking breach at {destroyedVoxelPos}, {detectedZones.Count} zones tracked");
            }

            foreach (var zone in detectedZones.Values.ToList())
            {
                if (zone.voxels.Contains(destroyedVoxelPos))
                {
                    zone.voxels.Remove(destroyedVoxelPos);
                    if (logZoneDetection)
                    {
                        Debug.Log($"CheckForBreach: Removed destroyed voxel from zone {zone.zoneId}");
                    }
                }
            }

            foreach (var zone in detectedZones.Values.Where(z => z.isEnclosed).ToList())
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForBreach: Checking zone {zone.zoneId} with {zone.voxels.Count} voxels");
                }

                if (IsVoxelAdjacentToZone(destroyedVoxelPos, zone))
                {
                    if (logZoneDetection)
                    {
                        Debug.Log($"CheckForBreach: Destroyed voxel {destroyedVoxelPos} is adjacent to zone {zone.zoneId}");
                    }

                    if (!IsZoneStillEnclosed(zone))
                    {
                        if (logZoneDetection)
                        {
                            Debug.Log($"CheckForBreach: Zone {zone.zoneId} is NO LONGER ENCLOSED - converting to global!");
                        }
                        ConvertZoneToGlobal(zone.zoneId);
                    }
                    else
                    {
                        if (logZoneDetection)
                        {
                            Debug.Log($"CheckForBreach: Zone {zone.zoneId} is still enclosed");
                        }
                    }
                }
                else
                {
                    if (logZoneDetection)
                    {
                        Debug.Log($"CheckForBreach: Destroyed voxel {destroyedVoxelPos} is NOT adjacent to zone {zone.zoneId}");
                    }
                }
            }

            CheckForZoneMerge(destroyedVoxelPos);
        }

        public void CheckForZoneMerge(Vector3Int destroyedVoxelPos)
        {
            if (logZoneDetection)
            {
                Debug.Log($"CheckForZoneMerge: Checking if voxel {destroyedVoxelPos} merges zones");
            }

            List<ZoneInfo> adjacentZones = new List<ZoneInfo>();
            foreach (var neighbor in GetNeighbors(destroyedVoxelPos))
            {
                if (!bridge.IsValidPosition(neighbor)) continue;
                var neighborData = bridge.GetVoxel(neighbor);
                if (!neighborData.IsPassable()) continue;

                foreach (var zone in detectedZones.Values)
                {
                    if (zone.voxels.Contains(neighbor) && !adjacentZones.Contains(zone))
                    {
                        adjacentZones.Add(zone);
                        break;
                    }
                }
            }

            if (adjacentZones.Count >= 2)
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForZoneMerge: Found {adjacentZones.Count} adjacent zones - they will MERGE! Re-detecting zones...");
                }
                DetectAndCreateZones();
            }
            else
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForZoneMerge: Only {adjacentZones.Count} adjacent zones, no merge");
                }
            }
        }

        public void CheckForSealing(Vector3Int createdVoxelPos)
        {
            if (logZoneDetection)
            {
                Debug.Log($"CheckForSealing: Checking seal at {createdVoxelPos}, {detectedZones.Count} zones tracked");
            }

            foreach (var zone in detectedZones.Values.Where(z => !z.isEnclosed).ToList())
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForSealing: Checking global zone {zone.zoneId} with {zone.voxels.Count} voxels");
                }

                if (IsVoxelAdjacentToZone(createdVoxelPos, zone))
                {
                    if (logZoneDetection)
                    {
                        Debug.Log($"CheckForSealing: Created voxel {createdVoxelPos} is adjacent to zone {zone.zoneId}");
                    }

                    if (IsZoneStillEnclosed(zone))
                    {
                        if (logZoneDetection)
                        {
                            Debug.Log($"CheckForSealing: Zone {zone.zoneId} is NOW ENCLOSED - converting to sealed!");
                        }
                        ConvertZoneToEnclosed(zone.zoneId);
                    }
                    else
                    {
                        if (logZoneDetection)
                        {
                            Debug.Log($"CheckForSealing: Zone {zone.zoneId} is still breached");
                        }
                    }
                }
                else
                {
                    if (logZoneDetection)
                    {
                        Debug.Log($"CheckForSealing: Created voxel {createdVoxelPos} is NOT adjacent to zone {zone.zoneId}");
                    }
                }
            }

            CheckForZoneSplit(createdVoxelPos);
        }

        public void CheckForZoneSplit(Vector3Int createdVoxelPos)
        {
            if (logZoneDetection)
            {
                Debug.Log($"CheckForZoneSplit: Checking if voxel {createdVoxelPos} splits any zones");
            }

            List<Vector3Int> adjacentPassableVoxels = new List<Vector3Int>();
            foreach (var neighbor in GetNeighbors(createdVoxelPos))
            {
                if (!bridge.IsValidPosition(neighbor)) continue;
                var neighborData = bridge.GetVoxel(neighbor);
                if (neighborData.IsPassable())
                {
                    adjacentPassableVoxels.Add(neighbor);
                }
            }

            if (adjacentPassableVoxels.Count < 2)
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForZoneSplit: Only {adjacentPassableVoxels.Count} passable neighbors, cannot split");
                }
                return;
            }

            ZoneInfo affectedZone = null;
            foreach (var zone in detectedZones.Values)
            {
                if (zone.voxels.Contains(adjacentPassableVoxels[0]))
                {
                    affectedZone = zone;
                    break;
                }
            }

            if (affectedZone == null)
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForZoneSplit: No zone found containing adjacent voxels");
                }
                return;
            }

            if (!AreVoxelsStillConnected(adjacentPassableVoxels[0], adjacentPassableVoxels, affectedZone))
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForZoneSplit: Zone {affectedZone.zoneId} has been SPLIT! Re-detecting zones...");
                }
                DetectAndCreateZones();
            }
            else
            {
                if (logZoneDetection)
                {
                    Debug.Log($"CheckForZoneSplit: Zone {affectedZone.zoneId} is still connected");
                }
            }
        }

        private bool AreVoxelsStillConnected(Vector3Int start, List<Vector3Int> targets, ZoneInfo zone)
        {
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(start);
            visited.Add(start);

            HashSet<Vector3Int> foundTargets = new HashSet<Vector3Int>();
            int iterations = 0;

            while (queue.Count > 0 && iterations < maxFloodFillDepth)
            {
                iterations++;
                Vector3Int current = queue.Dequeue();

                foreach (var target in targets)
                {
                    if (current == target)
                    {
                        foundTargets.Add(target);
                    }
                }

                if (foundTargets.Count == targets.Count)
                {
                    return true;
                }

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!bridge.IsValidPosition(neighbor)) continue;

                    var neighborData = bridge.GetVoxel(neighbor);
                    if (!neighborData.IsPassable()) continue;

                    if (!zone.voxels.Contains(neighbor)) continue;

                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }

            return foundTargets.Count == targets.Count;
        }

        private bool IsVoxelAdjacentToZone(Vector3Int voxel, ZoneInfo zone)
        {
            foreach (var neighbor in GetNeighbors(voxel))
            {
                if (zone.voxels.Contains(neighbor))
                    return true;
            }
            return false;
        }

        private bool IsZoneStillEnclosed(ZoneInfo zone)
        {
            foreach (var voxel in zone.voxels)
            {
                if (IsAtGridEdge(voxel) && !gridEdgesAreSealed)
                    return false;

                foreach (var neighbor in GetNeighbors(voxel))
                {
                    if (!bridge.IsValidPosition(neighbor))
                    {
                        if (!gridEdgesAreSealed)
                            return false;
                        continue;
                    }

                    var neighborData = bridge.GetVoxel(neighbor);
                    if (!neighborData.IsPassable() && !zone.voxels.Contains(neighbor))
                    {
                        continue;
                    }
                }
            }

            HashSet<Vector3Int> reachableFromStart = new HashSet<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            Vector3Int startVoxel = zone.voxels.First();
            queue.Enqueue(startVoxel);
            reachableFromStart.Add(startVoxel);

            bool foundEdge = false;
            int iterations = 0;

            while (queue.Count > 0 && iterations < maxFloodFillDepth)
            {
                iterations++;
                Vector3Int current = queue.Dequeue();

                if (IsAtGridEdge(current) && !gridEdgesAreSealed)
                {
                    foundEdge = true;
                    break;
                }

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (reachableFromStart.Contains(neighbor)) continue;

                    if (!bridge.IsValidPosition(neighbor))
                    {
                        if (!gridEdgesAreSealed)
                        {
                            foundEdge = true;
                            break;
                        }
                        continue;
                    }

                    var neighborData = bridge.GetVoxel(neighbor);
                    if (neighborData.IsPassable())
                    {
                        queue.Enqueue(neighbor);
                        reachableFromStart.Add(neighbor);
                    }
                }

                if (foundEdge) break;
            }

            return !foundEdge;
        }

        private bool IsAtGridEdge(Vector3Int pos)
        {
            return pos.x == 0 || pos.x == bridge.gridSize.x - 1 ||
                   pos.y == 0 || pos.y == bridge.gridSize.y - 1 ||
                   pos.z == 0 || pos.z == bridge.gridSize.z - 1;
        }

        private List<Vector3Int> GetNeighbors(Vector3Int pos)
        {
            return new List<Vector3Int>
            {
                pos + Vector3Int.right,
                pos + Vector3Int.left,
                pos + Vector3Int.up,
                pos + Vector3Int.down,
                pos + new Vector3Int(0, 0, 1),
                pos + new Vector3Int(0, 0, -1)
            };
        }

        private void ClearExistingZones()
        {
            foreach (var zone in detectedZones.Values)
            {
                if (zone.node != null)
                {
                    Destroy(zone.node.gameObject);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!showZoneGizmos || detectedZones == null) return;

            foreach (var zone in detectedZones.Values)
            {
                Gizmos.color = zone.isEnclosed ? enclosedZoneColor : globalZoneColor;
                Gizmos.DrawWireSphere(zone.centerPosition, 0.5f);
            }
        }

        public Dictionary<int, ZoneInfo> GetAllZones() => detectedZones;
    }
}
