namespace Subnautica.Client.Synchronizations.Processors.General
{
    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.Core;
    using Subnautica.Events.EventArgs;
    using Subnautica.Events.Patches.Events.Game;
    using Subnautica.Network.Models.Core;

    using ServerModel = Subnautica.Network.Models.Server;

    public class PetKilledProcessor : NormalProcessor
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
            var packet = networkPacket.GetPacket<ServerModel.PetKilledArgs>();
            if (packet == null)
            {
                return false;
            }

            var petLiveMixin = Network.Identifier.GetComponentByGameObject<global::LiveMixin>(packet.PetId, true);
            if (petLiveMixin)
            {
                PetKillPatch.SuppressBroadcast = true;
                try
                {
                    petLiveMixin.TakeDamage(5000f);
                }
                finally
                {
                    PetKillPatch.SuppressBroadcast = false;
                }
            }

            return true;
        }

        /**
         *
         * Evcil hayvan öldürüldüğünde tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnPetKilled(PetKilledEventArgs ev)
        {
            ServerModel.PetKilledArgs request = new ServerModel.PetKilledArgs()
            {
                PetId   = ev.PetId,
                PetName = ev.PetName,
            };

            NetworkClient.SendPacket(request);
        }
    }
}
