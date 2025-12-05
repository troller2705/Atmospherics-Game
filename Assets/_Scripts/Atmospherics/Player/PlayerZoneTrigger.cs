using UnityEngine;
using Atmospherics.Core;

namespace Atmospherics.Player
{
    public class PlayerZoneTrigger : MonoBehaviour
    {
        [Header("References")]
        public AtmosphericNode associatedNode;
        public PlayerAtmosphericNeeds targetPlayer;

        [Header("Settings")]
        public bool autoDetectPlayer = true;
        public string playerTag = "Player";

        [Header("Debug")]
        public bool logZoneChanges = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!ShouldProcessCollider(other)) return;

            PlayerAtmosphericNeeds player = GetPlayerFromCollider(other);
            if (player != null && associatedNode != null)
            {
                player.SetCurrentNode(associatedNode);

                if (logZoneChanges)
                {
                    Debug.Log($"[Zone Trigger] Player entered: {associatedNode.NodeName}");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ShouldProcessCollider(other)) return;

            PlayerAtmosphericNeeds player = GetPlayerFromCollider(other);
            if (player != null)
            {
                player.SetCurrentNode(null);

                if (logZoneChanges)
                {
                    Debug.Log("[Zone Trigger] Player left zone");
                }
            }
        }

        private bool ShouldProcessCollider(Collider other)
        {
            if (autoDetectPlayer)
            {
                return other.CompareTag(playerTag);
            }
            else
            {
                return targetPlayer != null && other.GetComponent<PlayerAtmosphericNeeds>() == targetPlayer;
            }
        }

        private PlayerAtmosphericNeeds GetPlayerFromCollider(Collider other)
        {
            if (!autoDetectPlayer && targetPlayer != null)
            {
                return targetPlayer;
            }

            return other.GetComponent<PlayerAtmosphericNeeds>();
        }

        private void OnDrawGizmos()
        {
            if (associatedNode == null) return;

            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(transform.position, associatedNode.transform.position);
        }
    }
}
