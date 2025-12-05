# Atmospheric Gameplay Systems Guide

## Overview
This guide covers the life-support and hazard detection systems for your terraformation/exploration game.

---

## 🛠️ Device Systems

### CO₂ Scrubber

**Purpose:** Remove carbon dioxide from atmosphere and optionally produce oxygen.

**Script:** `Atmospherics.Devices.CO2Scrubber`

#### Setup
1. Create a GameObject for the scrubber
2. Add `CO2Scrubber` component
3. Assign a `VolumeNode` as the target
4. Configure settings

#### Properties

**Scrubber Settings:**
- `Scrub Rate` - Moles of CO₂ removed per second (default: 0.1)
- `Produces Oxygen` - Should it generate O₂?
- `Oxygen Conversion Ratio` - Moles of O₂ per mole of CO₂ (default: 0.5)

**Power:**
- `Requires Power` - Must be powered to work?
- `Is Powered` - Current power state
- `Power Consumption Watts` - Energy usage (default: 100W)

**Status:**
- `Is Active` - Currently running?
- `Total CO2 Scrubbed` - Lifetime statistics
- `Total O2 Produced` - Lifetime statistics

#### Usage

```csharp
// Get reference to scrubber
CO2Scrubber scrubber = GetComponent<CO2Scrubber>();

// Toggle on/off
scrubber.ToggleActive();

// Set power state
scrubber.SetPowered(true);

// Check CO2 levels
float co2Percent = scrubber.GetCO2PercentInNode();

// Get status
string status = scrubber.GetStatusText();
```

#### Integration with AtmosphericsManager

Add scrubbers to the manager's list:
1. Select `AtmosphericsManager`
2. Expand `Devices > Scrubbers`
3. Add your scrubber to the list

The manager will automatically call `UpdateScrubber()` each simulation tick.

---

### Gas Canister

**Purpose:** Portable gas storage for filling/emptying nodes.

**Script:** `Atmospherics.Devices.GasCanister`

#### Setup
1. Create a GameObject for the canister
2. Add `GasCanister` component
3. Configure capacity and contents
4. Connect to a node when needed

#### Properties

**Canister Properties:**
- `Canister Name` - Display name
- `Max Pressure` - Pressure limit (default: 5000 kPa)
- `Volume` - Internal volume (default: 0.1 m³)

**Connection:**
- `Connected Node` - Which node is it connected to?
- `Is Connected` - Connection status
- `Transfer Rate` - Moles per second

**Transfer Mode:**
- `Manual` - No automatic transfer
- `Fill` - Pull gas from node into canister
- `Empty` - Push gas from canister into node
- `Equalize` - Balance pressure between both

#### Usage

```csharp
GasCanister canister = GetComponent<GasCanister>();

// Connect to a node
canister.ConnectToNode(myVolumeNode);

// Set transfer mode
canister.mode = GasCanister.TransferMode.Fill;

// Disconnect
canister.Disconnect();

// Check status
float pressure = canister.GetPressure();
float fillPercent = canister.GetFillPercentage();
string status = canister.GetStatusText();
```

#### Use Cases

**Emergency O₂ Supply:**
```csharp
// Pre-filled canister with pure oxygen
canister.storedGas.Moles["O2"] = 50f;
canister.storedGas.Moles["N2"] = 0f;
canister.storedGas.Moles["CO2"] = 0f;
```

**CO₂ Waste Collection:**
```csharp
// Set to fill mode near scrubber
canister.mode = GasCanister.TransferMode.Fill;
canister.ConnectToNode(scrubberOutputNode);
```

---

## ⚠️ Hazard Detection System

### Atmospheric Hazards

**Purpose:** Monitor atmospheric conditions and warn of dangerous situations.

**Script:** `Atmospherics.Core.AtmosphericHazards`

#### Setup
1. Create a GameObject (or use existing node)
2. Add `AtmosphericHazards` component
3. Assign a `VolumeNode` to monitor
4. Configure threshold values

#### Hazard Levels

The system classifies conditions into 4 levels:

1. **Safe** ✅ - All parameters within safe ranges
2. **Warning** ⚠️ - Minor issues, no immediate danger
3. **Dangerous** 🔶 - Hazardous conditions, action needed soon
4. **Critical** 🔴 - Life-threatening, immediate action required

#### Thresholds

**Pressure (kPa):**
- Safe Range: 50 - 150 kPa
- Critical: < 20 or > 200 kPa

**Temperature (K / °C):**
- Safe Range: 273K (0°C) - 310K (37°C)
- Critical: < 250K (-23°C) or > 340K (67°C)

**Oxygen (%):**
- Safe Range: 18% - 25%
- Optimal: 21%
- Critical: < 10%

**CO₂ (%):**
- Safe: < 1%
- Dangerous: > 3%
- Critical: > 5%

#### Usage

```csharp
AtmosphericHazards hazards = GetComponent<AtmosphericHazards>();

// Check current status
bool isSafe = hazards.IsSafeForHumans();
string summary = hazards.GetHazardSummary();
Color warningColor = hazards.GetHazardColor();

// Get detailed readings
AtmosphericReadings readings = hazards.GetReadings();
Debug.Log($"Pressure: {readings.pressure:F1} kPa");
Debug.Log($"Temperature: {readings.TemperatureCelsius:F1}°C");
Debug.Log($"Oxygen: {readings.oxygenPercent:F1}%");
```

#### Events

The system fires Unity Events when hazard levels change:

- `onDangerDetected` - Enters Dangerous level
- `onCriticalDetected` - Enters Critical level
- `onReturnToSafe` - Returns to Safe level

