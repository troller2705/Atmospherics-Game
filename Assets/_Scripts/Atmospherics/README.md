# Atmospheric Simulation System

Complete atmospheric simulation for voxel-based terraformation and exploration games.

## 📚 Documentation

- **[VALIDATION_GUIDE.md](VALIDATION_GUIDE.md)** - Energy conservation and testing
- **[GAMEPLAY_SYSTEMS_GUIDE.md](GAMEPLAY_SYSTEMS_GUIDE.md)** - Devices, hazards, and basic UI
- **[PLAYER_INTEGRATION_GUIDE.md](PLAYER_INTEGRATION_GUIDE.md)** - Player breathing, health, and atmospheric interaction
- **[UI_SYSTEMS_GUIDE.md](UI_SYSTEMS_GUIDE.md)** - Production UI, HUD, device controls, and visual effects
- **[VOXEL_INTEGRATION_GUIDE.md](VOXEL_INTEGRATION_GUIDE.md)** - Voxel-based terraformation and dynamic atmospheric zones
- **[VISUALIZATION_GUIDE.md](VISUALIZATION_GUIDE.md)** - 3D mesh visualization for voxels, zones, and pipes

---

## 🎯 Quick Start

### 1. Basic Atmospheric Simulation

**What you have:**
- ✅ Volume nodes (atmospheric containers)
- ✅ Pipes (passive gas flow)
- ✅ Pumps (active gas transfer)
- ✅ Heat transfer system
- ✅ Zone leak simulation
- ✅ Energy conservation

**How to use:**
1. Place `VolumeNode` objects in your scene
2. Create an `AtmosphericZone` and add nodes to it
3. Connect nodes with `Pipe` or `Pump` components
4. Watch gas flow and temperature changes in Play mode

### 2. Validation & Testing

**What you have:**
- ✅ Real-time energy monitoring
- ✅ NaN/negative value detection
- ✅ Automated test scenarios
- ✅ Debug visualization
- ✅ Validation reports

**How to use:**
1. Select `AtmosphericsManager` in your scene
2. Click "Add Validation System" in the Inspector
3. Enter Play mode
4. See on-screen debug HUD showing system health

### 3. Life Support & Gameplay

**What you have:**
- ✅ CO₂ Scrubber (removes CO₂, produces O₂)
- ✅ Gas Canister (portable storage)
- ✅ Hazard Detection (monitors safe conditions)
- ✅ UI System (displays atmosphere to player)

**How to use:**
1. Menu: `Atmospherics → Gameplay Systems Setup`
2. Select a target node
3. Click "Setup Complete Life Support System"
4. Configure in Inspector and test in Play mode

### 4. Player Integration

**What you have:**
- ✅ Player breathing (consumes O₂, produces CO₂)
- ✅ Health & stamina affected by atmosphere
- ✅ Damage from hazardous conditions
- ✅ Zone detection (radius or trigger-based)
- ✅ Debug HUD and UI systems

**How to use:**
1. Menu: `Atmospherics → Player Setup`
2. Select your player GameObject
3. Click "Setup Player Atmospheric Systems"
4. Enter Play mode and watch player interact with atmosphere

### 5. UI Systems

**What you have:**
- ✅ Complete player HUD (health, stamina, readouts)
- ✅ Atmospheric readout panel (P, T, gas percentages)
- ✅ Warning system (flashing alerts)
- ✅ Hazard level indicator
- ✅ Device control panels (scrubbers, canisters)
- ✅ World-space status displays
- ✅ Production-ready layouts

**How to use:**
1. Menu: `Atmospherics → UI Builder`
2. Configure which elements to include
3. Click "Build Complete UI"
4. Assign player reference and customize

### 6. Voxel Integration

**What you have:**
- ✅ Automatic atmospheric zones from voxel grids
- ✅ Dynamic zone updates when voxels change
- ✅ Terraforming integration (mining, building, damage)
- ✅ Pressure-based hull breach simulation
- ✅ Gas release on voxel destruction
- ✅ Runtime voxel editor for testing

**How to use:**
1. Menu: `Atmospherics → Voxel Integration Setup`
2. Configure grid size and voxel properties
3. Click "Create Voxel Atmospheric System" or "Create Test Scene"
4. Integrate with your voxel system or use runtime editor

### 7. 3D Mesh Visualization

**What you have:**
- ✅ Voxel visualization (3D cubes in Game view)
- ✅ Atmospheric zone visualization (colored spheres)
- ✅ Pipe visualization (animated flow lines)
- ✅ Pressure/temperature color coding
- ✅ Real-time visual feedback

**How to use:**
1. Menu: `Atmospherics → Enable All Visualizations`
2. Or menu: `Atmospherics → Rebuild Voxel Test Scene`
3. Enter Play mode to see 3D meshes
4. Customize colors and styles per component

---

## 🏗️ Project Structure

