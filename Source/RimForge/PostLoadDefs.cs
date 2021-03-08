using System;
using System.Collections.Generic;
using RimForge.Comps;
using Verse;

namespace RimForge
{
    [StaticConstructorOnStartup]
    internal static class PostLoadDefs
    {
        static PostLoadDefs()
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
        }

        private static void ProcessDefs()
        {
            foreach (var def in DefDatabase<AlloyDef>.AllDefsListForReading)
            {
                if (!def.IsValid)
                    continue;

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

            // Loop through every single ThingDef, see if it has the Extension on it, if it does
            // then it is considered 'part' of this mod's resources.
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var extension = def?.GetModExtension<Extension>();
                if (extension == null)
                    continue;

                AlloyHelper.AllRimForgeResources.Add(def);
                if (extension.equivalentTo != null)
                {
                    AlloyHelper.AddEquivalentResource(def, extension.equivalentTo);
                }
            }
        }
    }
}
