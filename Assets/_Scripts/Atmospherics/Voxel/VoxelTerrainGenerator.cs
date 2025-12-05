using UnityEngine;

namespace Atmospherics.Voxel
{
    [RequireComponent(typeof(VoxelAtmosphericBridge))]
    public class VoxelTerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Generation")]
        [Tooltip("Generate voxels automatically on start")]
        public bool autoGenerate = true;

        [Tooltip("Type of terrain to generate")]
        public TerrainType terrainType = TerrainType.FlatFloor;

        [Header("Floor Settings")]
        [Range(1, 10)]
        [Tooltip("Height of the floor in voxels")]
        public int floorHeight = 1;

        [Header("Room Settings")]
        [Tooltip("Create enclosed room")]
        public bool createWalls = true;

        [Range(3, 20)]
        [Tooltip("Height of room walls")]
        public int wallHeight = 5;

        private VoxelAtmosphericBridge bridge;

        public enum TerrainType
        {
            FlatFloor,
            Room,
            Terrain,
            Custom
        }

        private void Start()
        {
            bridge = GetComponent<VoxelAtmosphericBridge>();

            if (bridge == null)
            {
                Debug.LogError("VoxelTerrainGenerator: No VoxelAtmosphericBridge found!");
                return;
            }

            if (autoGenerate)
            {
                GenerateTerrain();
            }
        }

        public void GenerateTerrain()
        {
            switch (terrainType)
            {
                case TerrainType.FlatFloor:
                    GenerateFlatFloor();
                    break;
                case TerrainType.Room:
                    GenerateRoom();
                    break;
                case TerrainType.Terrain:
                    GeneratePerlinTerrain();
                    break;
            }

            var zoneDetector = GetComponent<VoxelZoneDetector>();
            if (zoneDetector != null)
            {
                zoneDetector.DetectAndCreateZones();
            }
            else
            {
                bridge.RebuildAtmosphericZones();
            }
            
            var visualizer = GetComponent<VoxelVisualizer>();
            if (visualizer != null)
            {
                visualizer.RebuildVisualization();
            }

            Debug.Log($"VoxelTerrainGenerator: Generated {terrainType} terrain");
        }

        private void GenerateFlatFloor()
        {
            for (int x = 0; x < bridge.gridSize.x; x++)
            {
                for (int z = 0; z < bridge.gridSize.z; z++)
                {
                    for (int y = 0; y < floorHeight; y++)
                    {
                        bridge.SetVoxel(x, y, z, VoxelAtmosphericBridge.VoxelType.Solid, 1f, true);
                    }
                }
            }
        }

        private void GenerateRoom()
        {
            GenerateFlatFloor();

            if (!createWalls) return;

            for (int x = 0; x < bridge.gridSize.x; x++)
            {
                for (int z = 0; z < bridge.gridSize.z; z++)
                {
                    bool isEdge = x == 0 || x == bridge.gridSize.x - 1 || 
                                  z == 0 || z == bridge.gridSize.z - 1;

                    if (isEdge)
                    {
                        for (int y = floorHeight; y < wallHeight; y++)
                        {
                            bridge.SetVoxel(x, y, z, VoxelAtmosphericBridge.VoxelType.Solid, 1f, true);
                        }
                    }
                }
            }
        }

        private void GeneratePerlinTerrain()
        {
            float scale = 0.15f;
            float heightMultiplier = 3f;

            for (int x = 0; x < bridge.gridSize.x; x++)
            {
                for (int z = 0; z < bridge.gridSize.z; z++)
                {
                    float perlinValue = Mathf.PerlinNoise(x * scale, z * scale);
                    int height = Mathf.FloorToInt(perlinValue * heightMultiplier) + 1;

                    for (int y = 0; y < height && y < bridge.gridSize.y; y++)
                    {
                        bridge.SetVoxel(x, y, z, VoxelAtmosphericBridge.VoxelType.Solid, 1f, true);
                    }
                }
            }
        }

        [ContextMenu("Regenerate Terrain")]
        public void RegenerateTerrain()
        {
            ClearAllVoxels();
            GenerateTerrain();
        }

        [ContextMenu("Clear All Voxels")]
        public void ClearAllVoxels()
        {
            for (int x = 0; x < bridge.gridSize.x; x++)
            {
                for (int y = 0; y < bridge.gridSize.y; y++)
                {
                    for (int z = 0; z < bridge.gridSize.z; z++)
                    {
                        bridge.ClearVoxel(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }
}
