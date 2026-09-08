namespace Subnautica.Client.Modules
{
    using Subnautica.Events.EventArgs;
    using Subnautica.API.Features;

    using System.IO;
    using System;

    public class DiscordRichPresence
    {
        /**
         *
         * Sahne yüklendiğinde tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnSceneLoaded(SceneLoadedEventArgs ev)
        {
            if (ev.Scene.name == "XMenu")
            {
                Discord.UpdateRichPresence(ZeroLanguage.Get("DISCORD_STATUS_MAIN_MENU"), resetTime: true);
            }
        }
        
        /**
         *
         * Eklenti aktifleştiğinde tetiklenir.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public static void OnPluginEnabled()
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    var discordRpcFilePath = Paths.GetGameDependenciesPath("DiscordRPCNativeNamedPipe.dll");
                    if (File.Exists(discordRpcFilePath))
                    {
                        File.Copy(discordRpcFilePath, string.Format("{0}{1}DiscordRPCNativeNamedPipe.dll", Environment.CurrentDirectory, Paths.DS), true);    
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DiscordRichPresence.OnPluginEnabled: {ex.Message}");
            }
        }
    }
}