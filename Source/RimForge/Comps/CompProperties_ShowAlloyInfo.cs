using System.Collections.Generic;
using Verse;

namespace RimForge.Comps
{
    public class CompProperties_ShowAlloyInfo : CompProperties
    {
        public CompProperties_ShowAlloyInfo()
        {
            compClass = typeof(CompShowAlloyInfo);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var item in base.ConfigErrors(parentDef))
                yield return item;

            if(!parentDef.HasModExtension<Extension>())
                yield return $"Materials with the ShowAlloyInfo component should also have the RimForge.Extension ModExtension. ({parentDef.LabelCap})";
        }
    }
}
