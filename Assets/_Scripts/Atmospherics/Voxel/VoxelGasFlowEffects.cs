using UnityEngine;
using System.Collections.Generic;
using Atmospherics.Core;

namespace Atmospherics.Voxel
{
    [RequireComponent(typeof(VoxelAtmosphericBridge))]
    public class VoxelGasFlowEffects : MonoBehaviour
    {
        [Header("Particle Settings")]
        public GameObject gasFlowParticlePrefab;
        public int maxActiveEmitters = 20;
        public float emitterLifetime = 2f;
        public float minimumPressureDifference = 20f;

        [Header("Flow Detection")]
        public bool autoDetectFlow = true;
        public float flowCheckInterval = 0.5f;

        [Header("Particle Appearance")]
        public Color highPressureColor = new Color(1f, 0.3f, 0.3f, 0.5f);
        public Color lowPressureColor = new Color(0.3f, 0.3f, 1f, 0.5f);
        public float particleSize = 0.1f;
        public float particleSpeed = 2f;

        private VoxelAtmosphericBridge bridge;
        private Dictionary<Vector3Int, ParticleEmitterInfo> activeEmitters = new Dictionary<Vector3Int, ParticleEmitterInfo>();
        private float lastFlowCheckTime;

        private class ParticleEmitterInfo
        {
            public GameObject gameObject;
            public ParticleSystem particleSystem;
            public Vector3Int voxelPos;
            public float spawnTime;
            public Vector3 flowDirection;
        }

        private void Awake()
        {
            bridge = GetComponent<VoxelAtmosphericBridge>();
        }

        private void Update()
        {
            if (autoDetectFlow && Time.time - lastFlowCheckTime >= flowCheckInterval)
            {
                DetectAndCreateFlowEffects();
                lastFlowCheckTime = Time.time;
            }

            CleanupExpiredEmitters();
        }

        private void DetectAndCreateFlowEffects()
        {
            foreach (var kvp in bridge.VoxelGrid)
            {
                Vector3Int voxelPos = kvp.Key;
                VoxelAtmosphericBridge.VoxelData voxelData = kvp.Value;

                if (!voxelData.IsPassable()) continue;

                AtmosphericNode node = bridge.GetNodeAtVoxel(voxelPos);
                if (node == null) continue;

                CheckVoxelForFlow(voxelPos, node);
            }
        }

        private void CheckVoxelForFlow(Vector3Int voxelPos, AtmosphericNode node)
        {
            Vector3Int[] neighbors = new Vector3Int[]
            {
                voxelPos + Vector3Int.right,
                voxelPos + Vector3Int.left,
                voxelPos + Vector3Int.up,
                voxelPos + Vector3Int.down,
                voxelPos + Vector3Int.forward,
                voxelPos + Vector3Int.back
            };

            foreach (var neighborPos in neighbors)
            {
                if (!bridge.IsValidPosition(neighborPos)) continue;

                var neighborData = bridge.GetVoxel(neighborPos);
                if (neighborData.IsPassable())
                {
                    AtmosphericNode neighborNode = bridge.GetNodeAtVoxel(neighborPos);
                    if (neighborNode != null && neighborNode != node)
                    {
                        float pressureDiff = Mathf.Abs(node.Pressure - neighborNode.Pressure);
                        
                        if (pressureDiff >= minimumPressureDifference)
                        {
                            Vector3 flowDirection = (neighborNode.Pressure < node.Pressure) 
                                ? ((Vector3)(neighborPos - voxelPos)).normalized 
                                : ((Vector3)(voxelPos - neighborPos)).normalized;

                            CreateFlowEffect(voxelPos, flowDirection, pressureDiff);
                        }
                    }
                }
            }
        }

