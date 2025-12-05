# UI Systems Guide

Complete guide for creating and using UI systems for the atmospheric simulation.

---

## 🎯 Quick Start

### Automated UI Build (Recommended)

1. **Menu:** `Atmospherics → UI Builder`
2. **Optional:** Assign your player reference
3. **Configure** which UI elements to include
4. **Click** "Build Complete UI"
5. **Done!** A complete canvas is created and selected

### Manual Setup

1. Create Canvas (Screen Space - Overlay)
2. Add UI elements (Panels, Images, Text)
3. Add `AtmosphericUIManager` component to Canvas
4. Assign UI element references
5. Assign player reference

---

## 📦 Components Overview

### 1. AtmosphericUIManager

**Main UI controller for player atmospheric display**

**What it does:**
- ✅ Updates health and stamina bars
- ✅ Displays atmospheric readings
- ✅ Shows status and warnings
- ✅ Color-codes all elements based on danger
- ✅ Flashes warning panels

**Key Properties:**

```csharp
// Player reference
playerNeeds;                    // PlayerAtmosphericNeeds to monitor

// Health & Stamina
healthBarFill;                  // Image (Type: Filled)
staminaBarFill;                 // Image (Type: Filled)
healthText;                     // TextMeshProUGUI
staminaText;                    // TextMeshProUGUI

// Atmospheric readings
zoneNameText;                   // TextMeshProUGUI
pressureText;                   // TextMeshProUGUI
temperatureText;                // TextMeshProUGUI
oxygenText;                     // TextMeshProUGUI
co2Text;                        // TextMeshProUGUI
nitrogenText;                   // TextMeshProUGUI

// Status
statusText;                     // TextMeshProUGUI
statusPanel;                    // GameObject

// Warnings
warningPanel;                   // GameObject
warningText;                    // TextMeshProUGUI
warningBackground;              // Image (for flashing)

// Hazard indicator
hazardIndicator;                // Image (color-coded)
hazardLevelText;                // TextMeshProUGUI

// Settings
updateInterval = 0.1f;          // How often to refresh
```

**How to use:**

```csharp
// Automatic - just assign references in Inspector
// Or via code:
AtmosphericUIManager uiManager = GetComponent<AtmosphericUIManager>();
uiManager.SetPlayer(myPlayerNeeds);
```

**Color coding:**
- **Green** = Safe/healthy
- **Yellow** = Caution
- **Orange** = Warning
- **Red** = Danger/critical

---

### 2. DeviceControlPanel

**UI panel for controlling devices (scrubbers, canisters)**

**What it does:**
- ✅ Displays device status
- ✅ Power toggle control
- ✅ Scrubber rate adjustment
- ✅ Canister mode switching
- ✅ Connect/disconnect buttons

**Key Properties:**

```csharp
targetDevice;                   // The device (CO2Scrubber or GasCanister)
deviceType;                     // DeviceType enum

// Common UI
deviceNameText;                 // TextMeshProUGUI
statusText;                     // TextMeshProUGUI
powerToggle;                    // Toggle
interactButton;                 // Button

// CO2 Scrubber specific
scrubRateText;                  // TextMeshProUGUI
scrubRateSlider;                // Slider
co2RemovedText;                 // TextMeshProUGUI

// Gas Canister specific
canisterModeText;               // TextMeshProUGUI
pressureText;                   // TextMeshProUGUI
capacityText;                   // TextMeshProUGUI
changeModeButton;               // Button

updateInterval = 0.2f;
```

**How to use:**

```csharp
DeviceControlPanel panel = GetComponent<DeviceControlPanel>();
panel.SetDevice(myScrubber, DeviceControlPanel.DeviceType.CO2Scrubber);
```

**Supported devices:**
- CO₂ Scrubber
- Gas Canister

---

### 3. SimpleStatusDisplay

**Minimalist in-world or screen-space status display**

**What it does:**
- ✅ Shows node atmospheric data
- ✅ Can be world-space (floating above nodes)
- ✅ Can be screen-space (fixed UI)
- ✅ Color-coded hazard levels
- ✅ Configurable fields

