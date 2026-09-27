namespace Subnautica.Events.EventArgs
{
    using System;

    public class PetKilledAllEventArgs : EventArgs
    {
        /**
         *
         * Sınıf ayarlamalarını yapar
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public PetKilledAllEventArgs(string consoleId)
        {
            this.ConsoleId = consoleId;
        }

        /**
         *
         * ConsoleId Değerini barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public string ConsoleId { get; set; }
    }
}
