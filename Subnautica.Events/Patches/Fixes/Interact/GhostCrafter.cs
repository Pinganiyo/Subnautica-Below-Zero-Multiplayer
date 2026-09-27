namespace Subnautica.Events.Patches.Fixes.Interact
{
    using HarmonyLib;

    using Subnautica.API.Extensions;
    using Subnautica.API.Features;

    using UnityEngine;

    [HarmonyPatch(typeof(global::GhostCrafter), nameof(global::GhostCrafter.OnHandHover))]
    public class GhostCrafter
    {
        /**
         *
         * Fonksiyonu yamalar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static bool Prefix(global::GhostCrafter __instance, GUIHand hand)
        {
            if (!Network.IsMultiplayerActive)
            {
                return true;
            }

            if (!__instance.enabled || __instance.logic == null)
            {
                return false;
            }

            GhostCrafter.EnsureCrafterPower(__instance);

            var uniqueId = GhostCrafter.GetUniqueId(__instance.gameObject);
            if (uniqueId.IsNull())
            {
                return true;
            }

            if (Interact.IsBlocked(uniqueId))
            {
                Interact.ShowUseDenyMessage();
                return false;
            }

            return true;
        }

        /**
         * Ensures power relay, base reference, and mod dependencies are connected.
         */
        public static void EnsureCrafterPower(global::GhostCrafter crafter)
        {
            if (crafter == null)
            {
                return;
            }

            // Only use GetComponentInParent — the crafter MUST be in the Base hierarchy for
            // Base.IsPowered(position) to work correctly. Distance-based fallback would assign
            // a Base whose grid doesn't contain the crafter's world position → IsPowered returns
            // false (cell index -1) → EasyCraft shows "No Power!" even when base is powered.
            if (crafter.baseComp == null)
            {
                var foundBase = crafter.GetComponentInParent<global::Base>();
                if (foundBase != null)
                {
                    crafter.baseComp = foundBase;
                }
                // Do NOT fall back to distance-based Base search — it breaks EasyCraft's IsPowered check.
            }

            if (crafter.powerRelay == null)
            {
                // Walk hierarchy first (vanilla path — BasePowerRelay is on the Base GO and
                // BasePowerRelay extends PowerRelay, so GetComponentInParent will find it).
                var powerRelay = crafter.GetComponentInParent<PowerRelay>() ?? crafter.GetComponent<PowerRelay>();

                // If hierarchy walk failed (modded crafter not under Base GO), try the base component directly.
                if (powerRelay == null && crafter.baseComp != null)
                {
                    powerRelay = crafter.baseComp.GetComponent<BasePowerRelay>() as PowerRelay
                                 ?? crafter.baseComp.GetComponent<PowerRelay>();
                }

                // Try SubRoot as a final fallback (e.g. Cyclops-hosted crafters).
                if (powerRelay == null)
                {
                    var subRoot = crafter.GetComponentInParent<SubRoot>();
                    if (subRoot != null)
                    {
                        powerRelay = subRoot.powerRelay;
                    }
                }

                if (powerRelay != null)
                {
                    crafter.powerRelay = powerRelay;
                }
                else if (crafter.needsPower)
                {
                    // Last resort: disable power requirement so crafting isn't blocked entirely.
                    crafter.needsPower = false;
                }
            }

            // SubnauticaPets compatibility: ensure PetFabricator has Base, _baseParentGameObject, and _spawnPoint set
            try
            {
                var petFab = crafter.GetComponent("PetFabricator");
                if (petFab != null && crafter.baseComp != null)
                {
                    var baseProp = petFab.GetType().GetProperty("Base");
                    if (baseProp != null && baseProp.GetValue(petFab, null) == null)
                    {
                        baseProp.SetValue(petFab, crafter.baseComp, null);
                    }

                    var baseParentField = petFab.GetType().GetField("_baseParentGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (baseParentField != null && baseParentField.GetValue(petFab) == null)
                    {
                        baseParentField.SetValue(petFab, crafter.baseComp.gameObject);
                    }

                    var spawnPointField = petFab.GetType().GetField("_spawnPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (spawnPointField != null && (Vector3)spawnPointField.GetValue(petFab) == Vector3.zero)
                    {
                        var ghostModel = crafter.GetComponent<CrafterGhostModel>();
                        Vector3 spawnPos = (ghostModel != null && ghostModel.itemSpawnPoint != null)
                            ? ghostModel.itemSpawnPoint.position
                            : crafter.transform.position + crafter.transform.forward * 0.5f;
                        spawnPointField.SetValue(petFab, spawnPos);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"GhostCrafter.EnsureCrafterPower SubnauticaPets error: {ex}");
            }
        }

        /**
         *
         * Yapı idsini döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static string GetUniqueId(GameObject gameObject)
        {
            var constructable = gameObject.GetComponentInParent<Constructable>();
            if (constructable)
            {
                return constructable.gameObject.GetIdentityId();
            }

            var mapRoomFunctionality = gameObject.GetComponentInParent<MapRoomFunctionality>();
            if (mapRoomFunctionality)
            {
                return mapRoomFunctionality.GetBaseDeconstructable()?.gameObject?.GetIdentityId();
            }

            var spawnBase = gameObject.GetComponentInParent<global::PrefabSpawnBase>();
            if (spawnBase)
            {
                return spawnBase.gameObject.GetIdentityId();
            }

            if (gameObject.name.Contains("PrecursorFabricatorBase"))
            {
                return gameObject.gameObject.GetIdentityId();
            }

            var baseDeconstructable = gameObject.GetComponentInParent<global::BaseDeconstructable>();
            if (baseDeconstructable)
            {
                return baseDeconstructable.gameObject.GetIdentityId();
            }

            return null;
        }

        /**
         *
         * Fonksiyonu yamalar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static TechType GetTechType(GameObject gameObject)
        {
            var constructable = gameObject.GetComponentInParent<Constructable>();
            if (constructable)
            {
                return constructable.techType;
            }

            if (gameObject.GetComponentInParent<MapRoomFunctionality>())
            {
                return TechType.BaseMapRoom;
            }

            if (gameObject.GetComponentInParent<global::PrefabSpawnBase>() || gameObject.name.Contains("PrecursorFabricatorBase"))
            {
                return TechType.Fabricator;
            }

            var baseDeconstructable = gameObject.GetComponentInParent<global::BaseDeconstructable>();
            if (baseDeconstructable)
            {
                return baseDeconstructable.recipe;
            }

            return TechType.None;
        }

        /**
         *
         * Maproom için
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */    
        private static BaseDeconstructable GetBaseMapRoomDeconstructable(MapRoomFunctionality mapRoom)
        {
            var baseComp = mapRoom.GetComponentInParent<global::Base>();
            if (baseComp == null)
            {
                return null;
            }

            var cell = baseComp.NormalizeCell(baseComp.WorldToGrid(mapRoom.transform.position));

            foreach (var component in baseComp.GetComponentsInChildren<BaseDeconstructable>())
            {
                if (component.name == "BaseMapRoom" || component.name == "BaseMapRoom(Clone)")
                {
                    if (baseComp.NormalizeCell(baseComp.WorldToGrid(component.transform.position)) == cell)
                    {
                        return component;
                    }
                }
            }

            return null;
        }
    }
}
