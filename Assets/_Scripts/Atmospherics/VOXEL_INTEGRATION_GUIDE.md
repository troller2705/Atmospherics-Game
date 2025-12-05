# Voxel Integration Guide

Complete guide for integrating the atmospheric simulation with voxel-based terraformation systems.

---

## 🎯 Overview

The Voxel Integration system automatically:
- ✅ Creates atmospheric zones from connected voxel spaces
- ✅ Manages gas flow between voxel-based rooms
- ✅ Handles dynamic voxel destruction/creation
- ✅ Integrates with terraforming gameplay
- ✅ Simulates pressure-based damage and leaks

---

## 📦 Core Components

### 1. VoxelAtmosphericBridge

**Bridges voxel grid with atmospheric simulation**

**What it does:**
- Tracks 3D grid of voxels (Empty/Solid/Partial)
- Finds connected empty voxels (flood fill)
- Creates VolumeNode for each connected zone
- Auto-creates Pipes between adjacent zones
- Updates dynamically as voxels change

**Key Properties:**

```csharp
// Grid settings
gridSize = new Vector3Int(10, 10, 10);  // Voxel grid dimensions
voxelSize = 1f;                          // Size of each voxel (meters)
gridOrigin = Vector3.zero;               // World position of grid origin

// Atmospheric settings
volumePerVoxel = 1f;                     // Volume (m³) per voxel
autoCreateNodes = true;                  // Auto-build zones on Start
mergeAdjacentVoxels = true;              // Group connected voxels
minVoxelsForNode = 8;                    // Min voxels to create a node

// Dynamic updates
dynamicUpdates = true;                   // Auto-update on voxel changes
updateInterval = 1f;                     // Update frequency (seconds)
```

**How to use:**

```csharp
VoxelAtmosphericBridge bridge = GetComponent<VoxelAtmosphericBridge>();

// Set voxels
bridge.SetVoxel(5, 2, 3, VoxelAtmosphericBridge.VoxelType.Solid);
bridge.SetVoxel(new Vector3Int(1, 1, 1), VoxelAtmosphericBridge.VoxelType.Empty);

// Get voxel data
var voxelData = bridge.GetVoxel(5, 2, 3);
Debug.Log($"Type: {voxelData.type}, Density: {voxelData.density}");

// Clear a voxel
bridge.ClearVoxel(new Vector3Int(5, 2, 3));

// Rebuild atmospheric zones
bridge.RebuildAtmosphericZones();

// Get node at position
VolumeNode node = bridge.GetNodeAtPosition(playerPosition);
```

**Voxel Types:**

```csharp
VoxelType.Empty    // Passable, contains atmosphere
VoxelType.Solid    // Blocked, sealed wall
VoxelType.Partial  // Damaged, may leak atmosphere
```

---

### 2. VoxelTerraformingIntegration

**Handles terraforming effects on atmosphere**

**What it does:**
- Releases gas when voxels destroyed
- Creates leaks in damaged voxels
- Applies pressure-based damage
- Transfers heat between voxels and atmosphere
- Checks breathability

**Key Properties:**

```csharp
atmosphericBridge;                       // Reference to bridge

// Terraforming effects
enableAtmosphericLeaks = true;           // Damaged voxels leak gas
leakRatePerVoxel = 0.01f;                // Leak rate per damaged voxel
enablePressureBasedBreaking = true;      // High pressure breaks voxels
breakPressureThreshold = 500f;           // Pressure (kPa) that breaks voxels

// Gas release
releaseGasOnVoxelDestruction = true;     // Voxels release trapped gas
gasReleaseMultiplier = 1f;               // Amount of gas released

// Environmental
enableTemperatureTransfer = true;        // Heat conducts to voxels
heatConductivity = 0.1f;                 // Heat transfer rate
```

**How to use:**

```csharp
VoxelTerraformingIntegration terraform = GetComponent<VoxelTerraformingIntegration>();

// Notify voxel destroyed (mining, explosion, etc)
terraform.OnVoxelDestroyed(voxelPosition);

// Notify voxel created (building, construction)
terraform.OnVoxelCreated(voxelPosition, VoxelAtmosphericBridge.VoxelType.Solid);

// Damage voxel (partial destruction)
terraform.OnVoxelDamaged(voxelPosition, 0.5f);

// Check pressure damage
if (terraform.CheckPressureDamage(voxelPosition, out float damage))
{
    Debug.Log($"Voxel taking {damage * 100}% damage from pressure!");
}

// Get atmospheric data at position
float pressure = terraform.GetAtmosphericPressure(worldPos);
float temperature = terraform.GetAtmosphericTemperature(worldPos);
bool breathable = terraform.IsPositionBreathable(worldPos);
```

