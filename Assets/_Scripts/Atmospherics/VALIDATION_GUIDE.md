# Atmospheric System Validation Guide

## Overview
The validation system ensures your atmospheric simulation maintains thermodynamic accuracy and energy conservation.

## Setup

### 1. Add Validator Component
1. Select your `AtmosphericsManager` GameObject in the scene
2. Add the `AtmosphericsValidator` component
3. The validator will automatically reference the manager

### 2. Add Test Scenarios (Optional)
1. Add the `AtmosphericsTestScenarios` component to the same GameObject
2. This provides automated testing capabilities

## Using the Validator

### Real-Time Monitoring
When enabled, the validator displays an on-screen HUD showing:
- **Total System Energy** - Sum of all internal energy in all nodes
- **Total System Moles** - Total gas amount in the system
- **Average Temperature** - Energy-weighted average
- **Warnings** - Any detected issues

### Settings

**Enable Validation** - Turn on/off the validation system

**Log Energy Drift** - Logs warnings when energy changes unexpectedly

**Energy Drift Threshold** - How much energy change triggers a warning (default: 1000 J)

**Validation Interval** - How often to check (default: 1 second)

**Show Debug GUI** - Display the on-screen monitoring HUD

**Show Detailed Stats** - Show individual node information in HUD

## Validation Checks

The validator performs these checks:

### 1. Energy Conservation
Tracks total system energy frame-to-frame. Small changes are expected due to:
- External heat transfer (nodes losing heat to environment)
- Pipe thermal conduction

Large unexpected changes indicate bugs.

### 2. NaN Detection
Checks for `NaN` (Not a Number) values in:
- Temperature
- Internal Energy
- Pressure
- Mole counts

### 3. Negative Value Detection
Ensures no negative values for:
- Temperature (should be > 0 K)
- Energy (should be >= 0 J)
- Moles (should be >= 0)

### 4. Energy/Temperature Sync
Verifies that `InternalEnergyJ` matches the calculated value:
```
E = n × Cp × T
```
Where:
- n = total moles
- Cp = 29 J/(mol·K)
- T = temperature

## Running Tests

### Manual Testing
Right-click the `AtmosphericsTestScenarios` component and select:
- **Test: Pressure Equalization** - Verifies connected nodes equalize pressure
- **Test: Temperature Equilibrium** - Verifies thermal equilibrium is reached
- **Test: Energy Conservation** - Checks for energy drift over time
- **Run All Tests** - Runs the complete test suite

### Automated Testing
Enable `Auto Run Tests` on the `AtmosphericsTestScenarios` component to run tests automatically on Play.

### Test Scenarios
The component also provides context menu options to create specific test conditions:
- **Create Extreme Pressure Test** - Increases pressure 10x in one node
- **Create Temperature Gradient Test** - Sets 400K vs 200K temperature difference

## Interpreting Results

### ✓ PASS (Green)
The system is working correctly and maintaining physical accuracy.

### ⚠ ACCEPTABLE/PARTIAL (Yellow)
Minor issues detected but within acceptable ranges. May indicate:
- System still equilibrating
- Expected heat loss to environment
- Numerical precision limits

### ✗ FAIL (Red)
Significant issues detected:
- Energy creation/destruction
- NaN values
- Physical impossibilities

## Troubleshooting

### Energy Drift Detected
**Possible causes:**
1. Pipes/pumps not properly transferring enthalpy
2. Heat transfer system creating/destroying energy
3. Zone leaks not syncing internal energy
4. Temperature/energy desynchronization

**Fix:** Check that all gas transfer operations also transfer energy proportionally.

### NaN Values
**Possible causes:**
1. Division by zero (zero moles)
2. Invalid temperature calculations
3. Uninitialized nodes

**Fix:** Ensure all nodes are properly initialized with non-zero values.

### Pressure Not Equalizing
**Possible causes:**
1. Pipe conductance too low
2. Nodes not connected
3. Flow limiting too aggressive

**Fix:** Check pipe conductance values and connections.

### Temperature Not Equilibrating
**Possible causes:**
1. Pipe thermal conductance too low
2. Nodes have very different mole counts
3. External heat transfer overpowering conduction

**Fix:** Adjust `pipeThermalConductance` in HeatTransfer component.

## Expected Behavior

### Sealed System (No External Heat Transfer)
- Total energy should remain constant
- Pressure should equalize across connected nodes
- Temperature should equilibrate based on mass-weighted average

### Unsealed System (With External Heat Transfer)
- Total energy will decrease if nodes are warmer than environment
- Total energy will increase if nodes are colder than environment
- Energy change rate should match: `Σ(HeatLossCoefficient × ΔT)`

## Performance Notes

- Validation runs at specified intervals (default: 1 second)
- Minimal performance impact
- Can be disabled in production builds
- Debug GUI uses Unity's IMGUI (legacy UI)

## Next Steps

After validating system stability:
1. Implement device systems (CO₂ scrubbers, gas canisters)
2. Add atmospheric hazard detection
3. Integrate with gameplay mechanics
4. Optimize for larger networks

---

*For questions or issues, check the console for detailed error messages.*
