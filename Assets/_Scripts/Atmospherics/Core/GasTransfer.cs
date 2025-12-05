using System.Collections.Generic;
using UnityEngine;

namespace Atmospherics.Core
{
    public static class GasTransfer
    {
        private const float SPECIFIC_HEAT_CP = 29.0f;
        private const float MIN_TRANSFER = 1e-12f;

        public static void TransferMoles(
            AtmosphericNode source,
            AtmosphericNode destination,
            float molesToMove,
            out float enthalpyTransferred)
        {
            enthalpyTransferred = 0f;

            if (source == null || destination == null) return;
            if (source.Mixture == null || destination.Mixture == null) return;
            if (molesToMove <= MIN_TRANSFER) return;

            float totalSrc = source.Mixture.TotalMoles();
            if (totalSrc <= MIN_TRANSFER) return;

            float actualMove = Mathf.Clamp(molesToMove, 0f, totalSrc * 0.5f);
            float fraction = actualMove / totalSrc;

            var gasKeys = new List<string>(source.Mixture.Moles.Keys);

            foreach (var gas in gasKeys)
            {
                float srcMoles = source.Mixture.Moles[gas];
                float ratio = srcMoles / totalSrc;
                float moved = ratio * actualMove;

                if (moved < MIN_TRANSFER) continue;

                source.Mixture.Moles[gas] = Mathf.Max(0f, srcMoles - moved);

                if (!destination.Mixture.Moles.ContainsKey(gas))
                    destination.Mixture.Moles[gas] = 0f;

                destination.Mixture.Moles[gas] += moved;
            }

            enthalpyTransferred = actualMove * SPECIFIC_HEAT_CP * source.Mixture.Temperature;

            source.Thermal.InternalEnergyJ -= enthalpyTransferred;
            destination.Thermal.InternalEnergyJ += enthalpyTransferred;

            float newTotalSrc = source.Mixture.TotalMoles();
            float newTotalDst = destination.Mixture.TotalMoles();

            if (newTotalSrc > MIN_TRANSFER)
            {
                source.Mixture.Temperature = source.Thermal.CalculateTemperature(newTotalSrc);
            }

            if (newTotalDst > MIN_TRANSFER)
            {
                destination.Mixture.Temperature = destination.Thermal.CalculateTemperature(newTotalDst);
            }
        }

        public static float CalculatePressureDrivenFlow(float pressureA, float pressureB, float conductance, float deltaTime)
        {
            float dp = pressureA - pressureB;
            if (Mathf.Abs(dp) < 0.0001f) return 0f;

            return Mathf.Abs(dp) * conductance * deltaTime;
        }
    }
}