---

### 3. VoxelRuntimeEditor

**In-game voxel editing tool for testing**

**What it does:**
- Click to destroy/create voxels
- Visual target voxel highlighting
- On-screen voxel info display
- Real-time atmospheric feedback

**Key Properties:**

```csharp
bridge;                                  // VoxelAtmosphericBridge reference
terraforming;                            // VoxelTerraformingIntegration reference

// Input
destroyKey = KeyCode.Mouse0;             // Left-click to destroy
createKey = KeyCode.Mouse1;              // Right-click to create
raycastDistance = 100f;                  // Max distance for raycasts

// Edit settings
createType = VoxelType.Solid;            // Type to create
showTargetVoxel = true;                  // Highlight target
targetVoxelColor = Color.yellow;         // Highlight color

// UI
showOnScreenInfo = true;                 // Display info panel
infoFontSize = 14;                       // Font size
```

**How to use:**

```csharp
// Add to your voxel system GameObject
VoxelRuntimeEditor editor = gameObject.AddComponent<VoxelRuntimeEditor>();
editor.bridge = bridge;
editor.terraforming = terraform;

// In Play mode:
// - Aim at voxels (yellow wireframe shows target)
// - Left-click to destroy voxel
// - Right-click to create voxel
// - See on-screen info about target voxel and atmosphere
```

---

## 🛠️ Editor Tools

### Voxel Integration Setup Window

**Menu:** `Atmospherics → Voxel Integration Setup`

**Creates:**
- Complete voxel-atmospheric system
- Pre-configured bridge and terraforming
- Optional test scene with rooms

**Options:**
- Grid size configuration
- Voxel size and volume
- Dynamic updates toggle
- Terraforming integration

**Quick Test Scene:**
- Two sealed 3x3x3 rooms
- Connected by 1x1 hallway
- Automatic atmospheric zones
- Ready for runtime testing

---

## 💡 Example Workflows

### Example 1: Basic Voxel World

**Goal:** Create atmospheric zones in a voxel-based world

**Steps:**

```csharp
// 1. Create the system
GameObject systemObj = new GameObject("Voxel Atmosphere");
VoxelAtmosphericBridge bridge = systemObj.AddComponent<VoxelAtmosphericBridge>();
bridge.gridSize = new Vector3Int(50, 20, 50);
bridge.voxelSize = 1f;
bridge.volumePerVoxel = 1f;

// 2. Define your voxel world (from your existing voxel system)
for (int x = 0; x < 50; x++)
{
    for (int z = 0; z < 50; z++)
    {
        // Ground layer
        bridge.SetVoxel(x, 0, z, VoxelAtmosphericBridge.VoxelType.Solid);
    }
}

// 3. Build atmospheric zones
bridge.RebuildAtmosphericZones();

// Result: Atmospheric zones automatically created for open spaces
```

---

### Example 2: Mining Integration

**Goal:** Mine voxels and see atmosphere change

**Setup:**

```csharp
public class VoxelMiningTool : MonoBehaviour
{
    public VoxelTerraformingIntegration terraform;

    public void MineVoxel(Vector3Int position)
    {
        // Notify terraforming system
        terraform.OnVoxelDestroyed(position);
        
        // Your mining logic (drop resources, effects, etc)
        SpawnResources(position);
        PlayMiningEffect(position);
    }
}
```

**Result:**
- Mined voxel becomes empty space
- Trapped gas released into nearby zones
- Atmospheric zones automatically update
- Gas flows into newly opened space

---

### Example 3: Building/Construction

**Goal:** Place voxels and seal atmospheric zones

**Setup:**

```csharp
public class VoxelConstructor : MonoBehaviour
{
    public VoxelTerraformingIntegration terraform;

    public void PlaceVoxel(Vector3Int position)
    {
        // Notify terraforming system
        terraform.OnVoxelCreated(position, VoxelAtmosphericBridge.VoxelType.Solid);
        
        // Your construction logic
        SpawnVoxelVisual(position);
        PlayConstructionEffect(position);
    }
}
```

