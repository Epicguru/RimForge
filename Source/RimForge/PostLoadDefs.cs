using System;
using System.Collections.Generic;
using System.Text;
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
            Core.Log(RFDefOf.RF_GoldDoreAlloy.ToString(true));
        }

        private static void ProcessDefs()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                var extension = def?.GetModExtension<Extension>();
                if (extension == null)
                    continue;

                AlloyHelper.AllRimForgeResources.Add(def);
                float meltingPoint = def.GetMeltingPoint();
                def.description += $"\n\n<color=#ffb499><b>{"RF.ModName".Translate()}</b>\n{"RF.MeltingPoint".Translate()}: {meltingPoint.ToStringTemperature(format: "F0")}</color>";
                if (extension.equivalentTo != null)
                {
                    AlloyHelper.AddEquivalentResource(def, extension.equivalentTo);
                }
            }

            StringBuilder str = new StringBuilder();
            foreach (var def in AlloyHelper.AllRimForgeResources)
            {
                var equivalents = AlloyHelper.GetEquivalentResources(def);
                if (equivalents == null || equivalents.Count <= 1)
                    continue;

                str.Clear();
                str.Append("RF.EquivalentTo".Translate()).AppendLine(":");
                foreach (var item in equivalents)
                {
                    if (item == def)
                        continue;
                    str.Append("  -");
                    str.AppendLine(item.ModLabelCap());

                    def.descriptionHyperlinks ??= new List<DefHyperlink>();
                    def.descriptionHyperlinks.Add(new DefHyperlink(item));
                }
                def.description += $"\n<color=#ffb499>{str.ToString().TrimEnd()}</color>";

            }
        }
    }
}
