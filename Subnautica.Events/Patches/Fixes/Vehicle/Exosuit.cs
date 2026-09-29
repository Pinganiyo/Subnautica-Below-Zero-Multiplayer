namespace Subnautica.Events.Patches.Fixes.Vehicle
{
    using HarmonyLib;

    using Subnautica.API.Extensions;
    using Subnautica.API.Features;

    [HarmonyPatch]
    public static class Exosuit
    {
        private static bool IsLocalPiloting(global::Exosuit __instance)
        {
            if (__instance == null)
            {
                return false;
            }

            if (__instance.GetPilotingMode())
            {
                return true;
            }

            if (global::Player.main != null)
            {
                if (global::Player.main.inExosuit && (global::Player.main.currentMountedVehicle == __instance || global::Player.main.currentMountedVehicle == null))
                {
                    return true;
                }

                if (global::Player.main.transform.parent != null &&
                    __instance.playerPosition != null &&
                    global::Player.main.transform.parent == __instance.playerPosition.transform)
                {
                    return true;
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::Exosuit), nameof(global::Exosuit.Update))]
        private static bool ExosuitUpdate(global::Exosuit __instance)
        {
            if (!Network.IsMultiplayerActive || IsLocalPiloting(__instance))
            {
                return true;
            }

            if (IsUsingByPlayer(__instance))
            {
                __instance.SetIKEnabled(true);

                if (__instance.armsDirty)
                {
                    __instance.UpdateExosuitArms();
                }

                return false;
            }

            return true;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::Exosuit), nameof(global::Exosuit.FixedUpdate))]
        private static bool ExosuitFixedUpdate(global::Exosuit __instance)
        {
            if (!Network.IsMultiplayerActive || __instance.docked || IsLocalPiloting(__instance))
            {
                return true;
            }

            return !__instance.useRigidbody.isKinematic;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::Exosuit), nameof(global::Exosuit.UpdateAnimations))]
        private static bool ExosuitUpdateAnimations(global::Exosuit __instance)
        {
            if (!Network.IsMultiplayerActive || IsLocalPiloting(__instance))
            {
                return true;
            }

            return !IsUsingByPlayer(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(global::Exosuit), nameof(global::Exosuit.ShouldSetKinematic))]
        private static void ShouldSetKinematic(global::Exosuit __instance, ref bool __result)
        {
            if (!Network.IsMultiplayerActive)
            {
                return;
            }

            if (IsLocalPiloting(__instance))
            {
                __result = false;
                return;
            }

            if (__result == false)
            {
                var uniqueId = Network.Identifier.GetIdentityId(__instance.gameObject);
                if (string.IsNullOrEmpty(uniqueId))
                {
                    uniqueId = __instance.pilotId;
                }

                var vehicle = Network.DynamicEntity.GetEntity(uniqueId);
                if (vehicle == null || !ZeroPlayer.IsPlayerMine(vehicle.OwnershipId))
                {
                    __result = true;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(global::ExosuitDrillArm), nameof(global::ExosuitDrillArm.Start))]
        private static bool ExosuitDrillArmStart(global::ExosuitDrillArm __instance)
        {
            if (!Network.IsMultiplayerActive)
            {
                return true;
            }

            __instance.StopEffects();
            return false;
        }

        /**
         *
         * Aracı döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static bool IsUsingByPlayer(global::Exosuit __instance)
        {
            var uniqueId = Network.Identifier.GetIdentityId(__instance.gameObject);
            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = __instance.pilotId;
            }

            var vehicle = Network.DynamicEntity.GetEntity(uniqueId);

            return vehicle != null && vehicle.IsUsingByPlayer;
        }
    }
}