**Result:**
- Voxel placed and marked as solid
- Atmospheric zones split if connection broken
- Gas trapped in sealed areas

---

### Example 4: Pressure Hull Breach

**Goal:** Hull damage creates leaks and eventual breach

**Setup:**

```csharp
public class VoxelDamageSystem : MonoBehaviour
{
    public VoxelTerraformingIntegration terraform;
    public VoxelAtmosphericBridge bridge;

    private void Update()
    {
        // Check all voxels for pressure damage
        foreach (var voxelPos in GetExteriorWallVoxels())
        {
            if (terraform.CheckPressureDamage(voxelPos, out float damage))
            {
                terraform.OnVoxelDamaged(voxelPos, damage * Time.deltaTime);
            }
        }
    }

    private List<Vector3Int> GetExteriorWallVoxels()
    {
        // Return voxels that are walls exposed to space/high pressure
        // Implementation depends on your game structure
        return new List<Vector3Int>();
    }
}
```

**Result:**
- High internal pressure damages hull voxels
- Damaged voxels become Partial type
- Partial voxels leak atmosphere
- Eventually voxels break completely
- Explosive decompression!

---

### Example 5: Submarine/Spacecraft Sections

**Goal:** Multiple sealed compartments with airlocks

**Setup:**

```csharp
public void BuildSpacecraft()
{
    VoxelAtmosphericBridge bridge = GetComponent<VoxelAtmosphericBridge>();

    // Build hull (solid voxels)
    BuildHull(bridge, new Vector3Int(0, 0, 0), new Vector3Int(20, 5, 10));

    // Carve out three compartments
    CarveChamber(bridge, new Vector3Int(2, 1, 2), new Vector3Int(5, 3, 6));   // Room 1
    CarveChamber(bridge, new Vector3Int(8, 1, 2), new Vector3Int(11, 3, 6));  // Room 2
    CarveChamber(bridge, new Vector3Int(14, 1, 2), new Vector3Int(17, 3, 6)); // Room 3

    // Add airlocks (1 voxel connections)
    bridge.ClearVoxel(6, 2, 4);  // Airlock between Room 1 and 2
    bridge.ClearVoxel(12, 2, 4); // Airlock between Room 2 and 3

    // Build zones
    bridge.RebuildAtmosphericZones();
}

private void BuildHull(VoxelAtmosphericBridge bridge, Vector3Int min, Vector3Int max)
{
    for (int x = min.x; x <= max.x; x++)
        for (int y = min.y; y <= max.y; y++)
            for (int z = min.z; z <= max.z; z++)
                bridge.SetVoxel(x, y, z, VoxelAtmosphericBridge.VoxelType.Solid);
}

private void CarveChamber(VoxelAtmosphericBridge bridge, Vector3Int min, Vector3Int max)
{
    for (int x = min.x; x <= max.x; x++)
        for (int y = min.y; y <= max.y; y++)
            for (int z = min.z; z <= max.z; z++)
                bridge.ClearVoxel(x, y, z);
}
```

**Result:**
- Three separate atmospheric zones
- Gas flows through narrow airlocks
- Can seal off damaged sections
- Realistic compartmentalization

---

## 🎮 Integration with Existing Voxel Systems

### Integration Pattern

```csharp
public class YourVoxelSystem : MonoBehaviour
{
    public VoxelAtmosphericBridge atmosphericBridge;
    public VoxelTerraformingIntegration terraforming;

    // When YOUR voxel changes, notify the atmospheric system
    public void YourSetVoxelMethod(int x, int y, int z, bool isSolid)
    {
        // Your existing voxel logic
        yourVoxelData[x, y, z] = isSolid;
        UpdateYourVoxelMesh();

        // Notify atmospheric system
        var type = isSolid 
            ? VoxelAtmosphericBridge.VoxelType.Solid 
            : VoxelAtmosphericBridge.VoxelType.Empty;
        atmosphericBridge.SetVoxel(x, y, z, type);
    }

    // When YOUR voxel is destroyed
    public void YourDestroyVoxelMethod(int x, int y, int z)
    {
        // Your existing destruction logic
        yourVoxelData[x, y, z] = false;
        UpdateYourVoxelMesh();
        SpawnDebris(x, y, z);

        // Notify atmospheric system
        terraforming.OnVoxelDestroyed(new Vector3Int(x, y, z));
    }

    // Sync entire world at start
    private void Start()
    {
        SyncVoxelsWithAtmosphere();
    }

    private void SyncVoxelsWithAtmosphere()
    {
        for (int x = 0; x < worldSizeX; x++)
        {
            for (int y = 0; y < worldSizeY; y++)
            {
                for (int z = 0; z < worldSizeZ; z++)
                {
                    bool isSolid = yourVoxelData[x, y, z];
                    var type = isSolid 
                        ? VoxelAtmosphericBridge.VoxelType.Solid 
                        : VoxelAtmosphericBridge.VoxelType.Empty;
                    atmosphericBridge.SetVoxel(x, y, z, type);
                }
            }
        }

        atmosphericBridge.RebuildAtmosphericZones();
    }
}
```

