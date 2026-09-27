namespace Subnautica.Events.EventArgs
{
    using System;

    public class PetKilledEventArgs : EventArgs
    {
        /**
         *
         * Sınıf ayarlamalarını yapar
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public PetKilledEventArgs(string petId, string petName)
        {
            this.PetId   = petId;
            this.PetName = petName;
        }

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
         * PetName Değerini barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public string PetName { get; set; }
    }
}
