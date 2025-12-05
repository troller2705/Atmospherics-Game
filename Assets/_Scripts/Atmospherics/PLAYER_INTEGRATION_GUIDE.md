# Player Integration Guide

Complete guide for integrating atmospheric effects with player characters.

---

## 🎯 Quick Start

### Automated Setup (Recommended)

1. **Menu:** `Atmospherics → Player Setup`
2. **Assign** your player GameObject
3. **Configure** detection method
4. **Click** "Setup Player Atmospheric Systems"
5. **Done!** Enter Play mode and test

### Manual Setup

1. Select your player GameObject
2. Add `PlayerAtmosphericNeeds` component
3. Add `PlayerZoneDetector` component
4. (Optional) Add `PlayerAtmosphericDebugHUD` component
5. Configure settings in Inspector

---

## 📦 Components Overview

### 1. PlayerAtmosphericNeeds

**Core player atmospheric interaction system**

**What it does:**
- ✅ Consumes O₂ from current atmosphere
- ✅ Produces CO₂ (breathing simulation)
- ✅ Takes damage in hazardous conditions
- ✅ Manages health and stamina
- ✅ Fires events for game integration

**Key Properties:**

```csharp
// Breathing
oxygenConsumptionRate = 0.01f;      // Moles/sec consumed
co2ProductionRate = 0.008f;          // Moles/sec produced

// Health
maxHealth = 100f;
currentHealth = 100f;

// Stamina
maxStamina = 100f;
currentStamina = 100f;
staminaRegenRate = 10f;              // HP/sec regen

// Damage rates (HP/sec)
hypoxiaDamage = 5f;                  // Low O₂
co2ToxicityDamage = 10f;             // High CO₂
extremeColdDamage = 15f;             // Freezing
extremeHeatDamage = 15f;             // Burning
vacuumDamage = 20f;                  // No atmosphere
```

**Status Values:**
- `currentStatus` - "Normal", "Low Oxygen", "HYPOXIA", "CO₂ POISONING", "FREEZING", "BURNING", "VACUUM"
- `isSuffocating` - True if oxygen insufficient
- `isTakingDamage` - True if any damage occurring

**Usage:**

```csharp
PlayerAtmosphericNeeds needs = GetComponent<PlayerAtmosphericNeeds>();

// Check status
bool safe = !needs.isSuffocating && !needs.isTakingDamage;
float healthPercent = needs.GetHealthPercent();
string status = needs.GetStatusText();

// Get atmospheric readings
AtmosphericReadings readings = needs.GetCurrentReadings();
Debug.Log($"O₂: {readings.oxygenPercent:F1}%");

// Manually set zone
needs.SetCurrentNode(myVolumeNode);
```

---

### 2. PlayerZoneDetector

**Automatic atmospheric zone detection (radius-based)**

**What it does:**
- ✅ Finds nearest VolumeNode within radius
- ✅ Automatically updates player's current node
- ✅ Logs zone changes
- ✅ Shows debug gizmos

**Key Properties:**

```csharp
detectionRadius = 1f;                // How far to search
showDebugGizmos = true;              // Visualize in Scene view
```

**How it works:**
1. Every frame, searches for nearest VolumeNode
2. If distance < `detectionRadius`, sets as current node
3. If no nodes in range, player is in vacuum
4. Automatically calls `playerNeeds.SetCurrentNode()`

**Usage:**

```csharp
PlayerZoneDetector detector = GetComponent<PlayerZoneDetector>();

// Check current zone
if (detector.currentNode != null)
{
    Debug.Log($"In zone: {detector.currentNode.NodeName}");
}

// Check safety
bool safe = detector.IsInSafeAtmosphere();
```

**Visualization:**
- Green sphere = In a zone
- Red sphere = In vacuum
- Cyan line = Connection to current node

---

### 3. PlayerZoneTrigger

**Trigger-based zone detection (alternative to radius)**

**What it does:**
- ✅ Detects player entering/exiting zones via colliders
- ✅ More precise than radius detection
- ✅ Lower performance cost

**Setup:**
1. Create a trigger collider around your zone
2. Add `PlayerZoneTrigger` component to the trigger
3. Assign the associated `VolumeNode`
4. Ensure player has `Player` tag or assign manually

