namespace Subnautica.Client.Synchronizations.Processors.Player
{
    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.Extensions;
    using Subnautica.Network.Models.Core;

    using ClientModel = Subnautica.Network.Models.Client;

    public class AnotherPlayerConnectedProcessor : NormalProcessor
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
            var packet = networkPacket.GetPacket<ClientModel.AnotherPlayerConnectedArgs>();
            if (packet.UniqueId.IsNull())
            {
                return false;
            }
            
            var player = ZeroPlayer.CreateOrGetPlayerByUniqueId(packet.UniqueId, packet.PlayerId);
            player.SetPlayerName(packet.PlayerName);
            player.SetSubRootId(packet.SubrootId);
            player.SetInteriorId(packet.InteriorId);
            player.SuitColor = packet.SuitColor;
            player.HairColor = packet.HairColor;
            
            if (!player.IsCreatedModel)
            {
                player.CreateModel(packet.Position.ToVector3(true), packet.Rotation.ToQuaternion(true));
                player.InitBehaviours();
            }
            else if (player.PlayerModel != null)
            {
                Subnautica.Client.MonoBehaviours.Player.PlayerSuitCustomizer.ApplyCustomization(player.PlayerModel, player.PlayerId);
            }

            Discord.UpdateRichPresence(null, ZeroLanguage.GetServerPlayerCount());
            return true;
        }
    }
}
