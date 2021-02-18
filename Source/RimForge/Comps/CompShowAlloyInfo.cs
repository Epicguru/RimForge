using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimForge.Comps
{
    public class CompShowAlloyInfo : ThingComp
    {
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            if(base.SpecialDisplayStats() != null)
                foreach (var item in base.SpecialDisplayStats())
                    yield return item;

            var def = parent?.def;
            if (def == null)
                yield break;

            if (AlloyHelper.AllCraftableAlloys.TryGetValue(def, out var found))
            {
                List<Dialog_InfoCard.Hyperlink> links = new List<Dialog_InfoCard.Hyperlink>(found.input.Count);
                string names = "";
                for (int i = 0; i < found.input.Count; i++)
                {
                    names += found.input[i].resource.LabelCap;
                    if (i != found.input.Count - 1)
                        names += ", ";
                    links.Add(new Dialog_InfoCard.Hyperlink(found.input[i].resource));
                }

                yield return new StatDrawEntry(RFDefOf.RF_RimForgeStats,
                    "RF.Stats.MadeUsing".Translate(),
                    names,
                    "RF.Stats.MadeUsingDesc".Translate(),
                    2,
                    hyperlinks: links);
            }

            yield return new StatDrawEntry(RFDefOf.RF_RimForgeStats,
                "RF.Stats.MeltingPoint".Translate(),
                def.GetMeltingPoint().ToStringTemperature(),
                "RF.Stats.MeltingPointDesc".Translate(),
                0);

            var equivalents = AlloyHelper.GetEquivalentResources(def);
            if (equivalents != null && equivalents.Count > 1)
            {
                List<Dialog_InfoCard.Hyperlink> links = new List<Dialog_InfoCard.Hyperlink>(equivalents.Count);
                foreach (var item in equivalents)
                {
                    if (item == def)
                        continue;
                    links.Add(new Dialog_InfoCard.Hyperlink(item));
                }
                yield return new StatDrawEntry(RFDefOf.RF_RimForgeStats,
                    "RF.Stats.EquivalentTo".Translate(),
                    "RF.Stats.EquivalentToText".Translate(equivalents.Count - 1),
                    "RF.Stats.EquivalentToDesc".Translate(),
                    0,
                    hyperlinks: links);
            }

            if (AlloyHelper.UsedToMake.TryGetValue(def, out var usedToMake))
            {
                List<Dialog_InfoCard.Hyperlink> links = new List<Dialog_InfoCard.Hyperlink>(usedToMake.Count);
                string text = "";
                for(int i = 0; i < usedToMake.Count; i++)
                {
                    var item = usedToMake[i];
                    links.Add(new Dialog_InfoCard.Hyperlink(item));
                    text += item.LabelCap;
                    if (i != usedToMake.Count - 1)
                        text += ", ";
                }
                yield return new StatDrawEntry(RFDefOf.RF_RimForgeStats,
                    "RF.Stats.UsedToMake".Translate(),
                    text,
                    "RF.Stats.UsedToMakeDesc".Translate(),
                    0,
                    hyperlinks: links);
            }
        }
    }
}