```
/Assets/_Scripts/Atmospherics
├── /Core                          # Core simulation
│   ├── AtmosphericsManager.cs     # Central manager
│   ├── VolumeNode.cs              # Gas container
│   ├── GasMixture.cs              # Gas mixture data
│   ├── Pipe.cs                    # Passive gas flow
│   ├── Pump.cs                    # Active gas transfer
│   ├── HeatTransfer.cs            # Thermal simulation
│   ├── AtmosphericZone.cs         # Zone management
│   ├── AtmosphericHazards.cs      # Hazard detection
│   ├── AtmosphericsValidator.cs   # Energy monitoring
│   └── AtmosphericsTestScenarios.cs # Automated tests
│
├── /Devices                       # Gameplay devices
│   ├── CO2Scrubber.cs             # CO₂ removal
│   └── GasCanister.cs             # Gas storage
│
├── /Player                        # Player integration
│   ├── PlayerAtmosphericNeeds.cs  # Breathing & health
│   ├── PlayerZoneDetector.cs      # Radius-based detection
│   ├── PlayerZoneTrigger.cs       # Trigger-based detection
│   ├── PlayerHealthUI.cs          # Production UI
│   └── PlayerAtmosphericDebugHUD.cs # Debug display
│
├── /UI                            # User interface
│   ├── AtmosphericHUD.cs          # Basic HUD display
│   ├── AtmosphericUIManager.cs    # Production UI manager
│   ├── DeviceControlPanel.cs      # Device control UI
│   └── SimpleStatusDisplay.cs     # Minimalist status display
│
├── /Voxel                         # Voxel integration
│   ├── VoxelAtmosphericBridge.cs  # Voxel-to-atmosphere bridge
│   ├── VoxelTerraformingIntegration.cs # Terraforming effects
│   └── VoxelRuntimeEditor.cs      # Runtime voxel editor
│
├── /Editor                        # Unity Editor tools
│   ├── VolumeNodeEditor.cs        # Node inspector
│   ├── VolumeNodeGasEditor.cs     # Gas mixture editor
│   ├── AtmosphericsSetupHelper.cs # Quick setup buttons
│   ├── GameplaySystemsSetup.cs    # Device setup window
│   ├── PlayerAtmosphericSetup.cs  # Player setup window
│   ├── AtmosphericUIBuilder.cs    # UI builder tool
│   ├── DeviceControlUIBuilder.cs  # Device UI builder
│   └── VoxelIntegrationSetup.cs   # Voxel setup window
│
├── README.md                      # This file
├── VALIDATION_GUIDE.md            # Testing guide
├── GAMEPLAY_SYSTEMS_GUIDE.md      # Gameplay guide
├── PLAYER_INTEGRATION_GUIDE.md    # Player guide
├── UI_SYSTEMS_GUIDE.md            # UI guide
└── VOXEL_INTEGRATION_GUIDE.md     # Voxel guide
```

---

## 🔬 Core Systems

### Gas Mixture
- Stores moles of each gas type (O₂, N₂, CO₂, etc.)
- Calculates pressure via ideal gas law: `PV = nRT`
- Tracks temperature in Kelvin
- Methods: `GetPressure()`, `SetPressure()`, `TotalMoles()`, `GetFractions()`

### Volume Node
- Container for gas mixtures
- Holds `InternalEnergyJ` (thermodynamic energy)
- Syncs with `GasMixture.Temperature`
- Configured with volume in m³

### Pipe
- Passive flow based on pressure difference
- Conductance parameter controls flow rate
- Transfers gas + enthalpy (heat energy)
- Immediately updates temperatures after flow

### Pump
- Active flow at fixed rate (moles/sec)
- Transfers gas proportionally by species
- Energy-conserving enthalpy transfer
- Can be powered on/off

### Heat Transfer
- Environmental heat loss: `Q = k × (T - T_ext)`
- Pipe thermal conduction between nodes
- Syncs `InternalEnergyJ ↔ Temperature`
- Uses specific heat Cp = 29 J/(mol·K)

### Atmospheric Zone
- Groups multiple nodes
- Sealed vs unsealed modes
- Leak simulation when unsealed
- Connects to exterior node

---

## 🎮 Gameplay Systems

### CO₂ Scrubber
**Life support device**
- Removes CO₂ from target node
- Optionally produces O₂
- Requires power (configurable)
- Tracks lifetime statistics

### Gas Canister
**Portable storage**
- Holds compressed gas
- Fill/Empty/Equalize modes
- Max pressure: 5000 kPa (default)
- Connect/disconnect from nodes

### Hazard Monitor
**Safety system**
- 4 levels: Safe, Warning, Dangerous, Critical
- Monitors pressure, temperature, O₂, CO₂
- Fires Unity Events on level changes
- Color-coded warnings

### HUD
**Player interface**
- Displays atmospheric readings
- Color-coded status
- Supports Unity UI and TextMeshPro
- Configurable update rate

### Player Systems
**Atmospheric interaction**
- Player consumes O₂, produces CO₂
- Health affected by atmosphere
- Stamina affected by low oxygen
- Damage from hazardous conditions
- Events for suffocation, death
- Zone detection (radius or trigger)

---

## 🔧 Editor Tools