**Key Properties:**

```csharp
associatedNode;                      // Which node this trigger represents
autoDetectPlayer = true;             // Use "Player" tag
playerTag = "Player";                // Tag to look for
logZoneChanges = true;               // Debug logs
```

**When to use:**
- **Radius detection:** Quick setup, works everywhere, automatic
- **Trigger detection:** Precise room boundaries, better performance, requires manual trigger setup

---

### 4. PlayerAtmosphericDebugHUD

**On-screen debug display**

**What it does:**
- ✅ Shows player health, stamina, status
- ✅ Displays atmospheric readings
- ✅ Color-coded warnings
- ✅ Real-time updates

**Key Properties:**

```csharp
showHUD = true;
showDetailedReadings = true;
fontSize = 14;
```

**What it displays:**
- Status (Normal, Low Oxygen, etc.)
- Health and stamina bars
- Current zone name
- Pressure, temperature
- Gas percentages (O₂, CO₂, N₂)
- Hazard level
- Color indicators

---

### 5. PlayerHealthUI

**Production-ready UI integration**

**What it does:**
- ✅ Updates Unity UI elements
- ✅ Supports TextMeshPro
- ✅ Health/stamina bar fills
- ✅ Warning panels
- ✅ Flashing effects

**Setup:**
1. Create UI Canvas
2. Add UI elements (Images, Text)
3. Add `PlayerHealthUI` component
4. Assign references
5. Assign `playerNeeds`

**Supported UI Elements:**
- Health bar (Image with fill)
- Stamina bar (Image with fill)
- Status text (Text or TextMeshPro)
- Atmosphere readout text
- Warning text
- Warning panel (GameObject)
- Warning background (Image with flashing)

---

## 🎮 Gameplay Mechanics

### Breathing Simulation

**How it works:**

Every frame, the player:
1. Consumes O₂ from current node: `0.01 moles/sec` (default)
2. Produces CO₂ into current node: `0.008 moles/sec` (default)
3. Updates node temperature based on energy conservation

**Energy conservation:**
```csharp
// After changing mole counts
float totalMoles = node.Mixture.TotalMoles();
node.Mixture.Temperature = node.InternalEnergyJ / (totalMoles * Cp);
```

**Realistic rates:**
- Human O₂ consumption: ~0.008-0.012 moles/sec at rest
- CO₂ production: ~80% of O₂ consumption (respiratory quotient)
- Adjust for your game's pacing!

---

### Health Damage System

**Damage triggers:**

| Condition | Threshold | Damage (HP/sec) | Status |
|-----------|-----------|----------------|---------|
| **Vacuum** | Pressure < 20 kPa | 20 | "VACUUM" |
| **Hypoxia (Critical)** | O₂ < 10% | 5 | "HYPOXIA" |
| **Low Oxygen** | O₂ < 18% | 1.5 | "Low Oxygen" |
| **CO₂ Toxicity (Critical)** | CO₂ > 5% | 10 | "CO₂ POISONING" |
| **High CO₂** | CO₂ > 3% | 5 | "High CO₂" |
| **Extreme Cold** | T < 250K (-23°C) | 15 | "FREEZING" |
| **Extreme Heat** | T > 340K (67°C) | 15 | "BURNING" |

**Damage stacking:**
Multiple conditions can apply simultaneously (e.g., low O₂ + high CO₂).

---

### Stamina System

**How it works:**
- **Normal conditions:** Stamina regenerates at `10 HP/sec`
- **Low oxygen (< 18%):** Stamina drains at `20 HP/sec`
- **No atmosphere:** Stamina drains at `20 HP/sec`

**Gameplay uses:**
```csharp
PlayerAtmosphericNeeds needs = GetComponent<PlayerAtmosphericNeeds>();

// Disable sprinting if low stamina
bool canSprint = needs.currentStamina > 20f;

// Consume stamina for actions
if (isJumping)
{
    needs.currentStamina -= 10f;
}
```

---

## 🎯 Events Integration

### Unity Events

**Available events:**
- `onStartSuffocating` - Player begins suffocating
- `onStopSuffocating` - Player can breathe again
- `onDeath` - Player health reaches zero

