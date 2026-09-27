namespace Subnautica.Events.Patches.Fixes.Game
{
    using HarmonyLib;

    using Subnautica.API.Features;

    using System;
    using System.Collections.Generic;

    /**
     *
     * Kesik içerikten geri yüklenen evrensel araç yükseltmelerinin
     * hem prawn hem seatruck konsollarına uymasını sağlar.
     *
     */
    public static class VehicleUpgradeCompatibility
    {
        /**
         *
         * Her iki araca da uyması gereken yükseltme TechType isimleri.
         *
         */
        private static readonly string[] UniversalUpgradeTechNames = new string[]
        {
            "VehicleArmorPlating",
            "CustomVehiclePowerUpgradeModule",
            "VehiclePowerUpgradeModule",
        };

        private static HashSet<TechType> universalUpgrades;
        private static bool initialized;

        /**
         *
         * Kayıtlı evrensel yükseltmeleri döner.
         *
         */
        public static HashSet<TechType> GetUniversalUpgrades()
        {
            if (!initialized)
            {
                initialized = true;
                universalUpgrades = new HashSet<TechType>();

                foreach (var name in UniversalUpgradeTechNames)
                {
                    try
                    {
                        universalUpgrades.Add((TechType)Enum.Parse(typeof(TechType), name, true));
                    }
                    catch
                    {
                    }
                }
            }

            return universalUpgrades;
        }
    }

    /**
     *
     * Evrensel yükseltmelerin ekipman türünü VehicleModule yapar.
     * Böylece hem prawn hem seatruck yuvalarına uyarlar.
     *
     */
    [HarmonyPatch(typeof(global::TechData), nameof(global::TechData.GetEquipmentType))]
    public static class VehicleUpgradeEquipmentTypePatch
    {
        public static void Postfix(TechType techType, ref EquipmentType __result)
        {
            try
            {
                if (VehicleUpgradeCompatibility.GetUniversalUpgrades().Contains(techType))
                {
                    __result = EquipmentType.VehicleModule;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Game.VehicleUpgradeEquipmentTypePatch: {ex}");
            }
        }
    }

    /**
     *
     * VehicleModule türündeki yükseltmelerin SeaTruckModule yuvalarına
     * uymasını sağlar (vanilla sadece Seamoth/Exosuit yuvalarına izin verir).
     *
     */
    [HarmonyPatch(typeof(global::Equipment), nameof(global::Equipment.IsCompatible))]
    public static class VehicleUpgradeCompatibilityPatch
    {
        public static void Postfix(EquipmentType itemType, EquipmentType slotType, ref bool __result)
        {
            try
            {
                if (!__result && itemType == EquipmentType.VehicleModule && slotType == EquipmentType.SeaTruckModule)
                {
                    __result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Game.VehicleUpgradeCompatibilityPatch: {ex}");
            }
        }
    }
}
