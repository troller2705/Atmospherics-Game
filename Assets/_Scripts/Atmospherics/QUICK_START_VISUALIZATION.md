# Quick Start: See Your Voxels & Atmosphere! 🎨

**Problem:** Can't see voxels or atmospheric zones in Game view?  
**Solution:** This 5-minute guide will get everything visible!

---

## ⚡ Fast Method (Recommended)

### Step 1: Enable All Visualizations

1. Open Unity menu bar
2. Click **`Atmospherics → Enable All Visualizations`**
3. Click "OK" on confirmation dialog

**What this does:**
- ✅ Adds `VoxelVisualizer` to all voxel systems
- ✅ Adds `AtmosphericZoneVisualizer` to all zones
- ✅ Adds `PipeVisualizer` to all pipes
- ✅ Configures everything automatically

### Step 2: Enter Play Mode

1. Click the Play button (or press `Ctrl+P` / `Cmd+P`)
2. Look at the Game view

**What you should see:**
```
🟦🟦🟦🟦🟦🟦🟦🟦🟦🟦
🟦              ⚪  🟦
🟦                  🟦
🟦      ⚪          🟦
🟦                  🟦
🟦🟦🟦🟦🟦🟦🟦🟦🟦🟦

🟦 = Gray voxel cubes (walls)
⚪ = Colored spheres (atmospheric zones)
━━ = Gray lines with cyan spheres (pipes with gas flow)
```

### Step 3: Test Runtime Editing

**Left-click in Game view** = Destroy voxel  
**Right-click in Game view** = Create voxel

**You should see:**
- Voxel cubes appear/disappear immediately
- Zone spheres update color based on pressure
- Flow spheres move through pipes

---

## 🔧 Alternative: Rebuild From Scratch

If you want a fresh test scene:

### Method: Rebuild Voxel Test Scene

1. **Menu:** `Atmospherics → Rebuild Voxel Test Scene`
2. Click "Yes" to confirm
3. Enter Play mode

**What you get:**
- ✅ Pre-built test scene with two rooms
- ✅ All visualizations enabled
- ✅ Runtime editor ready to use
- ✅ Example voxel layout

---

## 🎨 What Each Visualization Shows

### Voxel Cubes (Gray)
**Component:** `VoxelVisualizer`
- **Solid voxels** = Gray semi-transparent cubes
- **Partial voxels** = Yellow fading cubes (damage)
- **Empty voxels** = Hidden (or green if enabled)

### Zone Spheres (Colored)
**Component:** `AtmosphericZoneVisualizer`
- 🔴 **Red** = Low pressure / vacuum
- 🟡 **Yellow** = Below normal pressure
- 🟢 **Green** = Normal pressure (safe)
- 🔵 **Cyan** = High pressure
- 🟣 **Magenta** = Extreme pressure

### Pipe Lines (Gray with Cyan)
**Component:** `PipeVisualizer`
- **Gray line** = Pipe connection
- **Cyan spheres** = Gas flowing through pipe
- **Movement direction** = Which way gas is flowing
- **Speed** = How fast gas is flowing

---

## 🎯 Customization Tips

### Change Voxel Colors

Select the GameObject with `VoxelVisualizer`:

**Inspector Settings:**
- `Solid Color` = Change wall color
- `Empty Color` = Change air color
- `Partial Color` = Change damage color
- `Show Solid Voxels` = Toggle walls on/off
- `Show Empty Voxels` = Toggle air on/off

### Change Zone Visualization

Select a zone GameObject with `AtmosphericZoneVisualizer`:

**Inspector Settings:**
- `Show Zone Boundary` = Toggle sphere on/off
- `Show Pressure Color` = Color by pressure
- `Show Temperature Color` = Color by temperature
- `Mesh Type` = Sphere, Cube, or Wireframe
- `Sphere Radius` = Size of visualization

### Change Pipe Visualization

Select a pipe GameObject with `PipeVisualizer`:

**Inspector Settings:**
- `Show Pipe` = Toggle line on/off
- `Show Flow Direction` = Toggle flow spheres
- `Animate Flow` = Toggle animation
- `Pipe Thickness` = Line width
- `Flow Speed` = Animation speed

---

## 🐛 Troubleshooting

### "I still don't see anything!"

**Check these:**

1. **Are you in Play mode?**
   - Visualizations only appear during Play mode
   - Press the Play button to start

2. **Is your camera positioned correctly?**
   - Move Scene view camera to see the voxel area
   - Game view shows main camera's view
   - Voxels default to origin (0, 0, 0)

