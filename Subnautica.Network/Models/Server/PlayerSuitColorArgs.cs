namespace Subnautica.Network.Models.Server
{
    using LiteNetLib;
    using MessagePack;
    using Subnautica.API.Enums;
    using Subnautica.Network.Models.Core;

    [MessagePackObject]
    public class PlayerSuitColorArgs : NetworkPacket
    {
        /**
         * Ağ Paket Türü
         */
        [Key(0)]
        public override ProcessType Type { get; set; } = ProcessType.PlayerSuitColor;

        /**
         * Packet Kanal Türü
         */
        [Key(1)]
        public override NetworkChannel ChannelType { get; set; } = NetworkChannel.PlayerAnimation;

        /**
         * Packet Teslim Türü
         */
        [Key(2)]
        public override DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.ReliableOrdered;

        /**
         * Suit Color Index
         */
        [Key(5)]
        public byte SuitColor { get; set; }

        /**
         * Hair Color Index
         */
        [Key(6)]
        public byte HairColor { get; set; } = 0;
    }
}
