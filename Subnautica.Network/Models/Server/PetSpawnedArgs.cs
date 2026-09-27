namespace Subnautica.Network.Models.Server
{
    using MessagePack;

    using Subnautica.API.Enums;
    using Subnautica.Network.Models.Core;

    [MessagePackObject]
    public class PetSpawnedArgs : NetworkPacket
    {
        /**
         *
         * Ağ Paket Türü
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        [Key(0)]
        public override ProcessType Type { get; set; } = ProcessType.PetSpawned;

        /**
         *
         * Evcil hayvan üreticisinin kimlik değerini barındırır.
         *
         */
        [Key(5)]
        public string FabricatorId { get; set; }

        /**
         *
         * Evcil hayvanın kimlik değerini barındırır.
         *
         */
        [Key(6)]
        public string PetId { get; set; }

        /**
         *
         * Evcil hayvanın türünü barındırır.
         *
         */
        [Key(7)]
        public TechType TechType { get; set; }

        /**
         *
         * Evcil hayvanın ismini barındırır.
         *
         */
        [Key(8)]
        public string PetName { get; set; }
    }
}
