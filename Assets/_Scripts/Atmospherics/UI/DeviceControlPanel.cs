using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Atmospherics.Core;
using Atmospherics.Devices;

namespace Atmospherics.UI
{
    public class DeviceControlPanel : MonoBehaviour
    {
        [Header("Device Reference")]
        public MonoBehaviour targetDevice;
        public DeviceType deviceType;

        [Header("UI Elements")]
        public TextMeshProUGUI deviceNameText;
        public TextMeshProUGUI statusText;
        public Toggle powerToggle;
        public Button interactButton;
        public TextMeshProUGUI buttonText;

        [Header("CO2 Scrubber Elements")]
        public TextMeshProUGUI scrubRateText;
        public Slider scrubRateSlider;
        public TextMeshProUGUI co2RemovedText;

        [Header("Gas Canister Elements")]
        public TextMeshProUGUI canisterModeText;
        public TextMeshProUGUI pressureText;
        public TextMeshProUGUI capacityText;
        public Button changeModeButton;
        public TextMeshProUGUI changeModeButtonText;

        [Header("Settings")]
        public float updateInterval = 0.2f;

        public enum DeviceType
        {
            CO2Scrubber,
            GasCanister
        }

        private float updateTimer = 0f;
        private CO2Scrubber scrubber;
        private GasCanister canister;

        private void Start()
        {
            InitializeDevice();
            SetupListeners();
            UpdateUI();
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                UpdateUI();
                updateTimer = 0f;
            }
        }

        private void InitializeDevice()
        {
            if (targetDevice == null) return;

            switch (deviceType)
            {
                case DeviceType.CO2Scrubber:
                    scrubber = targetDevice as CO2Scrubber;
                    break;
                case DeviceType.GasCanister:
                    canister = targetDevice as GasCanister;
                    break;
            }
        }

        private void SetupListeners()
        {
            if (powerToggle != null)
            {
                powerToggle.onValueChanged.AddListener(OnPowerToggled);
            }

            if (interactButton != null)
            {
                interactButton.onClick.AddListener(OnInteractClicked);
            }

            if (scrubRateSlider != null)
            {
                scrubRateSlider.onValueChanged.AddListener(OnScrubRateChanged);
            }

            if (changeModeButton != null)
            {
                changeModeButton.onClick.AddListener(OnChangeModeClicked);
            }
        }

        private void UpdateUI()
        {
            switch (deviceType)
            {
                case DeviceType.CO2Scrubber:
                    UpdateScrubberUI();
                    break;
                case DeviceType.GasCanister:
                    UpdateCanisterUI();
                    break;
            }
        }

        private void UpdateScrubberUI()
        {
            if (scrubber == null) return;

            if (deviceNameText != null)
            {
                deviceNameText.text = "CO₂ SCRUBBER";
            }

            if (statusText != null)
            {
                if (!scrubber.isPowered)
                    statusText.text = "OFFLINE";
                else if (!scrubber.isOperational)
                    statusText.text = "NO TARGET";
                else
                    statusText.text = "OPERATIONAL";

                statusText.color = scrubber.isOperational ? Color.green : Color.gray;
            }

            if (powerToggle != null && powerToggle.isOn != scrubber.isPowered)
            {
                powerToggle.SetIsOnWithoutNotify(scrubber.isPowered);
            }

            if (scrubRateText != null)
            {
                scrubRateText.text = $"Rate: {scrubber.scrubRate:F2} mol/s";
            }

            if (scrubRateSlider != null)
            {
                scrubRateSlider.SetValueWithoutNotify(scrubber.scrubRate);
            }

            if (co2RemovedText != null && scrubber.targetNode != null)
            {
                float co2Moles = 0f;
                if (scrubber.targetNode.Mixture != null && scrubber.targetNode.Mixture.Moles.ContainsKey("CO2"))
                {
                    co2Moles = scrubber.targetNode.Mixture.Moles["CO2"];
                }
                co2RemovedText.text = $"CO₂: {co2Moles:F2} mol";
            }
        }

        private void UpdateCanisterUI()
        {
            if (canister == null) return;

            if (deviceNameText != null)
            {
                deviceNameText.text = "GAS CANISTER";
            }

            if (statusText != null)
            {
                if (canister.connectedNode == null)
                    statusText.text = "DISCONNECTED";
                else
                    statusText.text = "CONNECTED";

                statusText.color = canister.connectedNode != null ? Color.green : Color.gray;
            }

            if (canisterModeText != null)
            {
                canisterModeText.text = $"Mode: {canister.mode}";
            }

            if (pressureText != null)
            {
                float pressure = canister.storedGas.GetPressure();
                pressureText.text = $"P: {pressure:F1} kPa";

                if (pressure > canister.maxPressure * 0.9f)
                    pressureText.color = Color.red;
                else if (pressure > canister.maxPressure * 0.7f)
                    pressureText.color = Color.yellow;
                else
                    pressureText.color = Color.white;
            }

            if (capacityText != null)
            {
                float totalMoles = canister.storedGas.TotalMoles();
                float maxMoles = canister.maxPressure * canister.storedGas.Volume / (8.314f * canister.storedGas.Temperature);
                float percent = maxMoles > 0 ? (totalMoles / maxMoles) * 100f : 0f;
                capacityText.text = $"Capacity: {percent:F0}%";
            }

            if (changeModeButtonText != null)
            {
                changeModeButtonText.text = "Change Mode";
            }

            if (interactButton != null && buttonText != null)
            {
                if (canister.connectedNode == null)
                {
                    buttonText.text = "Connect";
                }
                else
                {
                    buttonText.text = "Disconnect";
                }
            }
        }

        private void OnPowerToggled(bool isOn)
        {
            if (scrubber != null)
            {
                scrubber.isPowered = isOn;
            }
        }

        private void OnScrubRateChanged(float value)
        {
            if (scrubber != null)
            {
                scrubber.scrubRate = value;
            }
        }

        private void OnInteractClicked()
        {
            if (canister != null)
            {
                if (canister.connectedNode == null)
                {
                    AtmosphericNode nearestNode = FindNearestNode();
                    if (nearestNode != null)
                    {
                        canister.ConnectToNode(nearestNode);
                    }
                }
                else
                {
                    canister.Disconnect();
                }
            }
        }

        private void OnChangeModeClicked()
        {
            if (canister != null)
            {
                int nextMode = ((int)canister.mode + 1) % 3;
                canister.mode = (GasCanister.TransferMode)nextMode;
            }
        }

        private AtmosphericNode FindNearestNode()
        {
            AtmosphericNode[] allNodes = FindObjectsByType<AtmosphericNode>(FindObjectsSortMode.None);
            AtmosphericNode nearest = null;
            float minDistance = 5f;

            foreach (AtmosphericNode node in allNodes)
            {
                float distance = Vector3.Distance(canister.transform.position, node.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = node;
                }
            }

            return nearest;
        }

        public void SetDevice(MonoBehaviour device, DeviceType type)
        {
            targetDevice = device;
            deviceType = type;
            InitializeDevice();
            UpdateUI();
        }
    }
}
