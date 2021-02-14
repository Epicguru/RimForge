using System;
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
                Core.Error("Failed while processing defs. This could result in incorrect or missing detail in descriptions.", e);
            }
            watch.Stop();


            Core.Log($"Completed def processing in {watch.ElapsedMilliseconds} milliseconds.");
            Core.Log(RFDefOf.RF_GoldDoreAlloy.ToString(true));
        }

        static void ProcessDefs()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def == null)
                    continue; // Dunno, just in case.

                var extension = def.GetModExtension<Extension>();
                if (extension == null)
                    continue;

                float meltingPoint = def.GetMeltingPoint();
                def.description += $"\n\n<color=#ff5c87><b>RimForge</b>\nMelting point: {meltingPoint.ToStringTemperature(format: "F0")}</color>";
            }
        }
    }
}