**Example Integration:**
1. Select `AtmosphericHazards` component
2. Expand event (e.g., `On Critical Detected`)
3. Click `+` to add listener
4. Drag a GameObject and select a method

```csharp
// In your own script
public void OnAtmosphereCritical()
{
    Debug.LogError("CRITICAL ATMOSPHERE!");
    // Trigger alarms, lock doors, etc.
}
```

#### Integration with AtmosphericsManager

Add hazard monitors to the manager's list:
1. Select `AtmosphericsManager`
2. Expand `Hazard Monitoring > Hazard Monitors`
3. Add your monitor to the list

The manager will automatically call `UpdateHazards()` each tick.

---

## 🖥️ UI System

### Atmospheric HUD

**Purpose:** Display atmospheric status to the player.

**Script:** `Atmospherics.UI.AtmosphericHUD`

#### Setup
1. Create a Canvas if you don't have one
2. Add UI elements (Text or TextMeshPro)
3. Create a GameObject with `AtmosphericHUD` component
4. Assign UI references
5. Assign `AtmosphericHazards` reference

#### UI Elements

The HUD can display:
- Status (Safe/Warning/Dangerous/Critical)
- Pressure reading
- Temperature reading (°C and K)
- Oxygen percentage (color-coded)
- CO₂ percentage (color-coded)
- Warning panel background

#### Color Coding

**Oxygen:**
- Green: 18-25% (safe)
- Yellow: > 25% (high)
- Orange: < 18% (low)
- Red: < 10% (critical)

**CO₂:**
- Green: < 1% (safe)
- Yellow: 1-3% (elevated)
- Orange: 3-5% (dangerous)
- Red: > 5% (critical)

---

## 🎮 Gameplay Integration Examples

### Life Support Room

```
Setup:
1. Create a sealed atmospheric zone (player habitat)
2. Add CO₂ Scrubber connected to the room's node
3. Add AtmosphericHazards monitor
4. Add UI display

Result: Player sees O₂ decrease and CO₂ increase over time.
        Scrubber maintains breathable atmosphere when powered.
```

### Emergency Situation

```
Scenario: Power failure
1. Set scrubber.isPowered = false
2. Hazard monitor detects rising CO₂
3. Player must:
   - Restore power
   - Use emergency O₂ canister
   - Evacuate to another zone
```

### Terraformation Progression

```
Track planetary atmosphere improvement:
1. Monitor exterior node atmosphere
2. Compare O₂ levels over time
3. Unlock new areas as atmosphere becomes breathable
4. Show progress in UI
```

### Resource Management

```
Oxygen Production Chain:
1. Extract water (H₂O)
2. Use electrolysis to produce O₂
3. Store in canisters
4. Distribute to zones via pipes/pumps
5. Scrub CO₂ waste
```

---

## 💡 Tips & Best Practices

### Performance
- Use reasonable update intervals (0.5-1s for hazard checks)
- Don't create too many individual monitors
- One monitor per zone is usually sufficient

### Game Balance
- Adjust scrub rates based on your game's pacing
- Lower rates = more urgent survival gameplay
- Higher rates = more forgiving exploration

### Visual Feedback
- Use color-coded UI for quick status checks
- Add particle effects to devices when active
- Show pipe flow animations
- Play warning sounds on hazard level changes

### Realism vs Fun
Current thresholds are somewhat realistic but:
- Adjust for your game's difficulty
- Consider player experience over strict accuracy
- Real hypoxia happens < 16% O₂, but 18% gives warning time
- Real CO₂ toxicity is complex, simplified to percentages here

---

## 🔧 Extending the Systems

### Custom Gas Types

Add new gases beyond O₂, N₂, CO₂:

```csharp
// In your device script
node.Mixture.Moles["H2"] = 0.5f;  // Hydrogen
node.Mixture.Moles["He"] = 0.1f;  // Helium
```

Update hazard thresholds for toxic gases:
```csharp
// Custom hazard check
float hydrogenPercent = GetGasPercent("H2");
if (hydrogenPercent > 4f) // Explosive
{
    activeWarnings.Add("DANGER: Explosive gas mixture!");
}
```

### Gas-Specific Heat Capacities

Currently all gases use Cp = 29 J/(mol·K). For realism:

1. Update `GasMixture` to track per-gas heat capacities
2. Calculate weighted average Cp
3. Use in energy calculations

### Chemical Reactions

Add reactions in devices:
```csharp
// In a "Sabatier Reactor" device
float co2 = node.Mixture.Moles["CO2"];
float h2 = node.Mixture.Moles["H2"];

// CO₂ + 4H₂ → CH₄ + 2H₂O
float reactionRate = Mathf.Min(co2, h2 * 0.25f) * efficiencyFactor * dt;

node.Mixture.Moles["CO2"] -= reactionRate;
node.Mixture.Moles["H2"] -= reactionRate * 4f;
node.Mixture.Moles["CH4"] += reactionRate;
// Water production would need liquid phase system
```

---

## 📋 Quick Reference

### Adding a CO₂ Scrubber to a Scene
1. GameObject → Create Empty → Name: "CO2 Scrubber"
2. Add Component → CO2Scrubber
3. Assign target node
4. Add to AtmosphericsManager.Scrubbers list

### Adding Hazard Monitoring to a Zone
1. Select zone's main GameObject
2. Add Component → AtmosphericHazards
3. Assign a node from the zone
4. Add to AtmosphericsManager.HazardMonitors list
5. (Optional) Create UI and add AtmosphericHUD

### Checking if Area is Safe
```csharp
AtmosphericHazards hazards = GetComponent<AtmosphericHazards>();
if (hazards.IsSafeForHumans())
{
    // Allow player entry
}
```

---

*Combine with the Validation Guide for a complete atmospheric simulation system!*
