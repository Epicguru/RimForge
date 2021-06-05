using RimForge.Buildings;
using RimForge.Comps;
using System;
using System.Collections.Generic;
using AchievementsExpanded;
using HarmonyLib;
using RimForge.Achievements;
using RimForge.CombatExtended;
using RimForge.Patches;
using Verse;

namespace RimForge
{
    [StaticConstructorOnStartup]
    internal static class StartupLoading
    {
        static StartupLoading()
        {
            DoLoadEarly();
        }

        internal static void DoLoadEarly()
        {
            Core.Log("Starting def processing...");
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            try
            {
                ProcessDefs();
            }
            catch (Exception e)
            {
                Core.Error("Failed while processing defs. This could result in incorrect or missing detail in descriptions, and other bugs.", e);
            }
            watch.Stop();

            Core.Log($"Completed def processing in {watch.ElapsedMilliseconds} milliseconds.");
            Core.Log($"There are {AlloyHelper.AllAlloyDefs.Count} recipes ({AlloyHelper.AllCraftableAlloys.Count} craftable alloys), and {AlloyHelper.AllRimForgeResources.Count} general resources.");

            MiscLoadEarlyTasks();
        }

        internal static void DoLoadLate()
        {
            Core.Log("Doing late load...");

            // Necessary to let the game know that we actually have settings to save and load.
            Core.Instance.GetSettings<Settings>();

            // Toggle ore generation.
            if (!Settings.GenerateCopper)
            {
                var def = RFDefOf.RF_MineableCopper;
                def.building.mineableScatterCommonality = 0f;
            }
            if (!Settings.GenerateTin)
            {
                var def = RFDefOf.RF_MineableTin;
                def.building.mineableScatterCommonality = 0f;
            }

            if (CECompat.IsCEActive)
            {
                try
                {
                    var original = AccessTools.Method("CombatExtended.CompSuppressable:AddSuppression");
                    var patchRaw = AccessTools.Method(typeof(Patch_CompSuppressable_AddSuppression), nameof(Patch_CompSuppressable_AddSuppression.Prefix));
                    var patch = new HarmonyMethod(patchRaw);
                    Core.Instance.HarmonyInstance.Patch(original, prefix: patch);
                }
                catch(Exception e)
                {
                    Core.Error("Failed to patch CombatExtended suppression:", e);
                }

                foreach (var item in CECompat.GetCEMortarShells())
                {
                    Building_DroneLauncher.LoadableBombs.Add(item);
                }
            }
        }

        private static void ProcessDefs()
        {
            foreach (var def in DefDatabase<AlloyDef>.AllDefsListForReading)
            {
                if (!def.IsValid)
                {
                    Core.Error($"Invalid alloy def: {def}");
                    continue;
                }

                AlloyHelper.AllAlloyDefs.Add(def);

                var output = def.output.resource;
                AlloyHelper.AllCraftableAlloys.Add(output, def);

                foreach (var input in def.input)
                {
                    if (AlloyHelper.UsedToMake.TryGetValue(input.resource, out var list))
                    {
                        list.Add(def.output.resource);
                    }
                    else
                    {
                        list = new List<ThingDef>();
                        list.Add(def.output.resource);
                        AlloyHelper.UsedToMake.Add(input.resource, list);
                    }

                    if (!input.resource.HasComp(typeof(CompShowAlloyInfo)))
                    {
                        input.resource.comps.Add(new CompProperties_ShowAlloyInfo());
                        //Core.Log($"Since {input.resource.LabelCap} is an ingredient in '{def.LabelCap}', it has been given the ShowAlloyInfo component.");
                    }
                }
                if (!output.HasComp(typeof(CompShowAlloyInfo)))
                {
                    output.comps.Add(new CompProperties_ShowAlloyInfo());
                    //Core.Log($"Since {output.LabelCap} is the output of '{def.LabelCap}', it has been given the ShowAlloyInfo component.");
                }
            }

            Core.Log($"There were a total of {DefDatabase<AlloyDef>.AllDefsListForReading.Count} alloy defs (Bronze: {RFDefOf.RF_BronzeAlloy})");

            // Loop through every single ThingDef, see if it has the Extension on it, if it does
            // then it is considered 'part' of this mod's resources.
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (!CECompat.IsCEActive)
                {
                    if (def.IsShell)
                    {
                        Building_DroneLauncher.LoadableBombs.Add(def);
                    }
                }

                var extension = def?.GetModExtension<Extension>();
                if (extension == null)
                    continue;

                AlloyHelper.AllRimForgeResources.Add(def);
                if (extension.equivalentTo != null)
                {
                    AlloyHelper.AddEquivalentResource(def, extension.equivalentTo);
                }
                if (!def.HasComp(typeof(CompShowAlloyInfo)))
                {
                    def.comps.Add(new CompProperties_ShowAlloyInfo());
                }
            }

            // Generate forge recipes from alloy defs.
            var recipes = RecipeGenerator.GenAlloySmeltDefs(AlloyHelper.AllAlloyDefs);
            RFDefOf.RF_Forge.recipes ??= new List<RecipeDef>();
            RFDefOf.RF_Forge.recipes.AddRange(recipes);
        }

        private static void MiscLoadEarlyTasks()
        {
            Building_Coilgun.ShellDefs.AddRange(DefDatabase<CoilgunShellDef>.AllDefsListForReading);

            HEShellKillTracker.ReportKills += (e, count) =>
            {
                if (e == null )
                    return;

                #region VEA
                foreach (var card in AchievementPointManager.GetCards<CoilgunExplosiveTracker>())
                {
                    try
                    {
                        if ((card.tracker as CoilgunExplosiveTracker).Trigger(e, count))
                        {
                            card.UnlockCard();
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
                        card.UnlockCard();
                    }
                }
                #endregion
            };
        }
    }
}
