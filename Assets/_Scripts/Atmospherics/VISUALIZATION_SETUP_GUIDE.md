# Voxel Atmospheric Visualization Setup Guide

This guide explains how to set up and use the atmospheric visualization system for your voxel-based atmospherics.

## Components Overview

### 1. VoxelAtmosphericVisualizer
**Purpose:** Renders color-coded overlay cubes on voxels to visualize atmospheric properties.

**Features:**
- **Pressure Mode**: Blue (low) → Green (normal) → Red (high)
- **Oxygen Mode**: Red (no O₂) → Yellow (low) → Green (breathable)
- **Temperature Mode**: Blue (cold) → White (room temp) → Red (hot)
- **Breathability Mode**: Green (safe), Yellow (low O₂), Red (dangerous)

### 2. VoxelGasFlowEffects
**Purpose:** Creates particle effects showing gas flow between areas of different pressure.

**Features:**
- Auto-detects pressure differences
- Particle direction shows flow direction
- Particle color/speed based on pressure difference
- Auto-cleanup of old emitters

### 3. VoxelVisualizationUI
**Purpose:** Runtime UI controls to toggle visualization modes and effects.

**Features:**
- Button panel to switch modes
- Keyboard shortcuts
- Toggle flow effects on/off
- Show active emitter count

---

## Quick Setup

### Step 1: Add Components to Your Voxel Bridge GameObject

In the Unity Inspector, add these components to your GameObject that has `VoxelAtmosphericBridge`:

1. **VoxelAtmosphericVisualizer**
2. **VoxelGasFlowEffects**
3. **VoxelVisualizationUI**

### Step 2: Configure VoxelAtmosphericVisualizer

**Recommended Settings:**
- **Current Mode**: `None` (start with no visualization)
- **Show Only Passable Voxels**: `True` (only visualize air, not walls)
- **Update Interval**: `0.1` seconds (10 FPS updates)
- **Overlay Alpha**: `0.5` (semi-transparent overlays)

**Pressure Settings:**
- **Min Pressure**: `0` kPa
- **Max Pressure**: `200` kPa (normal is ~101 kPa)

**Oxygen Settings:**
- **Min Oxygen Percent**: `0%`
- **Max Oxygen Percent**: `30%` (normal is ~21%)

**Temperature Settings:**
- **Min Temperature**: `200` K (-73°C, very cold)
- **Max Temperature**: `350` K (77°C, very hot)

### Step 3: Configure VoxelGasFlowEffects

**Recommended Settings:**
- **Auto Detect Flow**: `True`
- **Max Active Emitters**: `20` (performance vs visual quality)
- **Emitter Lifetime**: `2` seconds
- **Minimum Pressure Difference**: `20` kPa (ignore small differences)
- **Particle Speed**: `2` m/s
- **Particle Size**: `0.1` units

### Step 4: Configure VoxelVisualizationUI

**Recommended Settings:**
- **Show UI**: `True`
- **Toggle UI Key**: `Tab`
- **Cycle Mode Key**: `V`
- **Toggle Flow Effects Key**: `F`

**Auto-References:** The component will automatically find `VoxelAtmosphericVisualizer` and `VoxelGasFlowEffects` on the same GameObject.

---

## Runtime Usage

### Keyboard Controls

| Key | Action |
|-----|--------|
| **Tab** | Toggle UI panel on/off |
| **V** | Cycle through visualization modes |
| **F** | Toggle gas flow particle effects |

### Visualization Modes

**None**
- No overlays shown
- Default mode, best performance

**Pressure**
- **Blue**: Low pressure (vacuum)
- **Cyan**: Below normal
- **Green**: Normal pressure (~101 kPa)
- **Yellow**: High pressure
- **Red**: Very high pressure (danger!)

**Oxygen**
- **Red**: No oxygen (0-5%)
- **Yellow**: Low oxygen (5-19%)
- **Green**: Breathable oxygen (19-30%)

**Temperature**
- **Dark Blue**: Freezing (~200K / -73°C)
- **Cyan**: Cold
- **White**: Room temperature (~293K / 20°C)
- **Yellow**: Warm
- **Red**: Hot (~350K / 77°C)

**Breathability**
- **Green**: Safe to breathe (19-23% O₂, 80-120 kPa pressure)
- **Yellow**: Low oxygen or wrong pressure
- **Red**: Deadly (vacuum, no O₂, extreme pressure)

