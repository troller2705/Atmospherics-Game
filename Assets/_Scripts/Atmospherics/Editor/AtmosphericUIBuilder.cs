#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Atmospherics.UI;
using Atmospherics.Player;
using UnityEngine.ProBuilder;

namespace Atmospherics.Editor
{
    public class AtmosphericUIBuilder : EditorWindow
    {
        private PlayerAtmosphericNeeds playerReference;
        private bool useTextMeshPro = true;
        private bool includeHealthBars = true;
        private bool includeAtmosphericReadout = true;
        private bool includeWarningPanel = true;
        private bool includeHazardIndicator = true;

        [MenuItem("Atmospherics/UI Builder")]
        public static void ShowWindow()
        {
            GetWindow<AtmosphericUIBuilder>("Atmospheric UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Atmospheric UI Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            playerReference = EditorGUILayout.ObjectField("Player Reference", playerReference, typeof(PlayerAtmosphericNeeds), true) as PlayerAtmosphericNeeds;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI Options", EditorStyles.boldLabel);

            useTextMeshPro = EditorGUILayout.Toggle("Use TextMeshPro", useTextMeshPro);
            includeHealthBars = EditorGUILayout.Toggle("Health & Stamina Bars", includeHealthBars);
            includeAtmosphericReadout = EditorGUILayout.Toggle("Atmospheric Readout", includeAtmosphericReadout);
            includeWarningPanel = EditorGUILayout.Toggle("Warning Panel", includeWarningPanel);
            includeHazardIndicator = EditorGUILayout.Toggle("Hazard Indicator", includeHazardIndicator);

            EditorGUILayout.Space();

            if (GUILayout.Button("Build Complete UI", GUILayout.Height(40)))
            {
                BuildCompleteUI();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This will create a complete atmospheric UI canvas with all selected elements.", MessageType.Info);
        }

        private void BuildCompleteUI()
        {
            GameObject canvasObj = new GameObject("Atmospheric UI Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            AtmosphericUIManager uiManager = canvasObj.AddComponent<AtmosphericUIManager>();
            uiManager.playerNeeds = playerReference;

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();

            if (includeHealthBars)
            {
                BuildHealthStaminaBars(canvasRect, uiManager);
            }

            if (includeAtmosphericReadout)
            {
                BuildAtmosphericReadout(canvasRect, uiManager);
            }

            if (includeWarningPanel)
            {
                BuildWarningPanel(canvasRect, uiManager);
            }

            if (includeHazardIndicator)
            {
                BuildHazardIndicator(canvasRect, uiManager);
            }

            EditorUtility.SetDirty(canvasObj);
            Selection.activeGameObject = canvasObj;

            Debug.Log("Atmospheric UI Canvas created successfully!");
            EditorUtility.DisplayDialog("Success", "Atmospheric UI Canvas created!\n\nDon't forget to assign your player reference in the AtmosphericUIManager component.", "OK");
        }

        private void BuildHealthStaminaBars(RectTransform parent, AtmosphericUIManager manager)
        {
            GameObject healthBarObj = CreateUIElement("Health Bar", parent);
            RectTransform healthRect = healthBarObj.GetComponent<RectTransform>();
            healthRect.pivot = new Vector2(0, 1);
            SetAnchorAndPosition(healthRect, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, 0), new Vector2(320, 60));

            GameObject healthBg = CreateUIElement("Background", healthRect);
            Image healthBgImg = healthBg.AddComponent<Image>();
            healthBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            SetFullRect(healthBg.GetComponent<RectTransform>());

            GameObject healthLabel = CreateTextElement("Label", healthRect, "HEALTH", 16);
            healthLabel.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            SetAnchorAndPosition(healthLabel.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(80, 30));

            GameObject healthFillObj = CreateUIElement("Fill Container", healthRect);
            SetAnchorAndPosition(healthFillObj.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(90, 0), new Vector2(-10, 20));

            GameObject healthFillBg = CreateUIElement("Fill Background", healthFillObj.GetComponent<RectTransform>());
            Image fillBgImg = healthFillBg.AddComponent<Image>();
            fillBgImg.color = new Color(0.3f, 0.1f, 0.1f, 1f);
            SetFullRect(healthFillBg.GetComponent<RectTransform>());

            GameObject healthFill = CreateUIElement("Fill", healthFillObj.GetComponent<RectTransform>());
            Image fillImg = healthFill.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.color = Color.green;
            SetFullRect(healthFill.GetComponent<RectTransform>());
            manager.healthBarFill = fillImg;

            GameObject healthTextObj = CreateTextElement("Text", healthFillObj.GetComponent<RectTransform>(), "100/100", 14);
            SetFullRect(healthTextObj.GetComponent<RectTransform>());
            manager.healthText = healthTextObj.GetComponent<TextMeshProUGUI>();

            GameObject staminaBarObj = CreateUIElement("Stamina Bar", parent);
            RectTransform staminaRect = staminaBarObj.GetComponent<RectTransform>();
            staminaRect.pivot = new Vector2(0, 1);
            SetAnchorAndPosition(staminaRect, new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -60), new Vector2(320, 60));

            GameObject staminaBg = CreateUIElement("Background", staminaRect);
            Image staminaBgImg = staminaBg.AddComponent<Image>();
            staminaBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            SetFullRect(staminaBg.GetComponent<RectTransform>());

            GameObject staminaLabel = CreateTextElement("Label", staminaRect, "STAMINA", 16);
            staminaLabel.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            SetAnchorAndPosition(staminaLabel.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(8, 0), new Vector2(80, 30));

            GameObject staminaFillObj = CreateUIElement("Fill Container", staminaRect);
            SetAnchorAndPosition(staminaFillObj.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(90, 0), new Vector2(-10, 20));

            GameObject staminaFillBg = CreateUIElement("Fill Background", staminaFillObj.GetComponent<RectTransform>());
            Image staminaFillBgImg = staminaFillBg.AddComponent<Image>();
            staminaFillBgImg.color = new Color(0.1f, 0.2f, 0.3f, 1f);
            SetFullRect(staminaFillBg.GetComponent<RectTransform>());

            GameObject staminaFill = CreateUIElement("Fill", staminaFillObj.GetComponent<RectTransform>());
            Image staminaFillImg = staminaFill.AddComponent<Image>();
            staminaFillImg.type = Image.Type.Filled;
            staminaFillImg.fillMethod = Image.FillMethod.Horizontal;
            staminaFillImg.color = new Color(0.3f, 0.8f, 1f);
            SetFullRect(staminaFill.GetComponent<RectTransform>());
            manager.staminaBarFill = staminaFillImg;

            GameObject staminaTextObj = CreateTextElement("Text", staminaFillObj.GetComponent<RectTransform>(), "100/100", 14);
            SetFullRect(staminaTextObj.GetComponent<RectTransform>());
            manager.staminaText = staminaTextObj.GetComponent<TextMeshProUGUI>();
        }