**Setup in Inspector:**
1. Select `PlayerAtmosphericNeeds` component
2. Expand event (e.g., `On Start Suffocating`)
3. Click `+` to add listener
4. Drag GameObject and select method

**Example script:**

```csharp
public class PlayerAudio : MonoBehaviour
{
    public AudioSource breathingSound;
    public AudioSource alarmSound;

    // Called when player starts suffocating
    public void OnSuffocationStart()
    {
        alarmSound.Play();
        breathingSound.pitch = 1.5f; // Panicked breathing
    }

    // Called when player can breathe again
    public void OnSuffocationStop()
    {
        alarmSound.Stop();
        breathingSound.pitch = 1.0f; // Normal breathing
    }

    // Called when player dies
    public void OnPlayerDeath()
    {
        breathingSound.Stop();
        // Trigger death animation, respawn, etc.
    }
}
```

---

## 💡 Example Scenarios

### Scenario 1: Basic Survival

**Setup:**
- Player in sealed habitat with limited O₂
- No CO₂ scrubber active
- Emergency O₂ canister available

**Gameplay:**
1. Player spawns with 100% health
2. O₂ slowly depletes, CO₂ rises
3. After ~5 minutes: O₂ < 18%, stamina stops regenerating
4. After ~7 minutes: O₂ < 10%, health damage begins
5. Player must activate scrubber or use emergency canister

---

### Scenario 2: Airlock Challenge

**Setup:**
- Interior zone (breathable)
- Airlock zone (variable)
- Exterior zone (vacuum)

**Gameplay:**
1. Player enters airlock from interior
2. Airlock depressurizes (pumps to exterior)
3. If player stays too long: suffocation damage
4. Player must time exit before O₂ runs out
5. Exterior is instant vacuum damage

---

### Scenario 3: Damaged Habitat

**Setup:**
- Habitat with leak (unsealed zone)
- Atmosphere slowly escaping to exterior
- Scrubber struggling to keep up

**Gameplay:**
1. Normal conditions initially
2. Pressure slowly drops
3. Player notices status warnings
4. Must repair leak or evacuate
5. Rising tension as conditions worsen

---

### Scenario 4: Terraformation Progress

**Setup:**
- Planetary exterior starts at 1% O₂
- Terraforming slowly increases O₂
- Track progress over hours

**Gameplay:**
1. Early game: Exterior is deadly
2. Mid game: Can survive briefly outside
3. Late game: Exterior becomes breathable
4. Unlock exploration areas as atmosphere improves

---

## 🔧 Customization

### Adjust Breathing Rates

**For slower-paced games:**
```csharp
needs.oxygenConsumptionRate = 0.005f;  // Half speed
needs.co2ProductionRate = 0.004f;
```

**For faster-paced games:**
```csharp
needs.oxygenConsumptionRate = 0.02f;   // Double speed
needs.co2ProductionRate = 0.016f;
```

---

### Adjust Damage Rates

**For easier difficulty:**
```csharp
needs.hypoxiaDamage = 2f;              // 5 → 2
needs.co2ToxicityDamage = 5f;          // 10 → 5
needs.vacuumDamage = 10f;              // 20 → 10
```

**For hardcore mode:**
```csharp
needs.hypoxiaDamage = 10f;             // 5 → 10
needs.co2ToxicityDamage = 20f;         // 10 → 20
needs.vacuumDamage = 50f;              // 20 → 50
```

---

### Custom Status Messages

```csharp
// In your own script
public class CustomPlayerStatus : MonoBehaviour
{
    public PlayerAtmosphericNeeds needs;

    private void Update()
    {
        string custom = GetCustomStatus();
        // Display custom to player UI
    }

    private string GetCustomStatus()
    {
        var readings = needs.GetCurrentReadings();

        if (readings.oxygenPercent < 5f)
            return "You can't breathe!";
        
        if (readings.co2Percent > 7f)
            return "The air is toxic!";
        
        if (readings.TemperatureCelsius < -20f)
            return "You're freezing!";
        
        return "All systems nominal.";
    }
}
```

---

### Activity-Based Consumption