**Key Properties:**

```csharp
targetNode;                     // VolumeNode to monitor
targetHazards;                  // AtmosphericHazards (optional)

displayText;                    // TextMeshProUGUI
showInWorldSpace = true;        // World vs screen space

// What to show
showPressure = true;
showTemperature = true;
showOxygen = true;
showCO2 = true;
showHazardLevel = true;

updateInterval = 0.5f;
```

**How to use:**

```csharp
// Attach to a node or create separate GameObject
SimpleStatusDisplay display = gameObject.AddComponent<SimpleStatusDisplay>();
display.targetNode = myNode;
display.showInWorldSpace = true;
```

**Example output:**

```
Test Room
P: 101.3 kPa
T: 20.0°C
O₂: 21.0%
CO₂: 0.04%
Safe
```

---

## 🛠️ UI Builder Tool

**Menu:** `Atmospherics → UI Builder`

**What it creates:**
- Complete Canvas with CanvasScaler (1920x1080 reference)
- Health and stamina bars (optional)
- Atmospheric readout panel (optional)
- Warning panel with flashing (optional)
- Hazard level indicator (optional)
- All elements properly linked to `AtmosphericUIManager`

**Options:**
- **Use TextMeshPro** - Recommended for better text quality
- **Health & Stamina Bars** - Player vitals display
- **Atmospheric Readout** - Zone name, P, T, gas percentages
- **Warning Panel** - Flashing critical alerts
- **Hazard Indicator** - Color-coded safety level

**Result:**
- One-click complete UI setup
- All references auto-assigned
- Production-ready layout
- Fully customizable afterward

---

## 🎨 UI Layouts

### Layout 1: Minimal HUD (default)

```
┌─────────────────────────────────────────────┐
│  HEALTH ████████░░      [Atmospheric Data]  │
│  STAMINA ██████░░░      Zone: Test Room     │
│                         P: 101.3 kPa        │
│                         T: 20°C             │
│                         O₂: 21.0%           │
│                                             │
│                                             │
│                                             │
│            [Hazard: SAFE]                   │
└─────────────────────────────────────────────┘
```

### Layout 2: With Warnings

```
┌─────────────────────────────────────────────┐
│  ╔════════════════════════════════════════╗ │
│  ║    ⚠ OXYGEN CRITICAL ⚠                ║ │
│  ╚════════════════════════════════════════╝ │
│                                             │
│  HEALTH ██░░░░░░░░      [Atmospheric Data]  │
│  STAMINA ░░░░░░░░░░     Zone: Airlock       │
│                         P: 45.2 kPa         │
│                         T: 18°C             │
│                         O₂: 8.3%            │
│                                             │
│            [Hazard: CRITICAL]               │
└─────────────────────────────────────────────┘
```

### Layout 3: Device Control Panel

```
┌────────────────────┐
│  CO₂ SCRUBBER      │
│  Status: ON        │
│                    │
│  ☑ Power           │
│                    │
│  Rate: 0.05 mol/s  │
│  ▬▬▬▬▬▬▬▬▬▬        │
│                    │
│  CO₂: 12.5 mol     │
└────────────────────┘
```

---

## 💡 Example Setups

### Example 1: Complete Player HUD

**Goal:** Full player UI with all elements

**Steps:**
1. Menu: `Atmospherics → UI Builder`
2. Assign player reference
3. Enable all options
4. Click "Build Complete UI"
5. Customize colors/positions as needed

**Result:** Production-ready player HUD

---

### Example 2: World-Space Node Displays

**Goal:** Floating status displays above each atmospheric zone

**Steps:**
1. Select each VolumeNode
2. Right-click → UI → Text - TextMeshPro
3. Add `SimpleStatusDisplay` component
4. Set `showInWorldSpace = true`
5. Target node auto-detected

**Result:** In-world floating atmospheric readouts

---

### Example 3: Device Control Room

**Goal:** Wall-mounted panels to control life support

**Steps:**
1. Create world-space Canvas near each device
2. Build UI panel with buttons/sliders
3. Add `DeviceControlPanel` component
4. Assign target device
5. Set device type