        private void BuildAtmosphericReadout(RectTransform parent, AtmosphericUIManager manager)
        {
            GameObject readoutObj = CreateUIElement("Atmospheric Readout", parent);
            RectTransform readoutRect = readoutObj.GetComponent<RectTransform>();
            readoutRect.pivot = new Vector2(1, 1);
            SetAnchorAndPosition(readoutRect, new Vector2(1, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(350, 200));

            GameObject bg = CreateUIElement("Background", readoutRect);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            SetFullRect(bg.GetComponent<RectTransform>());

            GameObject zoneText = CreateTextElement("Zone Name", readoutRect, "ZONE NAME", 18);
            SetAnchorAndPosition(zoneText.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(300, 30));
            manager.zoneNameText = zoneText.GetComponent<TextMeshProUGUI>();

            float yOffset = -50f;
            float spacing = 25f;

            GameObject pressureText = CreateTextElement("Pressure", readoutRect, "P: 101.3 kPa", 14);
            SetAnchorAndPosition(pressureText.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, yOffset), new Vector2(200, 20));
            manager.pressureText = pressureText.GetComponent<TextMeshProUGUI>();
            yOffset -= spacing;

            GameObject tempText = CreateTextElement("Temperature", readoutRect, "T: 20.0°C", 14);
            SetAnchorAndPosition(tempText.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, yOffset), new Vector2(200, 20));
            manager.temperatureText = tempText.GetComponent<TextMeshProUGUI>();
            yOffset -= spacing;

