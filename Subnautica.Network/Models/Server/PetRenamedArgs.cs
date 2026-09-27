namespace Subnautica.Network.Models.Server
{
    using MessagePack;

    using Subnautica.API.Enums;
    using Subnautica.Network.Models.Core;

    [MessagePackObject]
    public class PetRenamedArgs : NetworkPacket
    {
        /**
         *
         * Ağ Paket Türü
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        [Key(0)]
        public override ProcessType Type { get; set; } = ProcessType.PetRenamed;

        /**
         *
         * Evcil hayvanın kimlik değerini barındırır.
         *
         */
        [Key(5)]
        public string PetId { get; set; }

        /**
         *
         * Evcil hayvanın yeni ismini barındırır.
         *
         */
        [Key(6)]
        public string NewName { get; set; }
    }
}
