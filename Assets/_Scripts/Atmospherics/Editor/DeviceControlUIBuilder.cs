#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Atmospherics.UI;
using Atmospherics.Devices;

namespace Atmospherics.Editor
{
    public class DeviceControlUIBuilder : EditorWindow
    {
        private Canvas targetCanvas;
        private MonoBehaviour targetDevice;
        private DeviceControlPanel.DeviceType deviceType = DeviceControlPanel.DeviceType.CO2Scrubber;
        
        private bool createWorldSpace = false;
        private Vector2 panelSize = new Vector2(300, 400);
        private bool includeBackground = true;
        private bool includeButtons = true;
        private bool includeSliders = true;
        private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        [MenuItem("Atmospherics/Device Control UI Builder")]
        public static void ShowWindow()
        {
            GetWindow<DeviceControlUIBuilder>("Device Control UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Device Control UI Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Device Configuration", EditorStyles.boldLabel);
            targetCanvas = EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true) as Canvas;
            targetDevice = EditorGUILayout.ObjectField("Target Device", targetDevice, typeof(MonoBehaviour), true) as MonoBehaviour;
            deviceType = (DeviceControlPanel.DeviceType)EditorGUILayout.EnumPopup("Device Type", deviceType);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI Options", EditorStyles.boldLabel);

            createWorldSpace = EditorGUILayout.Toggle("World Space Canvas", createWorldSpace);
            panelSize = EditorGUILayout.Vector2Field("Panel Size", panelSize);
            panelColor = EditorGUILayout.ColorField("Panel Color", panelColor);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI Elements", EditorStyles.boldLabel);
            includeBackground = EditorGUILayout.Toggle("Background Panel", includeBackground);
            includeButtons = EditorGUILayout.Toggle("Control Buttons", includeButtons);
            includeSliders = EditorGUILayout.Toggle("Control Sliders", includeSliders);

            EditorGUILayout.Space();

            if (GUILayout.Button("Build CO₂ Scrubber Control Panel", GUILayout.Height(35)))
            {
                deviceType = DeviceControlPanel.DeviceType.CO2Scrubber;
                BuildDeviceControlPanel();
            }

            if (GUILayout.Button("Build Gas Canister Control Panel", GUILayout.Height(35)))
            {
                deviceType = DeviceControlPanel.DeviceType.GasCanister;
                BuildDeviceControlPanel();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Build Generic Device Panel", GUILayout.Height(35)))
            {
                BuildDeviceControlPanel();
            }

            EditorGUILayout.Space();
            
            if (targetDevice != null)
            {
                EditorGUILayout.HelpBox($"Will create control panel for: {targetDevice.name}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a device or create a panel first, then assign the device later.", MessageType.Info);
            }
        }

        private void BuildDeviceControlPanel()
        {
            GameObject panelObj = new GameObject($"{deviceType} Control Panel");

            Canvas canvas;
            if (createWorldSpace)
            {
                canvas = panelObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                RectTransform canvasRect = panelObj.GetComponent<RectTransform>();
                canvasRect.sizeDelta = panelSize;
                canvasRect.localScale = Vector3.one * 0.01f;

                if (targetDevice != null)
                {
                    panelObj.transform.SetParent(targetDevice.transform);
                    panelObj.transform.localPosition = Vector3.forward * 2f;
                    panelObj.transform.localRotation = Quaternion.identity;
                }
            }
            else
            {
                GameObject canvasObj;
                if (targetCanvas == null)
                {
                    canvasObj = new GameObject("Device Control Canvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();

                    CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0.5f;
                }
                else
                {
                    canvasObj = targetCanvas.gameObject;
                }

                panelObj.transform.SetParent(canvasObj.transform, false);
                if (!panelObj.GetComponent<RectTransform>()) panelObj.AddComponent<RectTransform>();
                RectTransform panelRect = panelObj.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0, 0.5f);
                panelRect.anchorMax = new Vector2(0, 0.5f);
                panelRect.anchoredPosition = new Vector2(20 + panelSize.x / 2, 0);
                panelRect.sizeDelta = panelSize;
            }

            panelObj.AddComponent<CanvasRenderer>();
            RectTransform mainRect = panelObj.GetComponent<RectTransform>();

            DeviceControlPanel controlPanel = panelObj.AddComponent<DeviceControlPanel>();
            controlPanel.targetDevice = targetDevice;
            controlPanel.deviceType = deviceType;

            if (includeBackground)
            {
                GameObject bg = CreateUIElement("Background", mainRect);
                Image bgImg = bg.AddComponent<Image>();
                bgImg.color = panelColor;
                SetFullRect(bg.GetComponent<RectTransform>());
            }

            float yOffset = -20f;
            float spacing = 30f;

            GameObject titleObj = CreateTextElement("Device Name", mainRect, deviceType.ToString().ToUpper(), 20);
            SetAnchorAndPosition(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yOffset), new Vector2(panelSize.x - 20, 30));
            TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            controlPanel.deviceNameText = titleText;
            yOffset -= spacing + 10;