            yOffset -= 10f;

            GameObject o2Text = CreateTextElement("Oxygen", readoutRect, "O₂: 21.0%", 14);
            SetAnchorAndPosition(o2Text.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, yOffset), new Vector2(200, 20));
            manager.oxygenText = o2Text.GetComponent<TextMeshProUGUI>();
            yOffset -= spacing;

            GameObject co2Text = CreateTextElement("CO2", readoutRect, "CO₂: 0.04%", 14);
            SetAnchorAndPosition(co2Text.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, yOffset), new Vector2(200, 20));
            manager.co2Text = co2Text.GetComponent<TextMeshProUGUI>();
            yOffset -= spacing;

            GameObject n2Text = CreateTextElement("Nitrogen", readoutRect, "N₂: 78.9%", 14);
            SetAnchorAndPosition(n2Text.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, yOffset), new Vector2(200, 20));
            manager.nitrogenText = n2Text.GetComponent<TextMeshProUGUI>();
        }

        private void BuildWarningPanel(RectTransform parent, AtmosphericUIManager manager)
        {
            GameObject warningObj = CreateUIElement("Warning Panel", parent);
            RectTransform warningRect = warningObj.GetComponent<RectTransform>();
            warningRect.pivot = new Vector2(0.5f, 1);
            SetAnchorAndPosition(warningRect, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(600, 80));

            GameObject bg = CreateUIElement("Background", warningRect);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(1f, 0f, 0f, 0.5f);
            SetFullRect(bg.GetComponent<RectTransform>());
            manager.warningBackground = bgImg;

            GameObject warningText = CreateTextElement("Warning Text", warningRect, "⚠ WARNING ⚠", 32);
            SetFullRect(warningText.GetComponent<RectTransform>());
            TextMeshProUGUI tmpText = warningText.GetComponent<TextMeshProUGUI>();
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            manager.warningText = tmpText;

            warningObj.SetActive(false);
            manager.warningPanel = warningObj;
        }

        private void BuildHazardIndicator(RectTransform parent, AtmosphericUIManager manager)
        {
            GameObject hazardObj = CreateUIElement("Hazard Indicator", parent);
            RectTransform hazardRect = hazardObj.GetComponent<RectTransform>();
            hazardRect.pivot = new Vector2(0.5f, 0);
            SetAnchorAndPosition(hazardRect, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(200, 60));

            GameObject bg = CreateUIElement("Background", hazardRect);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            SetFullRect(bg.GetComponent<RectTransform>());

            GameObject indicator = CreateUIElement("Indicator", hazardRect);
            Image indicatorImg = indicator.AddComponent<Image>();
            indicatorImg.color = Color.green;
            SetAnchorAndPosition(indicator.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(40, 40));
            manager.hazardIndicator = indicatorImg;

            GameObject hazardText = CreateTextElement("Hazard Text", hazardRect, "SAFE", 16);
            SetAnchorAndPosition(hazardText.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(60, 0), new Vector2(-10, 30));
            TextMeshProUGUI tmpText = hazardText.GetComponent<TextMeshProUGUI>();
            tmpText.fontStyle = FontStyles.Bold;
            manager.hazardLevelText = tmpText;
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
            tmp.alignment = TextAlignmentOptions.Left;
            return obj;
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
