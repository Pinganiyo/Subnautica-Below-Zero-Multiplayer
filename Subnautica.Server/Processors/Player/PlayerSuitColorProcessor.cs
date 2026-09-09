namespace Subnautica.Server.Processors.Player
{
    using Server.Core;
    using Subnautica.Network.Models.Core;
    using Subnautica.Server.Abstracts.Processors;
    using ServerModel = Subnautica.Network.Models.Server;

    public class PlayerSuitColorProcessor : NormalProcessor
    {
        /**
         * Gelen veriyi işler
         */
        public override bool OnExecute(AuthorizationProfile profile, NetworkPacket networkPacket)
        {
            var packet = networkPacket.GetPacket<ServerModel.PlayerSuitColorArgs>();
            if (packet == null)
            {
                return this.SendEmptyPacketErrorLog(networkPacket);
            }

            profile.SuitColor = packet.SuitColor;
            profile.HairColor = packet.HairColor;
            profile.SendPacketToOtherClients(packet);
            return true;
        }
    }
}
