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
     * SubnauticaPets evcil hayvan yeniden adlandırma etkileşimini çok oyunculu ağa yansıtır.
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
            return type?.GetMethod("RenameButtonHandler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("RenameButtonHandler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Prefix(object __instance)
        {
            try
            {
                if (!Network.IsMultiplayerActive || __instance == null)
                {
                    return;
                }

                var instanceType = __instance.GetType();
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var selectedPet = instanceType.GetField("_selectedPet", flags)?.GetValue(__instance) as Component;
                var newName = instanceType.GetField("_petNameText", flags)?.GetValue(__instance) as string;

                if (selectedPet == null || string.IsNullOrEmpty(newName))
                {
                    return;
                }

                var petId = selectedPet.gameObject.GetIdentityId();
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
     * Tekil öldürme ve toplu öldürme akışlarının tamamı Pet.Kill üzerinden geçtiği için
     * tek yakalama noktası yeterlidir.
     *
     */
    [HarmonyPatch]
    public static class PetKillPatch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.Pets.Pet");
            return type?.GetMethod("Kill", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.Pets.Pet");
            return type?.GetMethod("Kill", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Postfix(object __instance)
        {
            try
            {
                if (!Network.IsMultiplayerActive)
                {
                    return;
                }

                var petComponent = __instance as Component;
                if (petComponent == null)
                {
                    return;
                }

                var petId = petComponent.gameObject.GetIdentityId();
                if (petId.IsNull())
                {
                    return;
                }

                var petName = petComponent.gameObject.name;
                try
                {
                    var nameProperty = __instance.GetType().GetProperty("PetName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var nameValue = nameProperty?.GetValue(__instance) as string;
                    if (!string.IsNullOrEmpty(nameValue))
                    {
                        petName = nameValue;
                    }
                }
                catch
                {
                }

                Handlers.Game.OnPetKilled(new PetKilledEventArgs(petId, petName));
            }
            catch (Exception ex)
            {
                Log.Error($"Game.PetKillPatch: {ex}");
            }
        }
    }

    /**
     *
     * SubnauticaPets evcil hayvan üretimini çok oyunculu ağa yansıtır.
     * Üretim zaman uyumsuz olduğu için geri çağrı sarmalanır ve
     * üretilen evcil hayvanın kimliğiyle birlikte yayın yapılır.
     *
     */
    [HarmonyPatch]
    public static class PetFabricatorSpawnPatch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetFabricator");
            return type?.GetMethod("SpawnPet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetFabricator");
            return type?.GetMethod("SpawnPet", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Prefix(object __instance, TechType techType, ref Action<GameObject> callBack)
        {
            try
            {
                if (!Network.IsMultiplayerActive)
                {
                    return;
                }

                var fabricatorComponent = __instance as Component;
                if (fabricatorComponent == null)
                {
                    return;
                }

                var fabricatorId = fabricatorComponent.gameObject.GetIdentityId();
                if (fabricatorId.IsNull())
                {
                    return;
                }

                var originalCallback = callBack;
                callBack = (spawnedPet) =>
                {
                    try
                    {
                        if (Network.IsMultiplayerActive && spawnedPet != null)
                        {
                            var petId = spawnedPet.GetIdentityId();
                            if (!petId.IsNull())
                            {
                                var petName = spawnedPet.name;
                                try
                                {
                                    var petComponent = spawnedPet.GetComponent("Pet");
                                    var nameProperty = petComponent?.GetType().GetProperty("PetName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    var nameValue = nameProperty?.GetValue(petComponent) as string;
                                    if (!string.IsNullOrEmpty(nameValue))
                                    {
                                        petName = nameValue;
                                    }
                                }
                                catch
                                {
                                }

                                Handlers.Game.OnPetSpawned(new PetSpawnedEventArgs(fabricatorId, petId, techType, petName));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Game.PetFabricatorSpawnPatch.Callback: {ex}");
                    }
                    finally
                    {
                        try
                        {
                            originalCallback?.Invoke(spawnedPet);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Game.PetFabricatorSpawnPatch.OriginalCallback: {ex}");
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Game.PetFabricatorSpawnPatch: {ex}");
            }
        }
    }
}
