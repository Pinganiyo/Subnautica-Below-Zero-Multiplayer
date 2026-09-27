namespace Subnautica.Events.Patches.Fixes.Game
{
    using HarmonyLib;
    using System;
    using System.Reflection;
    using System.Text;
    using Subnautica.API.Features;

    [HarmonyPatch]
    public static class NautilusTooltipFix
    {
        private static MethodInfo isDefinedByDefaultMethod;
        private static PropertyInfo extraItemInfoOptionProp;
        private static MethodInfo writeTechTypeMethod;

        public static MethodBase TargetMethod()
        {
            try
            {
                Type patcherType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    patcherType = asm.GetType("Nautilus.Patchers.TooltipPatcher");
                    if (patcherType != null)
                    {
                        break;
                    }
                }

                if (patcherType == null)
                {
                    return null;
                }

                extraItemInfoOptionProp = patcherType.GetProperty("ExtraItemInfoOption", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                writeTechTypeMethod = patcherType.GetMethod("WriteTechType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                Type enumExtType = patcherType.Assembly.GetType("Nautilus.Handlers.EnumExtensions");
                if (enumExtType != null)
                {
                    foreach (var m in enumExtType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (m.Name == "IsDefinedByDefault" && m.IsGenericMethodDefinition)
                        {
                            isDefinedByDefaultMethod = m.MakeGenericMethod(typeof(TechType));
                            break;
                        }
                    }
                }

                return patcherType.GetMethod("CustomTooltip", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            catch (Exception ex)
            {
                Log.Error($"NautilusTooltipFix.TargetMethod failed: {ex}");
                return null;
            }
        }

        [HarmonyPrefix]
        public static bool Prefix(StringBuilder sb, TechType techType)
        {
            try
            {
                bool isDefault = false;
                if (isDefinedByDefaultMethod != null)
                {
                    isDefault = (bool)isDefinedByDefaultMethod.Invoke(null, new object[] { techType });
                }
                else
                {
                    isDefault = Enum.IsDefined(typeof(TechType), techType);
                }

                if (isDefault)
                {
                    // Item is from the vanilla game (Below Zero).
                    // If user configured ModNameAndItemID (1), still show the ID:
                    if (extraItemInfoOptionProp != null && writeTechTypeMethod != null)
                    {
                        var opt = (int)extraItemInfoOptionProp.GetValue(null);
                        if (opt == 1)
                        {
                            writeTechTypeMethod.Invoke(null, new object[] { sb, techType });
                        }
                    }

                    // Skip Nautilus's CustomTooltip so "BelowZero" mod tag is NOT shown!
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"NautilusTooltipFix.Prefix failed: {ex}");
            }

            // Mod-added TechType: let Nautilus show the mod name as normal.
            return true;
        }
    }

    [HarmonyPatch]
    public static class SMLHelperTooltipFix
    {
        private static MethodInfo isVanillaTechTypeMethod;
        private static PropertyInfo extraItemInfoOptionProp;
        private static MethodInfo writeTechTypeMethod;

        public static MethodBase TargetMethod()
        {
            try
            {
                Type patcherType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    patcherType = asm.GetType("SMLHelper.V2.Patchers.TooltipPatcher");
                    if (patcherType != null)
                    {
                        break;
                    }
                }

                if (patcherType == null)
                {
                    return null;
                }

                extraItemInfoOptionProp = patcherType.GetProperty("ExtraItemInfoOption", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                writeTechTypeMethod = patcherType.GetMethod("WriteTechType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                isVanillaTechTypeMethod = patcherType.GetMethod("IsVanillaTechType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                return patcherType.GetMethod("CustomTooltip", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            }
            catch (Exception ex)
            {
                Log.Error($"SMLHelperTooltipFix.TargetMethod failed: {ex}");
                return null;
            }
        }

        [HarmonyPrefix]
        public static bool Prefix(StringBuilder sb, TechType techType)
        {
            try
            {
                bool isVanilla = false;
                if (isVanillaTechTypeMethod != null)
                {
                    isVanilla = (bool)isVanillaTechTypeMethod.Invoke(null, new object[] { techType });
                }
                else
                {
                    isVanilla = Enum.IsDefined(typeof(TechType), techType);
                }

                if (isVanilla)
                {
                    if (extraItemInfoOptionProp != null && writeTechTypeMethod != null)
                    {
                        var opt = (int)extraItemInfoOptionProp.GetValue(null);
                        if (opt == 1)
                        {
                            writeTechTypeMethod.Invoke(null, new object[] { sb, techType });
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SMLHelperTooltipFix.Prefix failed: {ex}");
            }

            return true;
        }
    }
}