---

## Performance Tips

### Optimization Settings

**For Better Performance:**
- Increase `updateInterval` to 0.2 or 0.5 seconds
- Reduce `maxActiveEmitters` to 10
- Set `showOnlyPassableVoxels` to `True`
- Use `None` mode when not debugging

**For Better Visuals:**
- Decrease `updateInterval` to 0.05 seconds
- Increase `maxActiveEmitters` to 50
- Lower `minimumPressureDifference` to 10 kPa
- Use smaller `particleSize` for finer detail

### Memory Considerations

Each overlay cube creates a GameObject with a MeshRenderer. For a 20×20×20 grid:
- Empty voxels: ~8,000 cubes
- Only passable voxels might be 50-500 cubes (much better!)

**Recommendation:** Always use `showOnlyPassableVoxels = true` for large grids.

---

## Testing Scenarios

### Test 1: Breach Visualization

1. Set mode to **Pressure**
2. Enable **Gas Flow Effects**
3. Destroy a wall voxel in a sealed room
4. **Expected Result:**
   - Room voxels turn blue (depressurizing)
   - Particle effects show gas rushing out
   - Pressure gradient visible at breach point

### Test 2: Temperature Gradient

1. Set mode to **Temperature**
2. Create a fire or heat source (if implemented)
3. **Expected Result:**
   - Hot areas show red
   - Cool areas show blue
   - Gradient shows heat dissipation

### Test 3: Oxygen Depletion

1. Set mode to **Oxygen**
2. Seal a room and consume oxygen (fire, player, etc.)
3. **Expected Result:**
   - Voxels gradually shift from green → yellow → red
   - Clear visual warning of suffocation danger

### Test 4: Zone Splitting

1. Set mode to **Breathability**
2. Build a wall through the middle of a room
3. **Expected Result:**
   - Both new zones show green (breathable)
   - Clear separation visible
   - Each zone has independent atmosphere

---

## Common Issues

### Issue: No Overlays Visible

**Solutions:**
- Check that mode is not `None`
- Verify voxels have atmospheric nodes
- Check that voxels are passable (Empty type)
- Ensure `VoxelZoneDetector` has run detection

### Issue: Overlays Are Solid Blocks

**Solutions:**
- Check material transparency settings
- Increase `overlayAlpha` value
- Verify URP material setup is correct

### Issue: No Particle Effects

**Solutions:**
- Press `F` to enable flow effects
- Check `autoDetectFlow` is `True`
- Verify pressure difference exceeds `minimumPressureDifference`
- Ensure particles aren't culled by camera

### Issue: Poor Performance

**Solutions:**
- Increase `updateInterval`
- Reduce `maxActiveEmitters`
- Enable `showOnlyPassableVoxels`
- Disable flow effects when not needed

---

## Advanced Customization

### Custom Gradients

You can customize the color gradients in the Inspector:

1. Select the `VoxelAtmosphericVisualizer` component
2. Expand the gradient fields (Pressure, Oxygen, Temperature)
3. Click the gradient bar to open the Gradient Editor
4. Add/remove/adjust color stops

### Custom Particle Prefab

To use your own particle effect:

1. Create a GameObject with a ParticleSystem
2. Configure it as desired
3. Save as a prefab
4. Assign to `VoxelGasFlowEffects.gasFlowParticlePrefab`

### Shader Customization

The default overlay material uses URP/Lit with transparency. To customize:

1. Create a new material
2. Use a shader with transparency support
3. Assign to `VoxelAtmosphericVisualizer.overlayMaterial`

---

## Integration with Existing Systems

### VoxelRuntimeEditor

The visualizer automatically refreshes when:
- Voxels are created/destroyed
- Zones are detected/split/merged
- `OnZonesChanged` event fires

### VoxelZoneDetector

Zone changes trigger visualization updates via the `OnZonesChanged` event.

### AtmosphericsSimulation

Atmospheric values (pressure, temperature, composition) are read every `updateInterval` to update colors.

---

## Next Steps

After setting up visualization, you can:

1. **Implement Player Integration**: Make player respond to atmospheric conditions
2. **Add Vents/Pipes**: Create controllable connections between zones
3. **Implement Fire/Combustion**: Visualize oxygen consumption and heat
4. **Add Sound Effects**: Audio feedback for depressurization, flow, etc.

---

**Happy Visualizing!** 🎨🚀
