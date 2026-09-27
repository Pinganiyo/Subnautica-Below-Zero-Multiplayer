namespace Subnautica.Events.Patches.Fixes.Interact
{
    using HarmonyLib;

    using Subnautica.API.Features;

    [HarmonyPatch(typeof(global::DockedVehicleHandTarget), nameof(global::DockedVehicleHandTarget.OnHandHover))]
    public class DockedVehicleHandTarget
    {
        /**
         *
         * Fonksiyonu yamalar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static bool Prefix(global::DockedVehicleHandTarget __instance)
        {
            if (!Network.IsMultiplayerActive)
            {
                return true;
            }

            if (IsBlocked(__instance))
            {
                Interact.ShowUseDenyMessage();
                return false;
            }

            return true;
        }

        /**
         *
         * Bloklanma durumunu döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static bool IsBlocked(global::DockedVehicleHandTarget __instance)
        {
            if (__instance.dockingBay.GetDockedObject() != null)
            {
                var dockedObject = __instance.dockingBay.GetDockedObject();

                // A docked Exosuit is always unoccupied, so any interact block on it is stale.
                // Never deny hovering it: the server clears the block and mounts on entry.
                if (!dockedObject.TryGetComponent<global::Exosuit>(out _))
                {
                    if (Interact.IsBlocked(Network.Identifier.GetIdentityId(dockedObject.gameObject, false)))
                    {
                        return true;
                    }
                }
            }

            var baseDeconstructable = __instance.gameObject.GetComponentInParent<BaseDeconstructable>();
            if (baseDeconstructable)
            {
                return Interact.IsBlocked(Network.Identifier.GetIdentityId(baseDeconstructable.gameObject, false));
            }

            return false;
        }
    }
}
