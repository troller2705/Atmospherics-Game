# Visualization System Guide

Complete guide for 3D mesh-based visualization of voxels, atmospheric zones, and pipes.

---

## 🎨 Overview

The visualization system uses **actual 3D meshes** instead of Gizmos, making everything visible in Game view and builds:

- ✅ **Voxels** - Cubes showing solid/empty/partial voxels
- ✅ **Atmospheric Zones** - Spheres/cubes showing gas pressure/temperature  
- ✅ **Pipes** - Lines with animated flow indicators

---

## 🚀 Quick Start

### Option 1: Enable Visualizations on Existing Scene

**Menu:** `Atmospherics → Enable All Visualizations`

This automatically adds visualization components to:
- All voxel systems
- All atmospheric zones (VolumeNodes)
- All pipes

### Option 2: Rebuild Complete Test Scene

**Menu:** `Atmospherics → Rebuild Voxel Test Scene`

Creates a fresh voxel scene with:
- Two rooms connected by hallway
- All visualizations pre-enabled
- Runtime editor for testing

---

## 📦 Visualization Components

### 1. VoxelVisualizer

**Visualizes voxel grid as 3D cubes**

**Attach to:** GameObject with VoxelAtmosphericBridge

**Settings:**
```csharp
showSolidVoxels = true;      // Show walls (gray cubes)
showEmptyVoxels = false;     // Show air (usually hidden)
showPartialVoxels = true;    // Show damaged voxels (yellow)

// Colors
solidColor = gray semi-transparent
emptyColor = green transparent
partialColor = yellow fading
```

**What you see:**
- 🟦 Gray semi-transparent cubes = solid walls
- 🟨 Yellow fading cubes = damaged/partial voxels
- Nothing = empty air spaces

**Manual refresh:**
```csharp
VoxelVisualizer viz = GetComponent<VoxelVisualizer>();
viz.RebuildVisualization();  // Recreate all voxel meshes
```

---

### 2. AtmosphericZoneVisualizer

**Visualizes atmospheric zones with colored spheres/cubes**

**Attach to:** GameObject with VolumeNode

**Settings:**
```csharp
showZoneBoundary = true;          // Show zone mesh
showPressureColor = true;         // Color by pressure
showTemperatureColor = false;     // Color by temperature (alternative)

// Visual style
meshType = WireframeSphere;       // Sphere, Cube, or wireframe
sphereRadius = 1.5f;              // Size of zone visual
wireframeThickness = 0.05f;       // Line thickness for wireframe

// Auto-updates
updateInterval = 0.5f;            // How often to update color
```

**Color Coding (Pressure Mode):**
- 🔴 Red = Vacuum (< 10 kPa)
- 🟡 Yellow = Low pressure (10-50 kPa)
- 🟢 Green = Normal (50-100 kPa)
- 🔵 Cyan = High (100-150 kPa)
- 🟣 Magenta = Extreme (> 150 kPa)

**Color Coding (Temperature Mode):**
- 🔵 Blue = Cold (< 250K / -23°C)
- ⚪ White = Normal (250-320K / -23 to 47°C)
- 🔴 Red = Hot (> 320K / 47°C)

---

### 3. PipeVisualizer

**Visualizes pipes with animated flow**

**Attach to:** GameObject with Pipe

**Settings:**
```csharp
showPipe = true;               // Show pipe line
showFlowDirection = true;      // Show flow animation
animateFlow = true;            // Animate flow spheres

// Visual style
pipeThickness = 0.15f;         // Pipe line width
pipeSegments = 8;              // Line smoothness
pipeColor = gray semi-transparent
flowColor = cyan transparent

// Animation
flowSpeed = 1f;                // Animation speed
flowIndicatorCount = 3;        // Number of flow spheres
```

**What you see:**
- Gray line connecting two zones
- Cyan spheres moving along line (gas flow)
- Flow direction shows which way gas is moving

---

## 🎮 In Play Mode

### What You'll See

**Voxel Grid:**
```
🟦🟦🟦🟦🟦
🟦      🟦
🟦  ⚪  🟦    <- Atmospheric zone sphere
🟦      🟦
🟦🟦🟦🟦🟦

🟦 = Gray voxel cubes (walls)
⚪ = Colored sphere (atmosphere)
```

**Pipe Flow:**
```
Zone A ⚪━━⚪━━⚪━━━━→ Zone B
        ↑ Cyan spheres flowing
```