---

## 🔬 Advanced Features

### Custom Voxel Properties

```csharp
// Extend VoxelData with your own properties
public class CustomVoxelBridge : VoxelAtmosphericBridge
{
    // Store material types per voxel
    private Dictionary<Vector3Int, MaterialType> voxelMaterials = 
        new Dictionary<Vector3Int, MaterialType>();

    public enum MaterialType
    {
        Steel,      // High pressure resistance
        Glass,      // Low pressure resistance, transparent
        Composite   // Medium resistance
    }

    public void SetVoxelWithMaterial(Vector3Int pos, VoxelType type, MaterialType material)
    {
        SetVoxel(pos, type);
        voxelMaterials[pos] = material;
    }

    public float GetPressureResistance(Vector3Int pos)
    {
        if (!voxelMaterials.ContainsKey(pos))
            return 100f; // Default

        switch (voxelMaterials[pos])
        {
            case MaterialType.Steel: return 1000f;
            case MaterialType.Glass: return 200f;
            case MaterialType.Composite: return 500f;
            default: return 100f;
        }
    }
}
```

---

### Temperature-Based Voxel Damage

```csharp
public class TemperatureDamageSystem : MonoBehaviour
{
    public VoxelTerraformingIntegration terraform;
    public float meltingPoint = 1800f; // Kelvin

    private void Update()
    {
        // Check voxels near hot zones
        foreach (var voxelPos in GetVoxelsNearHeat())
        {
            float temp = terraform.GetAtmosphericTemperature(
                terraform.atmosphericBridge.VoxelToWorldPosition(voxelPos)
            );

            if (temp > meltingPoint)
            {
                float damage = (temp - meltingPoint) / meltingPoint;
                terraform.OnVoxelDamaged(voxelPos, damage * Time.deltaTime);
            }
        }
    }
}
```

---

### Gas-Based Terraforming

```csharp
public class AtmosphericTerraformer : MonoBehaviour
{
    public VoxelAtmosphericBridge bridge;

    public void TeraformWithCO2(Vector3Int center, int radius)
    {
        // Release large amount of CO2 to terraform area
        VolumeNode node = bridge.GetNodeAtVoxel(center);
        if (node == null) return;

        // Add CO2 (greenhouse gas for warming)
        if (!node.Mixture.Moles.ContainsKey("CO2"))
            node.Mixture.Moles["CO2"] = 0f;

        node.Mixture.Moles["CO2"] += 1000f; // Large release

        // Recalculate temperature
        float totalMoles = node.Mixture.TotalMoles();
        node.Mixture.Temperature = node.InternalEnergyJ / (totalMoles * 29.0f);

        Debug.Log($"Released CO2 for terraforming. New temp: {node.Mixture.Temperature - 273.15f}°C");
    }
}
```

---

## 🎨 Visual Feedback

### Atmospheric Density Visualization

