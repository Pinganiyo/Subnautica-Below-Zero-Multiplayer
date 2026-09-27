namespace Subnautica.Events.Patches.Fixes.Game
{
    using HarmonyLib;
    using System;
    using Subnautica.API.Features;

    [HarmonyPatch(typeof(KnownTech), nameof(KnownTech.Initialize))]
    public static class KnownTechRetroactiveUnlock
    {
        private static void Postfix()
        {
            CheckPendingAnalysisTech();
        }

        public static void CheckPendingAnalysisTech()
        {
            try
            {
                if (KnownTech.analysisTech == null)
                {
                    return;
                }

                bool hasMoonpool = KnownTech.Contains(TechType.BaseMoonpool)
                                || KnownTech.Contains((TechType)1021)   // MoonpoolBlueprint
                                || KnownTech.Contains((TechType)10007)  // BaseMoonpoolExpansion
                                || (KnownTech.analyzedTech != null && KnownTech.analyzedTech.Contains(TechType.BaseMoonpool));

                foreach (var entry in KnownTech.analysisTech)
                {
                    if (entry == null || entry.unlockTechTypes == null)
                    {
                        continue;
                    }

                    bool parentKnown = KnownTech.Contains(entry.techType)
                                    || (KnownTech.analyzedTech != null && KnownTech.analyzedTech.Contains(entry.techType))
                                    || (entry.techType == TechType.BaseMoonpool && hasMoonpool);

                    if (parentKnown)
                    {
                        foreach (var unlock in entry.unlockTechTypes)
                        {
                            if (!KnownTech.Contains(unlock))
                            {
                                Log.Info($"[SubnauticaMultiplayer] Retroactively unlocking mod blueprint: {unlock} (Prerequisite {entry.techType} is known)");
                                KnownTech.Add(unlock, false, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"KnownTechRetroactiveUnlock.CheckPendingAnalysisTech: {ex}");
            }
        }
    }
}
