namespace Subnautica.Events.Patches.Fixes.Game
{
    using HarmonyLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Subnautica.API.Features;

    [HarmonyPatch(typeof(KnownTech), nameof(KnownTech.Initialize))]
    public static class KnownTechRetroactiveUnlock
    {
        private static readonly HashSet<string> RegisteredNautilusNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static void Postfix()
        {
            CheckPendingAnalysisTech();
        }

        public static void CheckPendingAnalysisTech()
        {
            try
            {
                if (KnownTech.analysisTech != null)
                {
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

                // Explicit Seatruck Depth Upgrades unlock checks
                UnlockSeaTruckDepthUpgrades();

                // Ensure Seatruck Depth Upgrades are in the craft trees (Workbench root, SeamothUpgrades, Fabricator)
                EnsureSeaTruckUpgradesCraftNodes();

                // Subnautica Pets & standalone mod blueprints
                UnlockModBlueprintIfFound("PetFabricator");
                UnlockModBlueprintIfFound("PetConsole");
                UnlockModBlueprintIfFound("PetFabricatorFragment");
                UnlockModBlueprintIfFound("PetConsoleFragment");
                UnlockSubnauticaPetsExtras();
            }
            catch (Exception ex)
            {
                Log.Error($"KnownTechRetroactiveUnlock.CheckPendingAnalysisTech: {ex}");
            }
        }

        private static void UnlockSeaTruckDepthUpgrades()
        {
            bool hasMoonpool = KnownTech.Contains(TechType.BaseMoonpool)
                            || KnownTech.Contains((TechType)1021)
                            || KnownTech.Contains((TechType)10007)
                            || (KnownTech.analyzedTech != null && KnownTech.analyzedTech.Contains(TechType.BaseMoonpool));

            if (hasMoonpool)
            {
                UnlockModBlueprintIfFound("SeaTruckDepthMK4");
            }

            bool hasSeatruckFab = KnownTech.Contains(TechType.SeaTruckFabricator)
                               || (KnownTech.analyzedTech != null && KnownTech.analyzedTech.Contains(TechType.SeaTruckFabricator));

            if (hasSeatruckFab || hasMoonpool)
            {
                UnlockModBlueprintIfFound("SeaTruckDepthMK5");
            }

            bool hasTorpedoArm = KnownTech.Contains(TechType.ExosuitTorpedoArmModule)
                              || (KnownTech.analyzedTech != null && KnownTech.analyzedTech.Contains(TechType.ExosuitTorpedoArmModule));

            if (hasTorpedoArm || hasMoonpool)
            {
                UnlockModBlueprintIfFound("SeaTruckDepthMK6");
            }
        }

        public static void EnsureSeaTruckUpgradesCraftNodes()
        {
            EnsureModdedCraftNode("SeaTruckDepthMK4");
            EnsureModdedCraftNode("SeaTruckDepthMK5");
            EnsureModdedCraftNode("SeaTruckDepthMK6");
        }

        private static void EnsureModdedCraftNode(string techTypeName)
        {
            try
            {
                if (TechTypeExtensions.FromString(techTypeName, out TechType techType, true) && techType != TechType.None)
                {
                    // 1. Register with Nautilus handler
                    AddToNautilusCraftTree(CraftTree.Type.Workbench, techType);
                    AddToNautilusCraftTree(CraftTree.Type.SeamothUpgrades, techType, "SeaTruckUpgrade");
                    AddToNautilusCraftTree(CraftTree.Type.Fabricator, techType, "Upgrades", "SeatruckUpgrades");

                    // 2. Inject into live in-memory CraftTree instances
                    InjectIntoActiveCraftTree(CraftTree.workbench, techType, null);
                    InjectIntoActiveCraftTree(CraftTree.seamothUpgrades, techType, "SeaTruckUpgrade");
                    InjectIntoActiveCraftTree(CraftTree.fabricator, techType, "SeatruckUpgrades");
                    InjectIntoActiveCraftTree(CraftTree.seatruckFabricator, techType, "SeatruckUpgrades");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"EnsureModdedCraftNode({techTypeName}): {ex}");
            }
        }

        private static void AddToNautilusCraftTree(CraftTree.Type treeType, TechType techType, params string[] steps)
        {
            try
            {
                string key = $"{treeType}:{techType}:{string.Join("/", steps ?? Array.Empty<string>())}";
                if (!RegisteredNautilusNodes.Add(key))
                {
                    return;
                }

                var nautilusAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Nautilus");
                if (nautilusAssembly != null)
                {
                    var handlerType = nautilusAssembly.GetType("Nautilus.Handlers.CraftTreeHandler");
                    if (handlerType != null)
                    {
                        var method = handlerType.GetMethod("AddCraftingNode", new Type[] { typeof(CraftTree.Type), typeof(TechType), typeof(string[]) });
                        if (method != null)
                        {
                            method.Invoke(null, new object[] { treeType, techType, steps ?? Array.Empty<string>() });
                            Log.Info($"[SubnauticaMultiplayer] Nautilus AddCraftingNode: {techType} in {treeType}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"AddToNautilusCraftTree failed for {techType} in {treeType}: {ex}");
            }
        }

        private static bool ContainsNode(CraftNode root, TechType techType)
        {
            if (root == null)
            {
                return false;
            }

            var enumerator = root.Traverse(true);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null && enumerator.Current.action == TreeAction.Craft && enumerator.Current.techType0 == techType)
                {
                    return true;
                }
            }

            return false;
        }

        private static CraftNode FindBranch(CraftNode root, string branchId)
        {
            if (root == null || string.IsNullOrEmpty(branchId))
            {
                return null;
            }

            var enumerator = root.Traverse(true);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null && enumerator.Current.id != null && enumerator.Current.id.Equals(branchId, StringComparison.OrdinalIgnoreCase))
                {
                    return enumerator.Current;
                }
            }

            return null;
        }

        private static void InjectIntoActiveCraftTree(CraftTree tree, TechType techType, string targetBranchId)
        {
            if (tree == null || tree.nodes == null)
            {
                return;
            }

            CraftNode targetBranch = tree.nodes;
            if (!string.IsNullOrEmpty(targetBranchId))
            {
                targetBranch = FindBranch(tree.nodes, targetBranchId);
            }

            if (targetBranch == null)
            {
                return;
            }

            if (!ContainsNode(targetBranch, techType))
            {
                targetBranch.AddNode(new CraftNode(techType.AsString(), TreeAction.Craft, techType));
                CraftTree.craftableTech?.Add(techType);
                Log.Info($"[SubnauticaMultiplayer] Injected {techType} into active CraftTree under '{targetBranch.id}'");
            }
        }

        private static void UnlockModBlueprintIfFound(string techTypeName)
        {
            try
            {
                if (TechTypeExtensions.FromString(techTypeName, out TechType techType, true))
                {
                    if (techType != TechType.None && !KnownTech.Contains(techType))
                    {
                        Log.Info($"[SubnauticaMultiplayer] Retroactively unlocking mod blueprint: {techTypeName} ({techType})");
                        KnownTech.Add(techType, false, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"KnownTechRetroactiveUnlock.UnlockModBlueprintIfFound({techTypeName}): {ex}");
            }
        }

        private static void UnlockSubnauticaPetsExtras()
        {
            try
            {
                PDAEncyclopedia.Add("PetFabricator", false, false);
                PDAEncyclopedia.Add("PetConsole", false, false);
                global::Story.StoryGoal.Execute("PetDna", global::Story.GoalType.Story, false, false);
            }
            catch
            {
                // Silently ignore if story goals or encyclopedia entries don't exist
            }
        }
    }

    [HarmonyPatch(typeof(global::uGUI_CraftingMenu), nameof(global::uGUI_CraftingMenu.Open))]
    public static class CraftingMenuOpenPatch
    {
        private static void Prefix()
        {
            KnownTechRetroactiveUnlock.CheckPendingAnalysisTech();
        }
    }
}