```csharp
public class AtmosphericRenderer : MonoBehaviour
{
    public VoxelAtmosphericBridge bridge;
    public Material atmosphereMaterial;

    private void Update()
    {
        // Render atmospheric density as colored cubes
        foreach (var kvp in bridge.nodeToVoxels)
        {
            VolumeNode node = kvp.Key;
            if (node == null || node.Mixture == null) continue;

            float pressure = node.Mixture.GetPressure();
            Color color = GetPressureColor(pressure);

            foreach (Vector3Int voxelPos in kvp.Value)
            {
                Vector3 worldPos = bridge.VoxelToWorldPosition(voxelPos);
                DrawAtmosphericVoxel(worldPos, color);
            }
        }
    }

    private Color GetPressureColor(float pressure)
    {
        if (pressure < 10f) return Color.clear;           // Vacuum
        if (pressure < 50f) return new Color(1, 0, 0, 0.1f);   // Low (red)
        if (pressure < 90f) return new Color(1, 1, 0, 0.1f);   // Medium (yellow)
        return new Color(0, 1, 0, 0.1f);                   // Normal (green)
    }

    private void DrawAtmosphericVoxel(Vector3 position, Color color)
    {
        // Use Graphics.DrawMesh or create particle effects
    }
}
```

---

## ⚡ Performance Tips

### 1. Batch Zone Rebuilds

```csharp
// Instead of rebuilding after every voxel change:
for (int i = 0; i < 100; i++)
{
    bridge.SetVoxel(positions[i], VoxelType.Empty);
    // DON'T call RebuildAtmosphericZones() here
}

// Rebuild once after all changes
bridge.RebuildAtmosphericZones();
```

---

### 2. Disable Dynamic Updates for Large Worlds

```csharp
// For massive voxel worlds
bridge.dynamicUpdates = false;

// Manually rebuild when needed
public void OnPlayerFinishedMining()
{
    bridge.RebuildAtmosphericZones();
}
```

---

### 3. Increase Update Interval

```csharp
// For less critical atmospheric updates
bridge.updateInterval = 5f; // Check every 5 seconds instead of 1
```

---

### 4. Limit Zone Size

```csharp
// Prevent massive single zones
bridge.minVoxelsForNode = 8;   // Min size
bridge.maxVoxelsForNode = 1000; // Add this limit (custom implementation)
```

---

## 🐛 Troubleshooting

### Zones Not Creating

**Check:**
1. `autoCreateNodes = true`
2. Enough connected voxels (≥ `minVoxelsForNode`)
3. Voxels set to `VoxelType.Empty`
4. Called `RebuildAtmosphericZones()`

**Fix:**
```csharp
Debug.Log($"Total voxels: {bridge.voxelGrid.Count}");
Debug.Log($"Min voxels needed: {bridge.minVoxelsForNode}");
bridge.RebuildAtmosphericZones();
```

---

### Gas Not Flowing Between Zones

**Check:**
1. Zones are adjacent (share voxel face)
2. Pipes created between zones
3. AtmosphericsManager exists and is running

**Fix:**
```csharp
// Check pipe count
AtmosphericsManager manager = FindFirstObjectByType<AtmosphericsManager>();
Debug.Log($"Total pipes: {manager.AllPipes.Count}");
```

---

### Performance Issues

**Symptoms:**
- Low FPS
- Stuttering when voxels change

**Solutions:**
1. Disable `dynamicUpdates`
2. Increase `updateInterval`
3. Reduce `gridSize`
4. Batch voxel changes
5. Use spatial partitioning for large worlds

---

### Voxels and Nodes Desync

**Check:**
1. Using `SetVoxel()` not direct modification
2. Calling `RebuildAtmosphericZones()` after batch changes

**Fix:**
```csharp
// Force full resync
bridge.RebuildAtmosphericZones();
```

---

## 🎯 Best Practices

1. **Batch voxel changes** - Don't rebuild after every single voxel
2. **Use terraforming integration** - Proper gas release and leak handling
3. **Limit dynamic updates** - Only enable for small/medium worlds
4. **Set appropriate voxel size** - Match your game scale
5. **Seal important areas** - Mark critical voxels as sealed
6. **Test with runtime editor** - Use VoxelRuntimeEditor for rapid testing
7. **Visualize atmospheric zones** - Debug gizmos show zone boundaries
8. **Profile regularly** - Monitor performance with large voxel counts

---

## 🚀 Next Steps

After setting up voxel integration:

1. **Integrate with your voxel system** - Add atmospheric calls to your code
2. **Test with runtime editor** - Verify zones form correctly
3. **Add visual feedback** - Show atmospheric density/pressure
4. **Implement damage** - Pressure-based hull breach system
5. **Create airlocks** - Player-controlled zone separation
6. **Add sound effects** - Hissing leaks, decompression warnings

---

**Your voxel-atmospheric integration is ready!** ⛏️🌌