```csharp
public class PlayerActivity : MonoBehaviour
{
    public PlayerAtmosphericNeeds needs;

    public float restingRate = 0.008f;
    public float walkingRate = 0.012f;
    public float runningRate = 0.020f;

    private void Update()
    {
        // Adjust based on player activity
        if (isRunning)
        {
            needs.oxygenConsumptionRate = runningRate;
            needs.co2ProductionRate = runningRate * 0.8f;
        }
        else if (isWalking)
        {
            needs.oxygenConsumptionRate = walkingRate;
            needs.co2ProductionRate = walkingRate * 0.8f;
        }
        else
        {
            needs.oxygenConsumptionRate = restingRate;
            needs.co2ProductionRate = restingRate * 0.8f;
        }
    }
}
```

---

## 🐛 Troubleshooting

### Player Not Consuming Oxygen

**Possible causes:**
1. `isBreathing` is false
2. `currentNode` is null
3. Node has no O₂ in `Mixture.Moles`

**Fix:**
```csharp
Debug.Log($"Breathing: {needs.isBreathing}");
Debug.Log($"Node: {needs.currentNode?.NodeName}");
if (needs.currentNode != null)
{
    float o2 = needs.currentNode.Mixture.Moles["O2"];
    Debug.Log($"O₂ available: {o2}");
}
```

---

### Player Taking Damage Immediately

**Possible causes:**
1. Starting node has no O₂
2. Damage thresholds too strict
3. Zone not initialized properly

**Fix:**
```csharp
// Ensure zone has breathable air
node.Mixture.Moles["O2"] = 100f;
node.Mixture.Moles["N2"] = 400f;
node.Mixture.Moles["CO2"] = 1f;
node.Mixture.Temperature = 293f;  // 20°C
```

---

### Zone Detection Not Working

**For radius detection:**
1. Check detection radius is large enough
2. Ensure VolumeNode has Transform position
3. Check gizmos to see detection sphere

**For trigger detection:**
1. Ensure trigger has Collider with `Is Trigger = true`
2. Check player has Rigidbody or CharacterController
3. Verify player tag matches `playerTag`
4. Check trigger GameObject has `PlayerZoneTrigger` component

---

### Health Not Regenerating

Currently there's no automatic health regeneration. To add it:

```csharp
// In your own script
public class PlayerHealthRegen : MonoBehaviour
{
    public PlayerAtmosphericNeeds needs;
    public float regenRate = 5f;  // HP/sec

    private void Update()
    {
        if (!needs.isTakingDamage && !needs.isSuffocating)
        {
            needs.currentHealth = Mathf.Min(
                needs.maxHealth, 
                needs.currentHealth + regenRate * Time.deltaTime
            );
        }
    }
}
```

---

## 📊 Performance Considerations

### Update Frequencies

The system updates every frame by default. For optimization:

**Option 1: Fixed Update**
```csharp
// Change Update() to FixedUpdate() in PlayerAtmosphericNeeds
private void FixedUpdate()  // Instead of Update()
{
    // ... existing code
}
```

**Option 2: Update Interval**
```csharp
// Add to PlayerAtmosphericNeeds
public float updateInterval = 0.1f;
private float updateTimer = 0f;

private void Update()
{
    updateTimer += Time.deltaTime;
    if (updateTimer >= updateInterval)
    {
        // ... existing update code
        updateTimer = 0f;
    }
}
```

---

## 🎯 Best Practices

1. **Always validate node references** before accessing
2. **Use events** for audio, animations, game state changes
3. **Adjust rates** for your game's pacing
4. **Test in different conditions** (vacuum, extreme temp, etc.)
5. **Provide player feedback** (UI, audio, visual effects)
6. **Balance challenge vs fun** - don't make it too punishing
7. **Give players tools** to solve problems (canisters, scrubbers)

---

## 🚀 Next Steps

After integrating the player:

1. **Add UI** - Create proper health/stamina/status displays
2. **Add audio** - Breathing, alarms, warnings
3. **Add visual effects** - Vignette for low health, screen frost/heat
4. **Add gameplay systems** - Suits, helmets, portable oxygen
5. **Integrate with voxel system** - Dynamic zones based on construction

---

**Player integration complete! Your game now has atmospheric survival mechanics.** 🎮