**Pressure Changes:**
- Zone sphere changes color based on pressure
- Watch it turn red if depressurizing!
- Watch flow spheres speed up with high pressure difference

---

## 💡 Usage Examples

### Example 1: Monitor Zone Pressure

```csharp
// Zone visualizer shows pressure in real-time
// Green = safe
// Yellow = caution
// Red = danger

// No code needed - just watch the sphere color!
```

---

### Example 2: Debug Gas Flow

```csharp
// Pipe visualizer shows:
// - Connection between zones (gray line)
// - Flow direction (sphere movement)
// - Flow rate (sphere speed)

// Watch spheres move from high pressure to low pressure
```

---

### Example 3: Visualize Voxel Mining

```csharp
VoxelTerraformingIntegration terraform = GetComponent<VoxelTerraformingIntegration>();

// Mine a voxel
terraform.OnVoxelDestroyed(new Vector3Int(5, 2, 3));

// VoxelVisualizer automatically removes the cube mesh
// AtmosphericZoneVisualizer updates zone size/color
```

---

### Example 4: Custom Visualization

```csharp
public class MyCustomVisualizer : MonoBehaviour
{
    public VoxelAtmosphericBridge bridge;

    void Update()
    {
        // Access voxel data
        foreach (var kvp in bridge.VoxelGrid)
        {
            Vector3Int pos = kvp.Key;
            var data = kvp.Value;

            // Create custom visualization based on voxel type
            if (data.type == VoxelAtmosphericBridge.VoxelType.Partial)
            {
                DrawDamageEffect(pos, data.density);
            }
        }

        // Access zone data
        foreach (var kvp in bridge.NodeToVoxels)
        {
            VolumeNode node = kvp.Key;
            List<Vector3Int> voxels = kvp.Value;

            // Create custom zone visualization
            DrawAtmosphericEffect(node.transform.position, node.Mixture.GetPressure());
        }
    }
}
```

---

## 🎨 Customization

### Change Voxel Colors

```csharp
VoxelVisualizer viz = GetComponent<VoxelVisualizer>();

// Make walls blue instead of gray
viz.solidColor = new Color(0, 0, 1, 0.8f);

// Make damaged voxels red
viz.partialColor = new Color(1, 0, 0, 0.6f);

// Rebuild to apply changes
viz.RebuildVisualization();
```

---

### Change Zone Visualization Style

```csharp
AtmosphericZoneVisualizer zoneViz = GetComponent<AtmosphericZoneVisualizer>();

// Use cube instead of sphere
zoneViz.meshType = AtmosphericZoneVisualizer.MeshType.WireframeCube;

// Make it bigger
zoneViz.sphereRadius = 3f;

// Show temperature instead of pressure
zoneViz.showPressureColor = false;
zoneViz.showTemperatureColor = true;

// Update faster
zoneViz.updateInterval = 0.1f;
```

---

### Customize Pipe Flow

```csharp
PipeVisualizer pipeViz = GetComponent<PipeVisualizer>();

// Make pipes thicker
pipeViz.pipeThickness = 0.3f;

// More flow indicators
pipeViz.flowIndicatorCount = 5;

// Faster animation
pipeViz.flowSpeed = 2f;

// Change colors
pipeViz.pipeColor = Color.white;
pipeViz.flowColor = Color.green;
```

---

### Custom Pressure Gradient

```csharp
AtmosphericZoneVisualizer viz = GetComponent<AtmosphericZoneVisualizer>();

// Create custom gradient
Gradient customGradient = new Gradient();
GradientColorKey[] colorKeys = new GradientColorKey[3];
colorKeys[0] = new GradientColorKey(Color.blue, 0f);    // Low
colorKeys[1] = new GradientColorKey(Color.white, 0.5f); // Medium
colorKeys[2] = new GradientColorKey(Color.red, 1f);     // High

GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
alphaKeys[0] = new GradientAlphaKey(0.5f, 0f);
alphaKeys[1] = new GradientAlphaKey(0.5f, 1f);

customGradient.SetKeys(colorKeys, alphaKeys);
viz.pressureGradient = customGradient;
```

---

## ⚡ Performance Tips

### 1. Limit Voxel Visualization

```csharp
VoxelVisualizer viz = GetComponent<VoxelVisualizer>();

// Only show solid voxels (walls)
viz.showSolidVoxels = true;
viz.showEmptyVoxels = false;    // Don't render air
viz.showPartialVoxels = false;  // Don't render damaged
```

