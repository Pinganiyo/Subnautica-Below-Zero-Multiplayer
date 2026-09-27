namespace Subnautica.Events.Patches.Fixes.Building
{
    using HarmonyLib;

    using Subnautica.API.Features;

    using System;

    using UnityEngine;

    /**
     *
     * Seatruck içi inşa edilen yapıları segmente ebeveynler, böylece kamyonla birlikte hareket ederler.
     *
     */
    [HarmonyPatch(typeof(global::Constructable), nameof(global::Constructable.Construct))]
    public static class SeatruckInteriorConstructionParent
    {
        /**
         *
         * Fonksiyonu yamalar.
         *
         */
        private static void Postfix(global::Constructable __instance)
        {
            if (!Network.IsMultiplayerActive)
            {
                return;
            }

            try
            {
                var gameObject = __instance.gameObject;
                if (gameObject == null)
                {
                    return;
                }

                if (gameObject.GetComponent<global::Vehicle>() != null)
                {
                    return;
                }

                if (gameObject.GetComponentInParent<global::Base>() != null)
                {
                    return;
                }

                if (gameObject.GetComponentInParent<global::SeaTruckSegment>() != null)
                {
                    return;
                }

                var segment = SeatruckInteriorConstructionParent.FindContainingSegment(gameObject.transform.position);
                if (segment != null)
                {
                    gameObject.transform.SetParent(segment.transform, true);
                    Log.Info($"Building.SeatruckInteriorConstructionParent: '{gameObject.name}' parented to SeaTruck segment.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Building.SeatruckInteriorConstructionParent: {ex}");
            }
        }

        /**
         *
         * Verilen konumu içeren SeaTruckSegment değerini döner.
         *
         */
        private static global::SeaTruckSegment FindContainingSegment(Vector3 position)
        {
            global::SeaTruckSegment bestSegment = null;

            var bestDistance = float.MaxValue;
            foreach (var collider in Physics.OverlapSphere(position, 0.1f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (collider == null)
                {
                    continue;
                }

                var segment = collider.GetComponentInParent<global::SeaTruckSegment>();
                if (segment == null)
                {
                    continue;
                }

                var distance = (segment.transform.position - position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSegment  = segment;
                }
            }

            return bestSegment;
        }
    }
}
