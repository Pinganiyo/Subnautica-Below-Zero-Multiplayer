namespace Subnautica.Network.Models.Server
{
    using MessagePack;

    using Subnautica.API.Enums;
    using Subnautica.Network.Models.Core;

    [MessagePackObject]
    public class PetKilledArgs : NetworkPacket
    {
        /**
         *
         * Ağ Paket Türü
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        [Key(0)]
        public override ProcessType Type { get; set; } = ProcessType.PetKilled;

        /**
         *
         * Evcil hayvanın kimlik değerini barındırır.
         *
         */
        [Key(5)]
        public string PetId { get; set; }

        /**
         *
         * Evcil hayvanın ismini barındırır.
         *
         */
        [Key(6)]
        public string PetName { get; set; }
    }
}
