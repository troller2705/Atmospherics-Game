using UnityEngine;

namespace Atmospherics.Core
{
    [System.Serializable]
    public class Pipe : GasConnection
    {
        [Header("Pipe Settings")]
        [Tooltip("How easily gas flows through this pipe (higher = more flow)")]
        public float Conductance = 0.1f;

        public void Initialize(string name, AtmosphericNode a, AtmosphericNode b, float conductance)
        {
            base.Initialize(name, a, b);
            Conductance = conductance;
        }

        public override void UpdateConnection(float deltaTime)
        {
            if (!IsValid || !IsActive) return;

            float pA = NodeA.Pressure;
            float pB = NodeB.Pressure;
            float molesToMove = GasTransfer.CalculatePressureDrivenFlow(pA, pB, Conductance, deltaTime);

            if (molesToMove <= 0f) return;

            AtmosphericNode source = pA > pB ? NodeA : NodeB;
            AtmosphericNode destination = pA > pB ? NodeB : NodeA;

            float maxMove = source.Mixture.TotalMoles() * 0.5f;
            molesToMove = Mathf.Clamp(molesToMove, 0f, maxMove);

            if (molesToMove > 0f)
            {
                GasTransfer.TransferMoles(source, destination, molesToMove, out _);
            }
        }

        public void UpdateFlow(float dt)
        {
            UpdateConnection(dt);
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            if (!IsValid) return;

            float deltaP = Mathf.Abs(NodeA.Pressure - NodeB.Pressure);
            Gizmos.color = Color.Lerp(Color.green, Color.yellow, Mathf.Clamp01(deltaP / 50f));
            Gizmos.DrawLine(NodeA.Position, NodeB.Position);
        }
#endif
    }
}
