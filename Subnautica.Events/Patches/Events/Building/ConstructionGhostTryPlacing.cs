namespace Subnautica.Events.Patches.Events.Building
{
    using HarmonyLib;

    using Subnautica.API.Features;
    using Subnautica.Events.EventArgs;

    using System;
    using UnityEngine;

    [HarmonyPatch(typeof(Builder), nameof(Builder.TryPlace))]
    public class ConstructionGhostTryPlacing
    {
        /**
         *
         * Fonksiyonu yamalar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static bool Prefix()
        {
            if (!Network.IsMultiplayerActive)
            {
                return true;
            }

            try
            {
                string subrootId = null;
                var position = Builder.placePosition;
                var rotation = Builder.placeRotation;

                if (global::Player.main.GetCurrentSub() != null)
                {
                    subrootId = Network.Identifier.GetIdentityId(global::Player.main.GetCurrentSub().gameObject, false);
                }
                else
                {
                    global::SeaTruckSegment segment = null;
                    if (global::Player.main.currentInterior != null && global::Player.main.currentInterior.GetGameObject()?.TryGetComponent<global::SeaTruckSegment>(out var interiorSeg) == true)
                    {
                        segment = interiorSeg;
                    }
                    else if (Builder.placementTarget != null)
                    {
                        segment = Builder.placementTarget.GetComponentInParent<global::SeaTruckSegment>();
                    }

                    if (segment != null)
                    {
                        subrootId = Network.Identifier.GetIdentityId(segment.gameObject, false);
                        position = segment.transform.InverseTransformPoint(Builder.placePosition);
                        rotation = Quaternion.Inverse(segment.transform.rotation) * Builder.placeRotation;
                    }
                }

                ConstructionGhostTryPlacingEventArgs args = new ConstructionGhostTryPlacingEventArgs(
                    Builder.ghostModel,
                    Network.Identifier.GetIdentityId(Builder.ghostModel), 
                    subrootId, 
                    Builder.lastTechType, 
                    Builder.lastRotation, 
                    position, 
                    rotation, 
                    Builder.GetAimTransform(), 
                    Builder.canPlace,
                    Builder.ghostModel.GetComponentInParent<ConstructableBase>(),
                    Builder.prefab == null || Builder.canPlace == false
                );

                Handlers.Building.OnConstructingGhostTryPlacing(args);

                if (!args.IsAllowed)
                {
                    Builder.End();
                }

                return args.IsAllowed;
            }
            catch (Exception e)
            {
                Log.Error($"ConstructingGhostTryPlacing.Prefix: {e}\n{e.StackTrace}");
                return true;
            }
        }
    }
}
