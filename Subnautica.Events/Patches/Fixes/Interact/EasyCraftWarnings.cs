namespace Subnautica.Events.Patches.Fixes.Interact
{
    using HarmonyLib;

    using Subnautica.API.Features;

    using System;
    using System.Reflection;

    /// <summary>
    /// Suppresses EasyCraft's "No Power!" and "Don't Have Needed Ingredients"
    /// warning toasts in multiplayer. Applied manually at runtime so that the
    /// patch is simply skipped when EasyCraft is not installed.
    /// </summary>
    public static class EasyCraftWarnings
    {
        private static bool _applied = false;

        /**
         * Called from the main patch initializer after all class-based Harmony
         * patches have been applied. Finds EasyCraft.Main.ShowMessage at runtime
         * and patches it only when EasyCraft is actually loaded.
         */
        public static void Apply(Harmony harmony)
        {
            if (_applied)
            {
                return;
            }

            try
            {
                Type easyCraftMain = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.GetName().Name == "EasyCraft_BZ")
                    {
                        easyCraftMain = asm.GetType("EasyCraft.Main");
                        break;
                    }
                }

                if (easyCraftMain == null)
                {
                    Log.Info("EasyCraftWarnings: EasyCraft_BZ not found — skipping ShowMessage suppression.");
                    return;
                }

                var showMessage = easyCraftMain.GetMethod(
                    "ShowMessage",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(string) },
                    null);

                if (showMessage == null)
                {
                    Log.Warn("EasyCraftWarnings: EasyCraft.Main.ShowMessage not found.");
                    return;
                }

                var prefix = typeof(EasyCraftWarnings).GetMethod(
                    nameof(ShowMessagePrefix),
                    BindingFlags.NonPublic | BindingFlags.Static);

                harmony.Patch(showMessage, prefix: new HarmonyMethod(prefix));
                _applied = true;

                Log.Info("EasyCraftWarnings: ShowMessage suppression patched successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"EasyCraftWarnings.Apply failed: {ex}");
            }
        }

        /**
         * Prefix for EasyCraft.Main.ShowMessage.
         * Returns false to skip the original (suppress the warning) in multiplayer.
         */
        private static bool ShowMessagePrefix(ref bool __result)
        {
            if (!Network.IsMultiplayerActive)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}
