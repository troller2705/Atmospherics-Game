using UnityEngine;

namespace Atmospherics.Core
{
    [System.Serializable]
    public class Pump : GasConnection
    {
        [Header("Pump Settings")]
        [Tooltip("Fixed flow rate in moles per second")]
        public float FlowRate = 1f;

        public AtmosphericNode SourceNode
        {
            get => NodeA;
            set => NodeA = value;
        }

        public AtmosphericNode TargetNode
        {
            get => NodeB;
            set => NodeB = value;
        }

        public void Initialize(string name, AtmosphericNode source, AtmosphericNode target, float flowRate)
        {
            base.Initialize(name, source, target);
            FlowRate = flowRate;
        }

        public override void UpdateConnection(float deltaTime)
        {
            if (!IsValid || !IsActive) return;

            float totalSrc = SourceNode.Mixture.TotalMoles();
            if (totalSrc < 1e-8f) return;

            float molesToMove = Mathf.Clamp(FlowRate * deltaTime, 0f, totalSrc * 0.5f);
            if (molesToMove > 0f)
            {
                GasTransfer.TransferMoles(SourceNode, TargetNode, molesToMove, out _);
            }
        }

        public void UpdatePump(float dt)
        {
            UpdateConnection(dt);
        }

        public override void DebugStatus()
        {
            if (!IsValid)
            {
                Debug.Log($"{ConnectionName} | INVALID PUMP");
                return;
            }

            Debug.Log($"{ConnectionName} | Flow: {FlowRate} mol/s | {SourceNode.NodeName} → {TargetNode.NodeName}");
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            if (!IsValid) return;

            Gizmos.color = IsActive ? Color.red : Color.gray;
            Gizmos.DrawLine(SourceNode.Position, TargetNode.Position);

            Gizmos.color = IsActive ? Color.yellow : Color.gray;
            Gizmos.DrawWireCube(MidPoint, Vector3.one * 0.2f);
        }
#endif
    }
}