            GameObject statusObj = CreateTextElement("Status Text", mainRect, "STATUS: UNKNOWN", 16);
            SetAnchorAndPosition(statusObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yOffset), new Vector2(panelSize.x - 20, 25));
            TextMeshProUGUI statusText = statusObj.GetComponent<TextMeshProUGUI>();
            statusText.alignment = TextAlignmentOptions.Center;
            controlPanel.statusText = statusText;
            yOffset -= spacing + 15;

            if (includeButtons)
            {
                GameObject powerToggleObj = CreateToggle("Power Toggle", mainRect, "Power");
                SetAnchorAndPosition(powerToggleObj.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, yOffset), new Vector2(0, 30));
                controlPanel.powerToggle = powerToggleObj.GetComponent<Toggle>();
                yOffset -= spacing + 5;
            }

            if (deviceType == DeviceControlPanel.DeviceType.CO2Scrubber)
            {
                BuildScrubberControls(mainRect, controlPanel, ref yOffset, spacing);
            }
            else if (deviceType == DeviceControlPanel.DeviceType.GasCanister)
            {
                BuildCanisterControls(mainRect, controlPanel, ref yOffset, spacing);
            }

            EditorUtility.SetDirty(panelObj);
            Selection.activeGameObject = panelObj;

            Debug.Log($"Device Control Panel created for {deviceType}!");
            EditorUtility.DisplayDialog("Success", 
                $"Device Control Panel created!\n\nDevice Type: {deviceType}\n\nAssign your device reference in the DeviceControlPanel component.", 
                "OK");
        }

        private void BuildScrubberControls(RectTransform parent, DeviceControlPanel panel, ref float yOffset, float spacing)
        {
            GameObject scrubRateLabelObj = CreateTextElement("Scrub Rate Label", parent, "SCRUB RATE", 14);
            scrubRateLabelObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            SetAnchorAndPosition(scrubRateLabelObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, yOffset), new Vector2(200, 20));
            yOffset -= 25;

            if (includeSliders)
            {
                GameObject sliderObj = CreateSlider("Scrub Rate Slider", parent, 0f, 0.1f, 0.05f);
                SetAnchorAndPosition(sliderObj.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, yOffset), new Vector2(0, 25));
                panel.scrubRateSlider = sliderObj.GetComponent<Slider>();
                yOffset -= spacing;
            }

            GameObject scrubRateTextObj = CreateTextElement("Scrub Rate Value", parent, "0.050 mol/s", 14);
            SetAnchorAndPosition(scrubRateTextObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yOffset), new Vector2(200, 20));
            TextMeshProUGUI scrubText = scrubRateTextObj.GetComponent<TextMeshProUGUI>();
            scrubText.alignment = TextAlignmentOptions.Center;
            panel.scrubRateText = scrubText;
            yOffset -= spacing;

            yOffset -= 15;

            GameObject co2TextObj = CreateTextElement("CO2 Removed", parent, "CO₂: 0.00 mol", 14);
            SetAnchorAndPosition(co2TextObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yOffset), new Vector2(200, 20));
            TextMeshProUGUI co2Text = co2TextObj.GetComponent<TextMeshProUGUI>();
            co2Text.alignment = TextAlignmentOptions.Center;
            panel.co2RemovedText = co2Text;
        }

        private void BuildCanisterControls(RectTransform parent, DeviceControlPanel panel, ref float yOffset, float spacing)
        {
            GameObject modeTextObj = CreateTextElement("Mode Text", parent, "Mode: FILL", 14);
            SetAnchorAndPosition(modeTextObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yOffset), new Vector2(200, 20));
            TextMeshProUGUI modeText = modeTextObj.GetComponent<TextMeshProUGUI>();
            modeText.alignment = TextAlignmentOptions.Center;
            panel.canisterModeText = modeText;
            yOffset -= spacing;

            if (includeButtons)
            {
                GameObject changeModeBtn = CreateButton("Change Mode Button", parent, "Change Mode");
                SetAnchorAndPosition(changeModeBtn.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, yOffset), new Vector2(0, 35));
                panel.changeModeButton = changeModeBtn.GetComponent<Button>();
                TextMeshProUGUI btnText = changeModeBtn.GetComponentInChildren<TextMeshProUGUI>();
                panel.changeModeButtonText = btnText;
                yOffset -= 40;
            }

            yOffset -= 15;

            GameObject pressureTextObj = CreateTextElement("Pressure Text", parent, "P: 0.0 kPa", 14);
            SetAnchorAndPosition(pressureTextObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yOffset), new Vector2(200, 20));
            TextMeshProUGUI pressureText = pressureTextObj.GetComponent<TextMeshProUGUI>();
            pressureText.alignment = TextAlignmentOptions.Center;
            panel.pressureText = pressureText;
            yOffset -= spacing;

            GameObject capacityTextObj = CreateTextElement("Capacity Text", parent, "Capacity: 0%", 14);
            SetAnchorAndPosition(capacityTextObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, yOffset), new Vector2(200, 20));
            TextMeshProUGUI capacityText = capacityTextObj.GetComponent<TextMeshProUGUI>();
            capacityText.alignment = TextAlignmentOptions.Center;
            panel.capacityText = capacityText;
            yOffset -= spacing + 15;

            if (includeButtons)
            {
                GameObject interactBtn = CreateButton("Interact Button", parent, "Connect");
                SetAnchorAndPosition(interactBtn.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, yOffset), new Vector2(0, 35));
                panel.interactButton = interactBtn.GetComponent<Button>();
                TextMeshProUGUI btnText = interactBtn.GetComponentInChildren<TextMeshProUGUI>();
                panel.buttonText = btnText;
            }
        }

        private GameObject CreateUIElement(string name, RectTransform parent)
        {
            GameObject obj = new GameObject(name);
            obj.AddComponent<RectTransform>();
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private GameObject CreateTextElement(string name, RectTransform parent, string text, int fontSize)
        {
            GameObject obj = CreateUIElement(name, parent);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            return obj;
        }

        private GameObject CreateButton(string name, RectTransform parent, string labelText)
        {
            GameObject buttonObj = CreateUIElement(name, parent);
            Image buttonImg = buttonObj.AddComponent<Image>();
            buttonImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            Button button = buttonObj.AddComponent<Button>();

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            button.colors = colors;

            GameObject textObj = CreateTextElement("Text", buttonObj.GetComponent<RectTransform>(), labelText, 16);
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            SetFullRect(textObj.GetComponent<RectTransform>());

            return buttonObj;
        }

        private GameObject CreateToggle(string name, RectTransform parent, string labelText)
        {
            GameObject toggleObj = CreateUIElement(name, parent);
            SetAnchorAndPosition(toggleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(panelSize.x, 30));

            GameObject background = CreateUIElement("Background", toggleObj.GetComponent<RectTransform>());
            RectTransform bgRect = background.GetComponent<RectTransform>();
            SetAnchorAndPosition(bgRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-30, 0), new Vector2(30, 30));
            Image bgImg = background.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            GameObject checkmark = CreateUIElement("Checkmark", background.GetComponent<RectTransform>());
            Image checkImg = checkmark.AddComponent<Image>();
            checkImg.color = Color.green;
            SetFullRect(checkmark.GetComponent<RectTransform>());

            GameObject label = CreateTextElement("Label", toggleObj.GetComponent<RectTransform>(), labelText, 16);
            SetAnchorAndPosition(label.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20, 0), new Vector2(50, 30));

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.targetGraphic = bgImg;
            toggle.graphic = checkImg;
            toggle.isOn = false;

            return toggleObj;
        }

        private GameObject CreateSlider(string name, RectTransform parent, float minValue, float maxValue, float value)
        {
            GameObject sliderObj = CreateUIElement(name, parent);

            GameObject background = CreateUIElement("Background", sliderObj.GetComponent<RectTransform>());
            Image bgImg = background.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            SetFullRect(background.GetComponent<RectTransform>());

            GameObject fillArea = CreateUIElement("Fill Area", sliderObj.GetComponent<RectTransform>());
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            SetAnchorAndPosition(fillAreaRect, new Vector2(0, 0.25f), new Vector2(1, 0.75f), Vector2.zero, Vector2.zero);

            GameObject fill = CreateUIElement("Fill", fillArea.GetComponent<RectTransform>());
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.8f, 1f, 1f);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject handleSlideArea = CreateUIElement("Handle Slide Area", sliderObj.GetComponent<RectTransform>());
            SetFullRect(handleSlideArea.GetComponent<RectTransform>());

            GameObject handle = CreateUIElement("Handle", handleSlideArea.GetComponent<RectTransform>());
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = value;

            return sliderObj;
        }

        private void SetAnchorAndPosition(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private void SetFullRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
