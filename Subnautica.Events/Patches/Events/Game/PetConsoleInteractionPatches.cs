namespace Subnautica.Events.Patches.Events.Game
{
    using HarmonyLib;

    using Subnautica.API.Extensions;
    using Subnautica.API.Features;
    using Subnautica.Events.EventArgs;
    using Subnautica.Events.Handlers;

    using System;
    using System.Linq;
    using System.Reflection;

    using UnityEngine;

    /**
     *
     * SubnauticaPets evcil hayvan etkileşimlerini çok oyunculu ağa yansıtır.
     *
     */
    [HarmonyPatch]
    public static class PetConsoleRenamePatch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("RenamePet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("RenamePet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Postfix(object __instance, object[] __args)
        {
            try
            {
                if (!Network.IsMultiplayerActive || __args == null)
                {
                    return;
                }

                GameObject petGameObject = null;
                string newName = null;

                foreach (var arg in __args)
                {
                    if (newName == null && arg is string name)
                    {
                        newName = name;
                    }
                    else if (petGameObject == null)
                    {
                        petGameObject = arg as GameObject ?? (arg as Component)?.gameObject;
                    }
                }

                if (petGameObject == null || string.IsNullOrEmpty(newName))
                {
                    return;
                }

                var petId = petGameObject.GetIdentityId();
                if (petId.IsNull())
                {
                    return;
                }

                Handlers.Game.OnPetRenamed(new PetRenamedEventArgs(petId, newName));
            }
            catch (Exception ex)
            {
                Log.Error($"Game.PetConsoleRenamePatch: {ex}");
            }
        }
    }

    /**
     *
     * SubnauticaPets evcil hayvan öldürme etkileşimini çok oyunculu ağa yansıtır.
     *
     */
    [HarmonyPatch]
    public static class PetConsoleKillPatch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("KillPet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("KillPet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Postfix(object __instance, object[] __args)
        {
            try
            {
                if (!Network.IsMultiplayerActive || __args == null)
                {
                    return;
                }

                GameObject petGameObject = null;
                string petName = null;

                foreach (var arg in __args)
                {
                    if (petName == null && arg is string name)
                    {
                        petName = name;
                    }
                    else if (petGameObject == null)
                    {
                        petGameObject = arg as GameObject ?? (arg as Component)?.gameObject;
                    }
                }

                if (petGameObject == null)
                {
                    return;
                }

                var petId = petGameObject.GetIdentityId();
                if (petId.IsNull())
                {
                    return;
                }

                Handlers.Game.OnPetKilled(new PetKilledEventArgs(petId, petName ?? petGameObject.name));
            }
            catch (Exception ex)
            {
                Log.Error($"Game.PetConsoleKillPatch: {ex}");
            }
        }
    }

    /**
     *
     * SubnauticaPets tüm evcil hayvanları öldürme etkileşimini çok oyunculu ağa yansıtır.
     *
     */
    [HarmonyPatch]
    public static class PetConsoleKillAllPatch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("KillAll", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null
                || type?.GetMethod("KillAllPets", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");

            return type?.GetMethod("KillAll", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? type?.GetMethod("KillAllPets", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Postfix(object __instance)
        {
            try
            {
                if (!Network.IsMultiplayerActive)
                {
                    return;
                }

                var consoleComponent = __instance as Component;
                if (consoleComponent == null)
                {
                    return;
                }

                var consoleId = consoleComponent.gameObject.GetIdentityId();
                if (consoleId.IsNull())
                {
                    return;
                }

                Handlers.Game.OnPetKilledAll(new PetKilledAllEventArgs(consoleId));
            }
            catch (Exception ex)
            {
                Log.Error($"Game.PetConsoleKillAllPatch: {ex}");
            }
        }
    }
}
