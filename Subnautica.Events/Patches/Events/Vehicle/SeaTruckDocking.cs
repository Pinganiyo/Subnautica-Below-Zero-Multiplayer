namespace Subnautica.Events.Patches.Events.Vehicle
{
    using HarmonyLib;

    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.Events.EventArgs;

    using System;
    using System.Collections.Generic;

    using UnityEngine;

    [HarmonyPatch]
    public static class SeaTruckDocking
    {
        /**
         *
         * Fonksiyonu yamalar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::SeaTruckDockingBay), nameof(global::SeaTruckDockingBay.OnTriggerEnter))]
        private static bool Prefix(global::SeaTruckDockingBay __instance, Collider other)
        {
            if (!Network.IsMultiplayerActive)
            {
                return true;
            }

            Log.Info($"SeaTruckDocking.Trigger: fired. dockedObjectNull={__instance.dockedObject == null}, timeSinceUndock={Time.time - __instance.timeUndocked:0.0}s");

            if (__instance.dockedObject != null || __instance.timeUndocked + 2.0 >= Time.time)
            {
                Log.Info($"SeaTruckDocking.Trigger: blocked. dockedObjectNull={__instance.dockedObject == null}");
                return false;
            }

            var gameObject = UWE.Utils.GetEntityRoot(other.gameObject);
            if (gameObject == null)
            {
                gameObject = other.gameObject;
            }

            var lwe = other.GetComponentInParent<LargeWorldEntity>();
            if (lwe == null)
            {
                Log.Info("SeaTruckDocking.Trigger: blocked. No LargeWorldEntity.");
                return false;
            }

            if (!gameObject.TryGetComponent<Dockable>(out var dockable) || ((IDockingBay)__instance).AllowedToDock(dockable) == false)
            {
                Log.Info("SeaTruckDocking.Trigger: blocked. Not dockable or not allowed.");
                return false;
            }

            try
            {
                VehicleDockingEventArgs args = new VehicleDockingEventArgs(__instance.truckSegment.gameObject.GetIdentityId(), lwe.gameObject, TechType.SeaTruckDockingModule, Vector3.zero, Vector3.zero, Quaternion.identity);

                Handlers.Vehicle.OnDocking(args);

                return args.IsAllowed;
            }
            catch (Exception e)
            {
                Log.Error($"Docking.Prefix: {e}\n{e.StackTrace}");
            }

            return true;
        }
    }

    /**
     *
     * Yakındaki prawn otomatik kenetlenir.
     * Fizik tetikleyicisi kaçırıldığında yedek olarak çalışır.
     *
     */
    [HarmonyPatch]
    public static class SeaTruckDockingProximity
    {
        private const float AutoDockRadius  = 4.0f;
        private const float CheckInterval   = 0.5f;
        private const float UndockCooldown  = 2.0f;

        private static readonly Dictionary<string, BayAutoDockState> BayStates = new Dictionary<string, BayAutoDockState>();

        private class BayAutoDockState
        {
            public float NextCheckTime;
            public bool Initialized;
            public Dictionary<string, bool> PrawnInside = new Dictionary<string, bool>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(global::SeaTruckDockingBay), nameof(global::SeaTruckDockingBay.Update))]
        private static void Postfix(global::SeaTruckDockingBay __instance)
        {
            if (!Network.IsMultiplayerActive)
            {
                return;
            }

            try
            {
                if (__instance.dockedObject != null || __instance.timeUndocked + UndockCooldown >= Time.time)
                {
                    return;
                }

                if (__instance.truckSegment == null || __instance.dockingPosition == null)
                {
                    return;
                }

                var bayId = __instance.truckSegment.gameObject.GetIdentityId(false);
                if (string.IsNullOrEmpty(bayId))
                {
                    return;
                }

                if (!BayStates.TryGetValue(bayId, out var state))
                {
                    state = new BayAutoDockState();
                    BayStates[bayId] = state;
                }

                if (Time.time < state.NextCheckTime)
                {
                    return;
                }

                state.NextCheckTime = Time.time + CheckInterval;

                var seen = new HashSet<string>();
                foreach (var exosuit in UnityEngine.Object.FindObjectsOfType<global::Exosuit>())
                {
                    if (exosuit == null)
                    {
                        continue;
                    }

                    var vehicle = exosuit.GetComponent<global::Vehicle>();
                    if (vehicle != null && vehicle.docked)
                    {
                        continue;
                    }

                    var prawnId = exosuit.gameObject.GetIdentityId(false);
                    if (string.IsNullOrEmpty(prawnId))
                    {
                        continue;
                    }

                    seen.Add(prawnId);

                    if (!exosuit.TryGetComponent<Dockable>(out var dockable) || ((IDockingBay)__instance).AllowedToDock(dockable) == false)
                    {
                        state.PrawnInside[prawnId] = false;
                        continue;
                    }

                    var inside = Vector3.Distance(exosuit.transform.position, __instance.dockingPosition.position) <= AutoDockRadius;

                    var wasInside = state.Initialized && state.PrawnInside.TryGetValue(prawnId, out var previous) && previous;
                    state.PrawnInside[prawnId] = inside;

                    if (inside != wasInside)
                    {
                        Log.Info($"SeaTruckDockingProximity: prawn {prawnId} {(inside ? "entered" : "left")} bay {bayId} radius.");
                    }

                    if (inside && !wasInside)
                    {
                        TryAutoDock(__instance, bayId, exosuit.gameObject);
                    }
                }

                foreach (var missingId in new List<string>(state.PrawnInside.Keys))
                {
                    if (!seen.Contains(missingId))
                    {
                        state.PrawnInside.Remove(missingId);
                    }
                }

                state.Initialized = true;
            }
            catch (Exception e)
            {
                Log.Error($"SeaTruckDockingProximity.Postfix: {e}\n{e.StackTrace}");
            }
        }

        /**
         *
         * Yakın prawn için kenetlenme isteği gönderir.
         *
         */
        private static void TryAutoDock(global::SeaTruckDockingBay bay, string bayId, GameObject prawnGameObject)
        {
            try
            {
                if (bay.dockedObject != null)
                {
                    Log.Info($"SeaTruckDockingProximity: auto-dock skipped, bay {bayId} occupied.");
                    return;
                }

                Log.Info($"SeaTruckDockingProximity: sending dock request. bay={bayId}, prawn={prawnGameObject.GetIdentityId(false)}");

                VehicleDockingEventArgs args = new VehicleDockingEventArgs(bayId, prawnGameObject, TechType.SeaTruckDockingModule, Vector3.zero, Vector3.zero, Quaternion.identity);

                Handlers.Vehicle.OnDocking(args);
            }
            catch (Exception e)
            {
                Log.Error($"SeaTruckDockingProximity.TryAutoDock: {e}\n{e.StackTrace}");
            }
        }
    }
}
