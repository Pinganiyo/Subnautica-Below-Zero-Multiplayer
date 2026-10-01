namespace Subnautica.Events.Patches.Fixes.Vehicle
{
    using System;

    using HarmonyLib;

    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.Events.EventArgs;
    using Subnautica.Network.Structures;

    using UnityEngine;

    [HarmonyPatch(typeof(global::SeaTruckDockingBay), nameof(global::SeaTruckDockingBay.Update))]
    public class SeaTruckDockingBay
    {
        private static float NextProcessTime = 0.0f;
        private static float AccumulatedRepair = 0.0f;
        private static float NextRepairSyncTime = 0.0f;

        private const float ProcessInterval = 0.5f;
        private const float RepairRatePerSecond = 10.0f;
        private const float EnergyCostPerHp = 0.2f;

        private static void Prefix(global::SeaTruckDockingBay __instance)
        {
            if (!Network.IsMultiplayerActive || !__instance.dockedObject)
            {
                return;
            }

            if (__instance.dockedObject.transform.localPosition != Vector3.zero)
            {
                if (ZeroVector3.Distance(__instance.dockedObject.transform.position, __instance.transform.position) >= 25f)
                {
                    __instance.dockedObject.transform.localPosition = Vector3.zero;
                    __instance.dockedObject.transform.localRotation = Quaternion.identity;
                }
            }

            UpdateAutoChargeAndRepair(__instance);
        }

        private static void UpdateAutoChargeAndRepair(global::SeaTruckDockingBay bay)
        {
            if (Time.time < NextProcessTime)
            {
                return;
            }

            var interval = NextProcessTime == 0.0f ? ProcessInterval : (Time.time - (NextProcessTime - ProcessInterval));
            NextProcessTime = Time.time + ProcessInterval;

            var vehicle = bay.dockedObject.vehicle ?? bay.dockedObject.GetComponent<global::Vehicle>() ?? bay.dockedObject.GetComponentInChildren<global::Vehicle>();

            // 1. Auto-Charge docked vehicle's batteries magically (WITHOUT draining SeaTruck power)
            try
            {
                bay.dockedObject.Recharge(1000f, interval, out _);
            }
            catch
            {
            }

            if (vehicle != null && vehicle.energyInterface != null && vehicle.energyInterface.sources != null)
            {
                float chargePerTick = 10.0f * interval;
                foreach (var source in vehicle.energyInterface.sources)
                {
                    if (source != null && source.battery != null)
                    {
                        if (source.battery.charge < source.battery.capacity)
                        {
                            source.battery.charge = Mathf.Min(source.battery.capacity, source.battery.charge + chargePerTick);
                        }
                    }
                }
            }

            // 2. Auto-Repair docked vehicle magically (WITHOUT draining SeaTruck power)
            var liveMixin = vehicle?.liveMixin ?? bay.dockedObject.GetComponent<global::LiveMixin>() ?? bay.dockedObject.GetComponentInChildren<global::LiveMixin>();

            if (liveMixin != null && liveMixin.health < liveMixin.maxHealth)
            {
                float neededHealth = liveMixin.maxHealth - liveMixin.health;
                float repairAmount = Mathf.Min(RepairRatePerSecond * interval, neededHealth);
                if (repairAmount > 0.0f)
                {
                    liveMixin.AddHealth(repairAmount);
                    AccumulatedRepair += repairAmount;

                    if (Time.time >= NextRepairSyncTime || liveMixin.health >= liveMixin.maxHealth)
                    {
                        NextRepairSyncTime = Time.time + 1.0f;
                        SyncRepair(bay, vehicle, liveMixin);
                    }
                }
            }
        }

        private static void SyncRepair(global::SeaTruckDockingBay bay, global::Vehicle vehicle, global::LiveMixin liveMixin)
        {
            if (AccumulatedRepair <= 0.0f)
            {
                return;
            }

            var vehicleObj = vehicle?.gameObject ?? bay.dockedObject.gameObject;
            var vehicleId = Network.Identifier.GetIdentityId(vehicleObj, false) ?? vehicleObj.GetIdentityId(false);
            if (!string.IsNullOrEmpty(vehicleId))
            {
                if (vehicleId.Contains("_PowerCell"))
                {
                    vehicleId = vehicleId.Substring(0, vehicleId.IndexOf("_PowerCell"));
                }

                try
                {
                    var techType = CraftData.GetTechType(vehicleObj);
                    if (techType == TechType.None)
                    {
                        techType = TechType.Exosuit;
                    }

                    Handlers.Items.OnWelding(new WeldingEventArgs(vehicleId, techType, AccumulatedRepair));
                }
                catch (Exception e)
                {
                    Log.Error($"SeaTruckDockingBay.SyncRepair error: {e}");
                }
            }

            AccumulatedRepair = 0.0f;
        }
    }
}