### AtmosphericsManager Inspector
Enhanced with custom buttons:
- Add Heat Transfer System
- Add Pipe Renderer
- Add Validation System
- Add Test Scenarios
- Print Validation Report
- Run All Tests

### Gameplay Systems Setup Window
Menu: `Atmospherics → Gameplay Systems Setup`
- Create CO₂ Scrubber
- Create Gas Canister
- Create Hazard Monitor
- Setup Complete Life Support System

### Volume Node Inspector
- Edit gas composition visually
- Add/remove/rename gases
- Drag-and-drop gas management
- Real-time pressure/temperature display

---

## 📊 Validation Features

### Energy Conservation
- Tracks total system energy
- Detects unexpected drift
- Accounts for external heat transfer
- Configurable drift threshold

### State Validation
- NaN detection
- Negative value prevention
- Energy/temperature sync checks
- Per-node diagnostics

### Automated Tests
- Pressure equalization test
- Temperature equilibrium test
- Energy conservation test
- Extreme scenario tests

### Debug Visualization
- On-screen HUD (Play mode)
- Real-time statistics
- Warning display
- Detailed node info

---

## 🎯 Common Use Cases

### 1. Sealed Habitat
```
Goal: Maintain breathable atmosphere in sealed room

Setup:
- Create VolumeNode for room
- Add CO₂ Scrubber
- Add Hazard Monitor
- Set zone to Sealed = true

Result: Player consumes O₂, produces CO₂
        Scrubber maintains balance
        Monitor warns if levels dangerous
```

### 2. Airlock System
```
Goal: Transition between interior and exterior

Setup:
- Create 3 nodes: Interior, Airlock, Exterior
- Connect with pipes + pumps
- Control pumps to fill/empty airlock

Result: Realistic pressure equalization
        Energy-conserving gas transfer
```

### 3. Terraformation Progress
```
Goal: Track planetary atmosphere improvement

Setup:
- Monitor exterior node over time
- Display O₂ percentage
- Unlock areas when breathable

Result: Long-term gameplay progression
```

### 4. Emergency Situations
```
Goal: Power failure requires backup oxygen

Setup:
- Scrubber.requiresPower = true
- Power failure sets isPowered = false
- Player uses emergency O₂ canister

Result: Tension from failing life support
```

---

## ⚙️ Technical Details

### Physical Constants
- Gas constant R = 8.314 J/(mol·K)
- Specific heat Cp = 29 J/(mol·K) (constant for all gases)
- Temperature minimum = 0.1 K (prevents division by zero)

### Simulation Loop (Per Tick)
1. Heat transfer (environmental + pipe conduction)
2. Pump updates (active flow)
3. Pipe updates (passive flow)
4. Zone leaks (unsealed zones)
5. Device updates (scrubbers, canisters)
6. Hazard monitoring
7. Value clamping (safety)

### Energy Conservation
All gas transfers use enthalpy formula:
```
H = n × Cp × T
```
Where:
- n = moles transferred
- Cp = specific heat capacity
- T = source temperature

Temperature recalculated after every transfer:
```
T = E / (n × Cp)
```

---

## 🚀 Performance Tips

- Default simulation tick: 0.1 seconds
- Hazard updates: 0.5-1 second intervals
- Use zones to group related nodes
- Disable validation in production builds
- Limit number of active devices

---

## 🐛 Troubleshooting

### Energy Keeps Increasing
**Cause:** Energy/temperature desynchronization
**Fix:** Ensure all gas transfers recalculate temperatures immediately

### NaN Values
**Cause:** Division by zero (empty nodes)
**Fix:** Validation system prevents this; ensure nodes initialized

### Pressure Not Equalizing
**Cause:** Low pipe conductance or aggressive flow limiting
**Fix:** Increase `Pipe.Conductance` or reduce `maxMoveA/B` fraction

### High CO₂ Not Detected
**Cause:** Hazard monitor not in manager's list
**Fix:** Add to `AtmosphericsManager.HazardMonitors`

---

## 📈 Future Expansion Ideas

### Gas-Specific Properties
- Individual heat capacities per gas
- Molecular weights for diffusion
- Reaction temperatures

### Chemical Reactions
- Combustion (O₂ + fuel → CO₂ + H₂O + heat)
- Photosynthesis (CO₂ + light → O₂)
- Sabatier reaction (CO₂ + H₂ → CH₄)

### Advanced Features
- Gas diffusion without pipes
- Convection currents
- Explosive decompression
- Voxel-atmosphere integration

---

## 📝 Credits

Developed for voxel-based terraformation/exploration games.

Key Features:
- Thermodynamically accurate
- Energy-conserving
- Gameplay-ready
- Fully validated

---

## 🔗 Quick Links

- Validation Guide: `VALIDATION_GUIDE.md`
- Gameplay Guide: `GAMEPLAY_SYSTEMS_GUIDE.md`
- Setup Menu: `Atmospherics → Gameplay Systems Setup`
- Manager Component: `AtmosphericsManager`

---

**Ready to build breathtaking atmospheric gameplay!** 🌍
