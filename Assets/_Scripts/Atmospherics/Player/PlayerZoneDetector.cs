using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Player
{
    public class PlayerZoneDetector : MonoBehaviour
    {
        [Header("Detection")]
        public PlayerAtmosphericNeeds atmosphericNeeds;
        public float detectionRadius = 1f;
        public LayerMask nodeLayerMask = -1;

        [Header("Current Zone")]
        public AtmosphericNode currentNode;
        public LeakBehavior currentZone;

        [Header("Debug")]
        public bool showDebugGizmos = true;

        private void Start()
        {
            if (atmosphericNeeds == null)
                atmosphericNeeds = GetComponent<PlayerAtmosphericNeeds>();

            DetectCurrentNode();
        }

        private void Update()
        {
            DetectCurrentNode();
        }

        private void DetectCurrentNode()
        {
            AtmosphericNode closestNode = FindClosestNode();

            if (closestNode != currentNode)
            {
                currentNode = closestNode;

                if (atmosphericNeeds != null)
                {
                    atmosphericNeeds.SetCurrentNode(currentNode);
                }

                if (currentNode != null)
                {
                    Debug.Log($"[Player] Entered zone: {currentNode.NodeName}");
                }
                else
                {
                    Debug.Log("[Player] Left all zones - in vacuum!");
                }
            }
        }

        private AtmosphericNode FindClosestNode()
        {
            AtmosphericNode closest = null;
            float closestDistance = detectionRadius;

            AtmosphericNode[] allNodes = FindObjectsByType<AtmosphericNode>(FindObjectsSortMode.None);

            foreach (AtmosphericNode node in allNodes)
            {
                float distance = Vector3.Distance(transform.position, node.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = node;
                }
            }

            return closest;
        }

        public bool IsInSafeAtmosphere()
        {
            if (currentNode == null) return false;

            AtmosphericHazards hazards = currentNode.GetComponent<AtmosphericHazards>();
            if (hazards != null)
            {
                return hazards.IsSafeForHumans();
            }

            return false;
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Gizmos.color = currentNode != null ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            if (currentNode != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, currentNode.transform.position);
            }
        }
    }
}
