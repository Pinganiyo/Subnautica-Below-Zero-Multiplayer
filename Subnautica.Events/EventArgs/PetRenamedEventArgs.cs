namespace Subnautica.Events.EventArgs
{
    using System;

    public class PetRenamedEventArgs : EventArgs
    {
        /**
         *
         * Sınıf ayarlamalarını yapar
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public PetRenamedEventArgs(string petId, string newName)
        {
            this.PetId   = petId;
            this.NewName = newName;
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
         * NewName Değerini barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public string NewName { get; set; }
    }
}
