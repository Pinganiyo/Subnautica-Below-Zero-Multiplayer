namespace Subnautica.Network.Models.Server
{
    using MessagePack;

    using Subnautica.API.Enums;
    using Subnautica.Network.Models.Core;

    [MessagePackObject]
    public class PetKilledAllArgs : NetworkPacket
    {
        /**
         *
         * Ağ Paket Türü
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        [Key(0)]
        public override ProcessType Type { get; set; } = ProcessType.PetKilledAll;

        /**
         *
         * Evcil hayvan konsolunun kimlik değerini barındırır.
         *
         */
        [Key(5)]
        public string ConsoleId { get; set; }
    }
}