**Result:** Interactive control panels

---

## 🎯 Customization

### Custom Colors

```csharp
AtmosphericUIManager uiManager = GetComponent<AtmosphericUIManager>();

// Change color scheme
uiManager.healthyColor = Color.cyan;
uiManager.cautionColor = Color.magenta;
uiManager.dangerColor = new Color(1f, 0.2f, 0f);
```

---

### Custom Update Rate

```csharp
// Slower updates for performance
uiManager.updateInterval = 0.5f;  // Every 0.5 seconds

// Faster updates for precision
uiManager.updateInterval = 0.05f; // 20 times per second
```

---

### Custom Warning Messages

Create custom script:

```csharp
using UnityEngine;
using TMPro;
using Atmospherics.Player;

public class CustomWarningText : MonoBehaviour
{
    public PlayerAtmosphericNeeds player;
    public TextMeshProUGUI warningText;

    private void Update()
    {
        if (player.isSuffocating)
        {
            warningText.text = "GET TO SAFETY!";
        }
        else if (player.currentHealth < 25f)
        {
            warningText.text = "CRITICAL CONDITION!";
        }
        else if (player.currentStamina < 10f)
        {
            warningText.text = "EXHAUSTED!";
        }
        else
        {
            warningText.text = "";
        }
    }
}
```

---

### Add Custom Readouts

```csharp
// Extend AtmosphericUIManager
public class ExtendedUIManager : AtmosphericUIManager
{
    public TextMeshProUGUI hydrogenText;
    public TextMeshProUGUI methaneText;

    protected override void UpdateAtmosphericReadout()
    {
        base.UpdateAtmosphericReadout();

        // Add custom gases
        if (hydrogenText != null)
        {
            float h2Percent = GetGasPercent("H2");
            hydrogenText.text = $"H₂: {h2Percent:F2}%";
        }

        if (methaneText != null)
        {
            float ch4Percent = GetGasPercent("CH4");
            methaneText.text = $"CH₄: {ch4Percent:F2}%";
        }
    }

    private float GetGasPercent(string gasName)
    {
        if (playerNeeds == null || playerNeeds.currentNode == null)
            return 0f;

        var mix = playerNeeds.currentNode.Mixture;
        float total = mix.TotalMoles();
        
        if (total <= 0f || !mix.Moles.ContainsKey(gasName))
            return 0f;

        return (mix.Moles[gasName] / total) * 100f;
    }
}
```

---

## 🎨 Visual Effects

### Pulsing Health Bar (Low Health)

```csharp
using UnityEngine;
using UnityEngine.UI;

public class PulsingHealthBar : MonoBehaviour
{
    public Image healthBar;
    public float pulseSpeed = 2f;

    private void Update()
    {
        if (healthBar.fillAmount < 0.3f)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
            healthBar.color = Color.Lerp(Color.red, Color.white, pulse);
        }
    }
}
```

---

### Screen Vignette (Suffocation)

```csharp
using UnityEngine;
using UnityEngine.UI;
using Atmospherics.Player;

public class SuffocationVignette : MonoBehaviour
{
    public PlayerAtmosphericNeeds player;
    public Image vignetteImage;
    public float fadeSpeed = 2f;

    private void Update()
    {
        float targetAlpha = player.isSuffocating ? 0.6f : 0f;
        
        Color color = vignetteImage.color;
        color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);
        vignetteImage.color = color;
    }
}
```

---

### Temperature Screen Tint

```csharp
using UnityEngine;
using UnityEngine.UI;
using Atmospherics.Player;

public class TemperatureScreenTint : MonoBehaviour
{
    public PlayerAtmosphericNeeds player;
    public Image tintOverlay;
    
    private void Update()
    {
        var readings = player.GetCurrentReadings();
        
        if (readings.temperature < 250f)
        {
            tintOverlay.color = new Color(0.5f, 0.7f, 1f, 0.3f); // Blue tint
        }
        else if (readings.temperature > 340f)
        {
            tintOverlay.color = new Color(1f, 0.5f, 0.2f, 0.3f); // Orange tint
        }
        else
        {
            tintOverlay.color = new Color(1f, 1f, 1f, 0f); // Clear
        }
    }
}
```