        public void CreateFlowEffect(Vector3Int voxelPos, Vector3 direction, float intensity)
        {
            if (activeEmitters.Count >= maxActiveEmitters)
            {
                RemoveOldestEmitter();
            }

            if (activeEmitters.ContainsKey(voxelPos))
            {
                UpdateExistingEmitter(voxelPos, direction, intensity);
                return;
            }

            GameObject emitterGO;
            ParticleSystem ps;

            if (gasFlowParticlePrefab != null)
            {
                emitterGO = Instantiate(gasFlowParticlePrefab, transform);
                ps = emitterGO.GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    ps = emitterGO.AddComponent<ParticleSystem>();
                }
            }
            else
            {
                emitterGO = new GameObject($"GasFlow_{voxelPos}");
                emitterGO.transform.SetParent(transform);
                ps = emitterGO.AddComponent<ParticleSystem>();
                ConfigureParticleSystem(ps);
            }

            Vector3 worldPos = bridge.VoxelToWorldPosition(voxelPos) + Vector3.one * bridge.voxelSize * 0.5f;
            emitterGO.transform.position = worldPos;
            emitterGO.transform.rotation = Quaternion.LookRotation(direction);

            var main = ps.main;
            main.startColor = Color.Lerp(lowPressureColor, highPressureColor, Mathf.Clamp01(intensity / 100f));
            main.startSpeed = particleSpeed * Mathf.Clamp01(intensity / 50f);

            var emission = ps.emission;
            emission.rateOverTime = Mathf.Clamp(intensity * 2f, 10f, 100f);

            ParticleEmitterInfo info = new ParticleEmitterInfo
            {
                gameObject = emitterGO,
                particleSystem = ps,
                voxelPos = voxelPos,
                spawnTime = Time.time,
                flowDirection = direction
            };

            activeEmitters[voxelPos] = info;
        }

        private void ConfigureParticleSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = particleSpeed;
            main.startSize = particleSize;
            main.startColor = highPressureColor;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.1f;

            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.5f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private void UpdateExistingEmitter(Vector3Int voxelPos, Vector3 direction, float intensity)
        {
            if (!activeEmitters.ContainsKey(voxelPos)) return;

            var info = activeEmitters[voxelPos];
            info.spawnTime = Time.time;
            info.flowDirection = direction;

            info.gameObject.transform.rotation = Quaternion.LookRotation(direction);

            var main = info.particleSystem.main;
            main.startColor = Color.Lerp(lowPressureColor, highPressureColor, Mathf.Clamp01(intensity / 100f));
            main.startSpeed = particleSpeed * Mathf.Clamp01(intensity / 50f);

            var emission = info.particleSystem.emission;
            emission.rateOverTime = Mathf.Clamp(intensity * 2f, 10f, 100f);
        }

        private void CleanupExpiredEmitters()
        {
            List<Vector3Int> toRemove = new List<Vector3Int>();

            foreach (var kvp in activeEmitters)
            {
                if (Time.time - kvp.Value.spawnTime >= emitterLifetime)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var voxelPos in toRemove)
            {
                RemoveEmitter(voxelPos);
            }
        }

        private void RemoveOldestEmitter()
        {
            Vector3Int oldest = Vector3Int.zero;
            float oldestTime = float.MaxValue;

            foreach (var kvp in activeEmitters)
            {
                if (kvp.Value.spawnTime < oldestTime)
                {
                    oldestTime = kvp.Value.spawnTime;
                    oldest = kvp.Key;
                }
            }

            if (activeEmitters.ContainsKey(oldest))
            {
                RemoveEmitter(oldest);
            }
        }

        private void RemoveEmitter(Vector3Int voxelPos)
        {
            if (activeEmitters.ContainsKey(voxelPos))
            {
                if (activeEmitters[voxelPos].gameObject != null)
                {
                    Destroy(activeEmitters[voxelPos].gameObject);
                }
                activeEmitters.Remove(voxelPos);
            }
        }

        public void ClearAllEmitters()
        {
            foreach (var info in activeEmitters.Values)
            {
                if (info.gameObject != null)
                {
                    Destroy(info.gameObject);
                }
            }
            activeEmitters.Clear();
        }

        private void OnDestroy()
        {
            ClearAllEmitters();
        }
    }
}