---

### 2. Reduce Update Frequency

```csharp
AtmosphericZoneVisualizer viz = GetComponent<AtmosphericZoneVisualizer>();

// Update less often
viz.updateInterval = 2f;  // Every 2 seconds instead of 0.5
```

---

### 3. Disable Flow Animation

```csharp
PipeVisualizer viz = GetComponent<PipeVisualizer>();

// Show pipes but not flow animation
viz.showFlowDirection = false;
viz.animateFlow = false;
```

---

### 4. Toggle Visualization at Runtime

```csharp
// Turn off all voxel visualization
VoxelVisualizer viz = GetComponent<VoxelVisualizer>();
viz.enabled = false;  // Hides all voxel meshes

// Turn off zone visualization
AtmosphericZoneVisualizer zoneViz = GetComponent<AtmosphericZoneVisualizer>();
zoneViz.showZoneBoundary = false;

// Turn off pipe visualization
PipeVisualizer pipeViz = GetComponent<PipeVisualizer>();
pipeViz.showPipe = false;
```

---

## 🐛 Troubleshooting

### Can't See Voxels in Game View

**Check:**
1. VoxelVisualizer component is attached
2. `showSolidVoxels = true`
3. Camera is positioned to see voxel area
4. In Play mode (visualization builds at Start)

**Fix:**
```csharp
// Force rebuild in Play mode
VoxelVisualizer viz = GetComponent<VoxelVisualizer>();
viz.RebuildVisualization();
```

---

### Zones Not Showing

**Check:**
1. AtmosphericZoneVisualizer component attached to VolumeNode
2. `showZoneBoundary = true`
3. `sphereRadius` is large enough to see
4. Zone has valid Mixture data

**Fix:**
```csharp
// Add visualizer to existing zones
VolumeNode[] nodes = FindObjectsByType<VolumeNode>(FindObjectsSortMode.None);
foreach (var node in nodes)
{
    if (node.GetComponent<AtmosphericZoneVisualizer>() == null)
    {
        node.gameObject.AddComponent<AtmosphericZoneVisualizer>();
    }
}
```

---

### Pipes Not Visible

**Check:**
1. PipeVisualizer component attached to Pipe
2. `showPipe = true`
3. Both NodeA and NodeB exist
4. `pipeThickness` is large enough

**Fix:**
```csharp
// Add visualizer to existing pipes
Pipe[] pipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);
foreach (var pipe in pipes)
{
    if (pipe.GetComponent<PipeVisualizer>() == null)
    {
        pipe.gameObject.AddComponent<PipeVisualizer>();
    }
}
```

Or use: **Menu → `Atmospherics → Enable All Visualizations`**

---

### Materials Look Wrong (Pink/Black)

**Cause:** URP shader not found

**Fix:**
1. Check you're using URP render pipeline
2. Materials use "Universal Render Pipeline/Lit" shader
3. Project Settings → Graphics → Scriptable Render Pipeline Settings is set

---

### Voxels Update But Visualization Doesn't

**Fix:**
```csharp
VoxelVisualizer viz = GetComponent<VoxelVisualizer>();

// Enable auto-refresh
viz.autoRefresh = true;
viz.refreshInterval = 1f;  // Refresh every second

// Or manually trigger
viz.RebuildVisualization();
```

---

## 📋 Quick Reference

### Enable All Visualizations
```
Menu → Atmospherics → Enable All Visualizations
```

### Rebuild Test Scene
```
Menu → Atmospherics → Rebuild Voxel Test Scene
```

### Manual Component Setup
```csharp
// Voxels
VoxelVisualizer voxelViz = voxelSystem.AddComponent<VoxelVisualizer>();

// Zones
AtmosphericZoneVisualizer zoneViz = volumeNode.AddComponent<AtmosphericZoneVisualizer>();

// Pipes
PipeVisualizer pipeViz = pipe.AddComponent<PipeVisualizer>();
```

---

## 🎯 Best Practices

1. **Use visualizations during development** - Great for debugging
2. **Disable for release builds** - Set `enabled = false` for better performance
3. **Color code by gameplay state** - Pressure = danger level
4. **Animate based on events** - Flash zones when damaged
5. **Match your game's art style** - Customize colors and materials
6. **Toggle with debug key** - Let players turn on/off for screenshots

---

**Your atmospheric system is now fully visualized!** 🎨🌌
