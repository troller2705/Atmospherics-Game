using UnityEngine;
using Atmospherics.Core;
using System.Collections.Generic;

namespace Atmospherics.Devices
{
    public class GasCanister : MonoBehaviour
    {
        [Header("Canister Properties")]
        public string canisterName = "Gas Canister";
        public float maxPressure = 5000f;
        public float volume = 0.1f;

        [Header("Gas Contents")]
        public GasMixture storedGas;

        [Header("Connection")]
        public AtmosphericNode connectedNode;
        public bool isConnected = false;
        public float transferRate = 1f;

        [Header("Transfer Mode")]
        public TransferMode mode = TransferMode.Manual;
        public enum TransferMode
        {
            Manual,
            Fill,
            Empty,
            Equalize
        }

        [Header("Status")]
        public float totalMolesFilled = 0f;
        public float totalMolesEmptied = 0f;

        private const float SPECIFIC_HEAT_CP = 29.0f;

        private void Awake()
        {
            if (storedGas == null)
            {
                storedGas = new GasMixture(101.3f, 293f, volume);
            }
        }

        public void UpdateCanister(float dt)
        {
            if (!isConnected || connectedNode == null) return;

            switch (mode)
            {
                case TransferMode.Fill:
                    FillFromNode(dt);
                    break;
                case TransferMode.Empty:
                    EmptyToNode(dt);
                    break;
                case TransferMode.Equalize:
                    Equalize(dt);
                    break;
            }
        }

        private void FillFromNode(float dt)
        {
            if (connectedNode.Mixture == null) return;

            float canisterPressure = storedGas.GetPressure();
            if (canisterPressure >= maxPressure) return;

            float nodePressure = connectedNode.Mixture.GetPressure();
            if (nodePressure <= canisterPressure) return;

            float pressureDiff = nodePressure - canisterPressure;
            float molesToTransfer = Mathf.Min(transferRate * dt, pressureDiff * 0.01f);

            TransferGas(connectedNode, this, molesToTransfer);
            totalMolesFilled += molesToTransfer;
        }

        private void EmptyToNode(float dt)
        {
            if (connectedNode.Mixture == null) return;

            float canisterMoles = storedGas.TotalMoles();
            if (canisterMoles <= 0.001f) return;

            float molesToTransfer = Mathf.Min(transferRate * dt, canisterMoles * 0.1f);

            TransferGas(this, connectedNode, molesToTransfer);
            totalMolesEmptied += molesToTransfer;
        }

        private void Equalize(float dt)
        {
            if (connectedNode.Mixture == null) return;

            float canisterPressure = storedGas.GetPressure();
            float nodePressure = connectedNode.Mixture.GetPressure();
            float pressureDiff = canisterPressure - nodePressure;

            if (Mathf.Abs(pressureDiff) < 0.1f) return;

            float molesToTransfer = Mathf.Abs(pressureDiff) * 0.01f * dt;

            if (pressureDiff > 0)
            {
                TransferGas(this, connectedNode, molesToTransfer);
            }
            else
            {
                TransferGas(connectedNode, this, molesToTransfer);
            }
        }

        private void TransferGas(object source, object destination, float moles)
        {
            GasMixture srcMix = null;
            GasMixture dstMix = null;
            AtmosphericNode srcNode = null;
            AtmosphericNode dstNode = null;

            if (source is GasCanister srcCanister)
            {
                srcMix = srcCanister.storedGas;
            }
            else if (source is AtmosphericNode srcVNode)
            {
                srcMix = srcVNode.Mixture;
                srcNode = srcVNode;
            }

            if (destination is GasCanister dstCanister)
            {
                dstMix = dstCanister.storedGas;
            }
            else if (destination is AtmosphericNode dstVNode)
            {
                dstMix = dstVNode.Mixture;
                dstNode = dstVNode;
            }

            if (srcMix == null || dstMix == null) return;

            float srcTotal = srcMix.TotalMoles();
            if (srcTotal <= 0f || moles <= 0f) return;

            moles = Mathf.Min(moles, srcTotal * 0.5f);

            var gasKeys = new List<string>(srcMix.Moles.Keys);
            foreach (var gas in gasKeys)
            {
                float ratio = srcMix.Moles[gas] / srcTotal;
                float moved = ratio * moles;

                if (moved < 1e-12f) continue;

                srcMix.Moles[gas] -= moved;

                if (!dstMix.Moles.ContainsKey(gas))
                    dstMix.Moles[gas] = 0f;

                dstMix.Moles[gas] += moved;
            }

            float enthalpyTransferred = moles * SPECIFIC_HEAT_CP * srcMix.Temperature;

            if (srcNode != null)
            {
                srcNode.Thermal.InternalEnergyJ -= enthalpyTransferred;
                float newSrcMoles = srcMix.TotalMoles();
                if (newSrcMoles > 0f)
                {
                    srcMix.Temperature = srcNode.Thermal.InternalEnergyJ / (newSrcMoles * SPECIFIC_HEAT_CP);
                    srcMix.Temperature = Mathf.Max(srcMix.Temperature, 0.1f);
                }
            }

            if (dstNode != null)
            {
                dstNode.Thermal.InternalEnergyJ += enthalpyTransferred;
                float newDstMoles = dstMix.TotalMoles();
                if (newDstMoles > 0f)
                {
                    dstMix.Temperature = dstNode.Thermal.InternalEnergyJ / (newDstMoles * SPECIFIC_HEAT_CP);
                    dstMix.Temperature = Mathf.Max(dstMix.Temperature, 0.1f);
                }
            }
        }

        public void ConnectToNode(AtmosphericNode node)
        {
            connectedNode = node;
            isConnected = true;
        }

        public void Disconnect()
        {
            connectedNode = null;
            isConnected = false;
        }

        public float GetPressure()
        {
            return storedGas.GetPressure();
        }

        public float GetFillPercentage()
        {
            return (GetPressure() / maxPressure) * 100f;
        }

        public string GetStatusText()
        {
            if (!isConnected) return "Disconnected";

            string modeText = mode.ToString();
            float pressure = GetPressure();
            return $"{modeText} | {pressure:F1} kPa ({GetFillPercentage():F0}%)";
        }

        private void OnDrawGizmosSelected()
        {
            if (isConnected && connectedNode != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, connectedNode.transform.position);
                Gizmos.DrawWireCube(connectedNode.transform.position, Vector3.one * 0.3f);
            }
        }
    }
}