3. **Did the components get added?**
   - Select your voxel system GameObject
   - Check Inspector for `VoxelVisualizer` component
   - If missing, run menu command again

4. **Check Console for errors**
   - Open Console window (`Ctrl+Shift+C`)
   - Look for any red error messages
   - Fix errors if present

### "I see pink/magenta materials!"

**Cause:** Missing URP shader

**Fix:**
1. Check you're using Universal Render Pipeline
2. Project Settings → Graphics → Scriptable Render Pipeline Settings
3. Assign a URP asset if not set

### "Voxels appear but I can't edit them"

**Check:**
1. `VoxelRuntimeEditor` component is attached
2. You're clicking in Game view (not Scene view)
3. Camera has proper raycast setup
4. Voxel colliders are enabled

### "Zone colors don't update"

**Check:**
1. Zone has valid `VolumeNode` component
2. `Mixture` data is initialized
3. `Update Interval` isn't too high
4. Simulation is running (AtmosphericsManager exists)

---

## 📊 Performance Tips

### For Large Voxel Grids

**Optimize visualization:**

```csharp
VoxelVisualizer viz = GetComponent<VoxelVisualizer>();

// Only show walls (hide air)
viz.showSolidVoxels = true;
viz.showEmptyVoxels = false;
viz.showPartialVoxels = false;

// Disable auto-refresh
viz.autoRefresh = false;

// Manual refresh only when needed
viz.RebuildVisualization();
```

### For Many Zones

**Reduce update frequency:**

```csharp
AtmosphericZoneVisualizer zoneViz = GetComponent<AtmosphericZoneVisualizer>();

// Update less often
zoneViz.updateInterval = 2f;  // Every 2 seconds

// Or disable during normal gameplay
zoneViz.enabled = false;  // Turn off when not debugging
```

### For Production Builds

**Disable visualization in release:**

```csharp
#if UNITY_EDITOR || DEBUG_BUILD
    // Keep visualizations in editor and debug builds
    GetComponent<VoxelVisualizer>().enabled = true;
#else
    // Disable in production
    GetComponent<VoxelVisualizer>().enabled = false;
#endif
```

---

## 🎮 Testing Checklist

After enabling visualizations, test these:

- [ ] Voxel cubes visible in Game view
- [ ] Zone spheres visible and colored
- [ ] Pipe lines connecting zones
- [ ] Flow spheres moving through pipes
- [ ] Left-click destroys voxel (cube disappears)
- [ ] Right-click creates voxel (cube appears)
- [ ] Zone color changes when destroying voxels
- [ ] Zone size/position updates correctly
- [ ] Flow animation shows gas movement
- [ ] No pink/magenta materials
- [ ] No console errors
- [ ] Smooth performance

---

## 📖 Next Steps

Once visualization is working:

1. **Read the full guide:** [VISUALIZATION_GUIDE.md](VISUALIZATION_GUIDE.md)
2. **Customize colors:** Match your game's art style
3. **Integrate with gameplay:** Use colors to show danger
4. **Add effects:** Particle systems, sounds, etc.
5. **Optimize:** Disable when not needed

---

## 💡 Pro Tips

### Use Colors for Gameplay Feedback

```csharp
// Flash red when zone is breached
AtmosphericZoneVisualizer viz = zone.GetComponent<AtmosphericZoneVisualizer>();
viz.showPressureColor = true;  // Will automatically turn red at low pressure
```

### Toggle Visualization with Hotkey

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.V))
    {
        // Toggle all voxel visualizations
        VoxelVisualizer[] vizs = FindObjectsByType<VoxelVisualizer>(FindObjectsSortMode.None);
        foreach (var viz in vizs)
        {
            viz.enabled = !viz.enabled;
        }
    }
}
```

### Screenshot Mode

```csharp
// Hide all UI and visualizations for clean screenshots
public void EnableScreenshotMode(bool enable)
{
    var allVisualizers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
    foreach (var viz in allVisualizers)
    {
        if (viz is VoxelVisualizer || 
            viz is AtmosphericZoneVisualizer || 
            viz is PipeVisualizer)
        {
            viz.enabled = !enable;  // Disable when screenshot mode is on
        }
    }
}
```

---

**You're all set!** 🎉  
Your voxels and atmospherics are now fully visible with 3D meshes!

For detailed customization, see **[VISUALIZATION_GUIDE.md](VISUALIZATION_GUIDE.md)**
