namespace Subnautica.Client.Synchronizations.Processors.Player
{
    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.MonoBehaviours.Player;
    using Subnautica.Network.Models.Core;
    using ServerModel = Subnautica.Network.Models.Server;

    public class PlayerSuitColorProcessor : NormalProcessor
    {
        /**
         * Gelen veriyi işler
         */
        public override bool OnDataReceived(NetworkPacket networkPacket)
        {
            var packet = networkPacket.GetPacket<ServerModel.PlayerSuitColorArgs>();
            if (packet == null || packet.GetPacketOwnerId() == 0)
            {
                return false;
            }

            var player = ZeroPlayer.GetPlayerById(packet.GetPacketOwnerId());
            if (player != null && !player.IsMine)
            {
                player.SuitColor = packet.SuitColor;
                player.HairColor = packet.HairColor;
                if (player.PlayerModel != null)
                {
                    PlayerSuitCustomizer.ApplyCustomization(player.PlayerModel, player.PlayerId);
                }
            }

            return true;
        }
    }
}
