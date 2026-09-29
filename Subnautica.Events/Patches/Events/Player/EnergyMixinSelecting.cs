namespace Subnautica.Events.Patches.Events.Player
{
    using System;

    using HarmonyLib;

    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.Events.EventArgs;

    using UnityEngine;

    [HarmonyPatch(typeof(global::EnergyMixin), nameof(global::EnergyMixin.Select))]
    public class EnergyMixinSelecting
    {
        /**
         *
         * Fonksiyonu yamalar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static bool Prefix(global::EnergyMixin __instance, InventoryItem item)
        {
            if (!Network.IsMultiplayerActive )
            {
                return true;
            }

            var batterySlotId = EnergyMixinSelecting.GetBatterySlotId(__instance);
            var batteryType   = EnergyMixinSelecting.GetBatteryTechType(item);
            var vehicleId     = EnergyMixinSelecting.GetVehicleUniqueId(__instance);
            var vehicleType   = EnergyMixinSelecting.GetVehicleType(__instance);

            if (batterySlotId.IsNull() || vehicleId.IsNull() || !vehicleType.IsVehicle())
            {
                return true;
            }

            var storedItem = __instance.batterySlot.storedItem;
            if (storedItem != null)
            {
                if (item != null)
                {
                    if (storedItem == item)
                    {
                        return false;
                    }

                    try
                    {
                        EnergyMixinSelectingEventArgs args = new EnergyMixinSelectingEventArgs(vehicleId, batterySlotId, batteryType, vehicleType, item.item, false, true);

                        Handlers.Player.OnEnergyMixinSelecting(args);

                        return args.IsAllowed;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"EnergyMixinSelecting.Prefix(1): {e}\n{e.StackTrace}");
                    }
                }
                else
                {
                    try
                    {
                        EnergyMixinSelectingEventArgs args = new EnergyMixinSelectingEventArgs(vehicleId, batterySlotId, batteryType, vehicleType, null, false, false);

                        Handlers.Player.OnEnergyMixinSelecting(args);

                        return args.IsAllowed;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"EnergyMixinSelecting.Prefix(2): {e}\n{e.StackTrace}");
                    }
                }
            }
            else
            {
                if (item == null)
                {
                    return false;
                }

                try
                {
                    EnergyMixinSelectingEventArgs args = new EnergyMixinSelectingEventArgs(vehicleId, batterySlotId, batteryType, vehicleType, item.item, true);

                    Handlers.Player.OnEnergyMixinSelecting(args);

                    return args.IsAllowed;
                }
                catch (Exception e)
                {
                    Log.Error($"EnergyMixinSelecting.Prefix(3): {e}\n{e.StackTrace}");
                }
            }

            return true;
        }

        /**
         *
         * Slot idsini döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static string GetBatterySlotId(global::EnergyMixin energyMixin)
        {
            if (energyMixin == null)
            {
                return null;
            }

            string id = null;
            if (energyMixin.storageRoot != null && energyMixin.storageRoot.gameObject != null)
            {
                id = energyMixin.storageRoot.gameObject.GetIdentityId();
            }

            if (string.IsNullOrEmpty(id))
            {
                id = energyMixin.gameObject.GetIdentityId();
            }

            if (string.IsNullOrEmpty(id))
            {
                var vehicleId = GetVehicleUniqueId(energyMixin);
                if (!string.IsNullOrEmpty(vehicleId))
                {
                    bool isSlot2 = energyMixin.name.Contains("Right") || energyMixin.name.Contains("2") || (energyMixin.storageRoot != null && (energyMixin.storageRoot.name.Contains("Right") || energyMixin.storageRoot.name.Contains("2")));
                    id = $"{vehicleId}_PowerCell{(isSlot2 ? 2 : 1)}";
                    if (energyMixin.storageRoot != null && energyMixin.storageRoot.gameObject != null)
                    {
                        Network.Identifier.SetIdentityId(energyMixin.storageRoot.gameObject, id);
                    }
                    Network.Identifier.SetIdentityId(energyMixin.gameObject, id);
                }
            }

            return id;
        }

        /**
         *
         * Batarya türünü döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static TechType GetBatteryTechType(global::InventoryItem item)
        {
            if (item == null || item.item == null)
            {
                return TechType.None;
            }

            var techType = item.item.GetTechType();
            if (techType != TechType.None)
            {
                return techType;
            }

            return CraftData.GetTechType(item.item.gameObject);
        }

        /**
         *
         * Araç idsini döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static string GetVehicleUniqueId(global::EnergyMixin gameObject)
        {
            var vehicleGameObject = GetVehicleGameObject(gameObject);
            if (vehicleGameObject == null)
            {
                return null;
            }

            return vehicleGameObject.GetIdentityId();
        }

        /**
         *
         * Araç türünü döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static TechType GetVehicleType(global::EnergyMixin gameObject)
        {
            var vehicleGameObject = GetVehicleGameObject(gameObject);
            if (vehicleGameObject == null)
            {
                return TechType.None;
            }

            if (vehicleGameObject.GetComponent<global::Exosuit>() != null)
            {
                return TechType.Exosuit;
            }

            if (vehicleGameObject.GetComponent<global::SeaTruckSegment>() != null)
            {
                return TechType.SeaTruck;
            }

            return CraftData.GetTechType(vehicleGameObject);
        }

        /**
         *
         * Araç nesnesini döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static GameObject GetVehicleGameObject(global::EnergyMixin energyMixin)
        {
            if (energyMixin == null)
            {
                return null;
            }

            var exosuit = energyMixin.GetComponentInParent<global::Exosuit>();
            if (exosuit != null)
            {
                return exosuit.gameObject;
            }

            var seaTruck = energyMixin.GetComponentInParent<global::SeaTruckSegment>();
            if (seaTruck != null)
            {
                return seaTruck.gameObject;
            }

            var lwe = energyMixin.GetComponentInParent<LargeWorldEntity>();
            if (lwe != null)
            {
                return lwe.gameObject;
            }

            if (energyMixin.storageRoot != null && energyMixin.storageRoot.transform.parent != null)
            {
                return energyMixin.storageRoot.transform.parent.gameObject;
            }

            return null;
        }
    }
}