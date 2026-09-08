namespace Subnautica.Client
{
    using BepInEx;
    using System;
    using UnityEngine;

    [BepInPlugin("com.subnautica.multiplayer", "Subnautica Below Zero Multiplayer", "1.0.0")]
    public class BepInExPlugin : BaseUnityPlugin
    {
        private Subnautica.Events.Main eventsPlugin;
        private Subnautica.Client.Main clientPlugin;

        public void Awake()
        {
            try
            {
                Debug.Log("[SubnauticaMultiplayer] Initializing Events...");
                this.eventsPlugin = new Subnautica.Events.Main();
                this.eventsPlugin.OnEnabled();

                Debug.Log("[SubnauticaMultiplayer] Initializing Client...");
                this.clientPlugin = new Subnautica.Client.Main();
                this.clientPlugin.OnEnabled();

                Debug.Log("[SubnauticaMultiplayer] Mod loaded successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SubnauticaMultiplayer] Failed to initialize mod: {ex}");
            }
        }

        public void OnDestroy()
        {
            try
            {
                this.clientPlugin?.OnDisabled();
                this.eventsPlugin?.OnDisabled();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SubnauticaMultiplayer] Error during unload: {ex}");
            }
        }
    }
}
