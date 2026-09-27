namespace Subnautica.Events.Patches.Fixes.Game
{
    using HarmonyLib;
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using Subnautica.API.Features;

    [HarmonyPatch]
    public static class SubnauticaPetsConsoleFix
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Prefix(MonoBehaviour __instance)
        {
            try
            {
                if (__instance != null && __instance.transform.parent == null)
                {
                    var baseComp = __instance.GetComponentInParent<global::Base>();
                    if (baseComp == null)
                    {
                        baseComp = UnityEngine.Object.FindObjectsOfType<global::Base>()
                            .OrderBy(b => Vector3.Distance(b.transform.position, __instance.transform.position))
                            .FirstOrDefault();
                    }

                    if (baseComp != null)
                    {
                        __instance.transform.SetParent(baseComp.transform, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SubnauticaPetsConsoleFix.Prefix: {ex}");
            }
        }
    }

    [HarmonyPatch]
    public static class SubnauticaPetsConsoleUpdateFix
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetConsole");
            return type?.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Postfix(MonoBehaviour __instance)
        {
            try
            {
                var activeScreenField = __instance.GetType().GetField("activeScreen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var activeScreen = activeScreenField?.GetValue(__instance) as GameObject;
                if (activeScreen != null && !activeScreen.activeSelf)
                {
                    var isConstructedField = __instance.GetType().GetField("_isConstructed", BindingFlags.Instance | BindingFlags.NonPublic);
                    var hasPowerField = __instance.GetType().GetField("_hasPower", BindingFlags.Instance | BindingFlags.NonPublic);

                    isConstructedField?.SetValue(__instance, true);
                    hasPowerField?.SetValue(__instance, true);

                    var method = __instance.GetType().GetMethod("ConstructedOrPowerStateChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    method?.Invoke(__instance, null);
                }
            }
            catch
            {
            }
        }
    }

    [HarmonyPatch]
    public static class SubnauticaPetsFabricatorFix
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetFabricator");
            return type?.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        public static MethodBase TargetMethod()
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "SubnauticaPets_BZ");
            var type = asm?.GetType("DaftAppleGames.SubnauticaPets.BaseParts.PetFabricator");
            return type?.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static void Prefix(MonoBehaviour __instance)
        {
            try
            {
                if (__instance != null && __instance.transform.parent == null)
                {
                    var baseComp = __instance.GetComponentInParent<global::Base>();
                    if (baseComp == null)
                    {
                        baseComp = UnityEngine.Object.FindObjectsOfType<global::Base>()
                            .OrderBy(b => Vector3.Distance(b.transform.position, __instance.transform.position))
                            .FirstOrDefault();
                    }

                    if (baseComp != null)
                    {
                        __instance.transform.SetParent(baseComp.transform, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SubnauticaPetsFabricatorFix.Prefix: {ex}");
            }
        }
    }
}
