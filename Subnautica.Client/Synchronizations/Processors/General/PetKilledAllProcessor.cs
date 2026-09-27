namespace Subnautica.Client.Synchronizations.Processors.General
{
    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.Core;
    using Subnautica.Events.EventArgs;
    using Subnautica.Network.Models.Core;

    using UnityEngine;

    using ServerModel = Subnautica.Network.Models.Server;

    public class PetKilledAllProcessor : NormalProcessor
    {
        /**
         *
         * Gelen veriyi işler
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public override bool OnDataReceived(NetworkPacket networkPacket)
        {
            var packet = networkPacket.GetPacket<ServerModel.PetKilledAllArgs>();
            if (packet == null)
            {
                return false;
            }

            var consoleGameObject = Network.Identifier.GetGameObject(packet.ConsoleId, true);
            if (consoleGameObject == null)
            {
                return true;
            }

            var petConsole = consoleGameObject.GetComponent("PetConsole") as Component;
            if (petConsole == null)
            {
                return true;
            }

            try
            {
                var petListField = petConsole.GetType().GetField("_pets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                var petList = petListField?.GetValue(petConsole) as System.Collections.IEnumerable;
                if (petList == null)
                {
                    return true;
                }

                foreach (var pet in petList)
                {
                    var petGameObject = pet as GameObject ?? (pet as Component)?.gameObject;
                    if (petGameObject == null)
                    {
                        continue;
                    }

                    var petLiveMixin = petGameObject.GetComponent<global::LiveMixin>();
                    if (petLiveMixin)
                    {
                        petLiveMixin.TakeDamage(5000f);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Warn($"PetKilledAllProcessor.OnDataReceived -> Exception: {ex.Message}");
            }

            return true;
        }

        /**
         *
         * Tüm evcil hayvanlar öldürüldüğünde tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnPetKilledAll(PetKilledAllEventArgs ev)
        {
            ServerModel.PetKilledAllArgs request = new ServerModel.PetKilledAllArgs()
            {
                ConsoleId = ev.ConsoleId,
            };

            NetworkClient.SendPacket(request);
        }
    }
}
