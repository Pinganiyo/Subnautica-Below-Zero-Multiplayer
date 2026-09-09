namespace Subnautica.Client.Synchronizations.Processors.Player
{
    using System.Collections.Generic;

    using Subnautica.API.Features;
    using Subnautica.Client.Abstracts;
    using Subnautica.Client.Core;
    using Subnautica.Events.EventArgs;
    using Subnautica.Network.Models.Core;

    using ServerModel = Subnautica.Network.Models.Server;

    public class ConsoleCommandProcessor : NormalProcessor
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
            return true;
        }

        /**
         *
         * Komut kullanıldığında tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnUsingCommand(PlayerUsingCommandEventArgs ev)
        {
            if (ev.Command.Equals("suitcolor", System.StringComparison.OrdinalIgnoreCase) || ev.Command.Equals("color", System.StringComparison.OrdinalIgnoreCase))
            {
                ev.IsAllowed = false;
                HandleSuitColorCommand(ev.FullCommand);
                return;
            }

            if (ev.Command.Equals("haircolor", System.StringComparison.OrdinalIgnoreCase) || ev.Command.Equals("hair", System.StringComparison.OrdinalIgnoreCase))
            {
                ev.IsAllowed = false;
                HandleHairColorCommand(ev.FullCommand);
                return;
            }

            if (IsDeveloperModeOn() || AllowedCommands.Contains(ev.Command))
            {
                ServerModel.PlayerConsoleCommandArgs request = new ServerModel.PlayerConsoleCommandArgs()
                {
                    Command = ev.FullCommand,
                };

                NetworkClient.SendPacket(request);
            }
            else
            {
                ev.IsAllowed = false;

                ErrorMessage.AddMessage("This command has been disabled.");
            }
        }

        /**
         *
         * Handles the suitcolor command.
         *
         */
        private static void HandleSuitColorCommand(string fullCommand)
        {
            var parts = fullCommand.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || parts[1].Equals("list", System.StringComparison.OrdinalIgnoreCase) || parts[1].Equals("help", System.StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage.AddMessage("Available Suit Colors (use /suitcolor <name|number> or press F6):");
                foreach (var item in Subnautica.Client.MonoBehaviours.Player.PlayerSuitCustomizer.Palette)
                {
                    ErrorMessage.AddMessage($"{item.Index}: {item.Name}");
                }
                return;
            }

            string query = parts[1];
            if (Subnautica.Client.MonoBehaviours.Player.PlayerSuitCustomizer.TryFindColorOption(query, out var option))
            {
                Subnautica.Client.MonoBehaviours.Player.PlayerSuitCustomizer.SetLocalSuitColor(option.Index);
            }
            else
            {
                ErrorMessage.AddMessage($"Unknown color: '{query}'. Type 'suitcolor list' to see all options.");
            }
        }

        /**
         *
         * Handles the haircolor command.
         *
         */
        private static void HandleHairColorCommand(string fullCommand)
        {
            var parts = fullCommand.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || parts[1].Equals("list", System.StringComparison.OrdinalIgnoreCase) || parts[1].Equals("help", System.StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage.AddMessage("Available Hair Colors (use /haircolor <name|number> or press F7):");
                foreach (var item in Subnautica.Client.MonoBehaviours.Player.PlayerSuitCustomizer.HairPalette)
                {
                    ErrorMessage.AddMessage($"{item.Index}: {item.Name}");
                }
                return;
            }

            string query = parts[1];
            if (Subnautica.Client.MonoBehaviours.Player.PlayerSuitCustomizer.TryFindHairColorOption(query, out var option))
            {
                Subnautica.Client.MonoBehaviours.Player.PlayerSuitCustomizer.SetLocalHairColor(option.Index);
            }
            else
            {
                ErrorMessage.AddMessage($"Unknown hair color: '{query}'. Type 'haircolor list' to see all options.");
            }
        }

        /**
         *
         * Geliştirici modu aktif mi?
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static bool IsDeveloperModeOn()
        {
            return Tools.GetLoggedInName().Contains("BOT Benson") || Tools.GetLoggedInName().Contains("ismail0234");
        }

        /**
         *
         * Kullanılabilen komutlar listesi
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        private static List<string> AllowedCommands { get; set; } = new List<string>()
        {
            "item",
            "clearinventory",
            "fastbuild",
            "kill",
            "nocost",
            "unlock",
            "unlockall",
            "unlockallbuildables",
            "niceloot",
            "madloot",
            "bobthebuilder",
            "charge",
            "fastswim",
            "rebuildbase",
            "precursorkeys",
            "seatruckupgrades",
            "tools",
            "vehicleupgrades",
            "fly",
            "cold",
            "oxygen",
            "ency",
            "fps",
            "collect",
            "dbc",
            "weathergui",
            "biome",
            "goto",
            "warp",
            "batch",
            "chunk",
            "gotofast",
            "ghost",
            "spawnnearby",
            "warpforward",
            "food",
            "water",
            "nohunger",
            "nothirst",
        };
    }
}