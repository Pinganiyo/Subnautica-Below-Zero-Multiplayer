namespace Subnautica.Client
{
    using BepInEx;
    using System;
    using UnityEngine;

    [BepInPlugin("com.subnautica.multiplayer", "Subnautica Below Zero Multiplayer", "1.0.0")]
    [BepInDependency("com.snmodding.nautilus", BepInDependency.DependencyFlags.SoftDependency)]
    public class BepInExPlugin : BaseUnityPlugin
    {
        private Subnautica.Events.Main eventsPlugin;
        private Subnautica.Client.Main clientPlugin;

        public void Awake()
        {
            try
            {
                RegisterModdedWorkbenchTab();

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

        private static void RegisterModdedWorkbenchTab()
        {
            try
            {
                var craftTreeHandler = System.Type.GetType("Nautilus.Handlers.CraftTreeHandler, Nautilus");
                if (craftTreeHandler != null)
                {
                    var addTabMethod = craftTreeHandler.GetMethod("AddTabNode", new Type[] {
                        typeof(CraftTree.Type), typeof(string), typeof(string), typeof(Sprite)
                    });
                    if (addTabMethod != null)
                    {
                        addTabMethod.Invoke(null, new object[] { CraftTree.Type.Workbench, "ModdedWorkbench", "Modded Modules", null });
                        Debug.Log("[SubnauticaMultiplayer] Registered 'ModdedWorkbench' tab for Nautilus Workbench.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SubnauticaMultiplayer] Could not register ModdedWorkbench tab: {ex.Message}");
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
