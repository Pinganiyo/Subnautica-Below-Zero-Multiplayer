namespace Subnautica.Events.EventArgs
{
    using System;

    public class PetSpawnedEventArgs : EventArgs
    {
        /**
         *
         * Sınıf ayarlamalarını yapar
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public PetSpawnedEventArgs(string fabricatorId, string petId, TechType techType, string petName)
        {
            this.FabricatorId = fabricatorId;
            this.PetId        = petId;
            this.TechType     = techType;
            this.PetName      = petName;
        }

        /**
         *
         * FabricatorId Değerini barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public string FabricatorId { get; set; }

        /**
         *
         * PetId Değerini barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public string PetId { get; set; }

        /**
         *
         * TechType Değerini barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public TechType TechType { get; set; }

        /**
         *
         * PetName Değerini barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public string PetName { get; set; }
    }
}
