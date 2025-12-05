using UnityEngine;

namespace Atmospherics.Core
{
    public abstract class GasConnection : MonoBehaviour
    {
        [Header("Connection")]
        public string ConnectionName;
        public AtmosphericNode NodeA;
        public AtmosphericNode NodeB;

        [Header("Settings")]
        public bool IsActive = true;

        public bool IsValid => NodeA != null && NodeB != null && 
                              NodeA.Mixture != null && NodeB.Mixture != null;

        public Vector3 MidPoint => IsValid ? 
            (NodeA.Position + NodeB.Position) * 0.5f : Vector3.zero;

        public Vector3 Direction => IsValid ? 
            (NodeB.Position - NodeA.Position).normalized : Vector3.forward;

        public float Distance => IsValid ? 
            Vector3.Distance(NodeA.Position, NodeB.Position) : 0f;

        public virtual void Initialize(string name, AtmosphericNode a, AtmosphericNode b)
        {
            ConnectionName = name;
            NodeA = a;
            NodeB = b;
        }

        public abstract void UpdateConnection(float deltaTime);

        public virtual void DebugStatus()
        {
            if (!IsValid)
            {
                Debug.Log($"{ConnectionName} | INVALID CONNECTION");
                return;
            }

            Debug.Log($"{ConnectionName} | {NodeA.NodeName} ({NodeA.Pressure:F1} kPa) <-> {NodeB.NodeName} ({NodeB.Pressure:F1} kPa)");
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (!IsValid) return;

            Gizmos.color = IsActive ? Color.cyan : Color.gray;
            Gizmos.DrawLine(NodeA.Position, NodeB.Position);
        }
#endif
    }
}