---

## 📱 Responsive Design

The UI Builder creates responsive layouts using:

```csharp
CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
scaler.referenceResolution = new Vector2(1920, 1080);
```

**This ensures:**
- UI scales properly on different resolutions
- Layout remains consistent
- Text stays readable

**To customize:**
- Change reference resolution for different aspect ratios
- Adjust match width/height slider
- Use anchor presets for responsive positioning

---

## 🐛 Troubleshooting

### UI Not Updating

**Check:**
1. Player reference assigned in `AtmosphericUIManager`
2. UI element references properly assigned
3. Update interval not too high
4. Canvas is active

**Fix:**
```csharp
Debug.Log($"Player: {uiManager.playerNeeds != null}");
Debug.Log($"Health bar: {uiManager.healthBarFill != null}");
```

---

### Text Not Showing

**Possible causes:**
1. TextMeshPro not imported
2. Font asset missing
3. Text color is transparent
4. Canvas not rendering

**Fix:**
- Window → TextMeshPro → Import TMP Essential Resources
- Check text color alpha is > 0
- Verify Canvas settings

---

### UI Elements Misaligned

**Check:**
- Canvas Scaler settings
- Anchor presets
- RectTransform positions

**Fix:**
- Reset RectTransform
- Use anchor presets (top-left, center, etc.)
- Check Canvas render mode

---

### Warning Panel Not Flashing

**Check:**
1. `warningPanel` GameObject assigned
2. `warningBackground` Image assigned
3. `warningFlashSpeed` > 0

**Fix:**
```csharp
uiManager.warningFlashSpeed = 3f; // Default value
```

---

## ⚡ Performance Tips

### Optimize Update Frequency

```csharp
// Don't need ultra-fast UI updates
uiManager.updateInterval = 0.2f; // 5 updates/sec is plenty
```

---

### Pool UI Elements

For dynamic UI (many panels):

```csharp
public class UIPool : MonoBehaviour
{
    public GameObject panelPrefab;
    private List<GameObject> pool = new List<GameObject>();

    public GameObject GetPanel()
    {
        foreach (var panel in pool)
        {
            if (!panel.activeSelf)
            {
                panel.SetActive(true);
                return panel;
            }
        }

        GameObject newPanel = Instantiate(panelPrefab, transform);
        pool.Add(newPanel);
        return newPanel;
    }

    public void ReturnPanel(GameObject panel)
    {
        panel.SetActive(false);
    }
}
```

---

### Batch UI Updates

```csharp
// Update all text at once instead of individually
private void UpdateAllText()
{
    var readings = playerNeeds.GetCurrentReadings();
    
    string[] updates = new string[]
    {
        $"P: {readings.pressure:F1} kPa",
        $"T: {readings.TemperatureCelsius:F1}°C",
        $"O₂: {readings.oxygenPercent:F1}%"
    };

    pressureText.text = updates[0];
    temperatureText.text = updates[1];
    oxygenText.text = updates[2];
}
```

---

## 🎯 Best Practices

1. **Use TextMeshPro** for all text - better quality and performance
2. **Anchor UI elements properly** - ensures responsive layouts
3. **Color-code everything** - instant visual feedback
4. **Keep update rates reasonable** - 5-10 updates/sec is enough
5. **Use image fills** for bars - better than scaling
6. **Test on different resolutions** - ensure readability
7. **Provide visual AND text feedback** - accessibility
8. **Use sprites for icons** - more recognizable than text

---

## 🚀 Next Steps

After setting up UI:

1. **Add audio** - Button clicks, warning alarms
2. **Add animations** - Smooth transitions, popup panels
3. **Add tooltips** - Hover for more info
4. **Add tutorials** - First-time user guidance
5. **Add accessibility** - Color-blind modes, font size options

---

**Your atmospheric UI system is ready for production!** 🎨
