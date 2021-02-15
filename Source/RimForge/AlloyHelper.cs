using System.Collections.Generic;
using Verse;

namespace RimForge
{
    public static class AlloyHelper
    {
        public const float DEFAULT_MELTING_POINT = 800;

        /// <summary>
        /// A list of all RimForge compatible resources.
        /// These are resources (materials, metals) that have melting points, meaning that they can be used in alloys.
        /// </summary>
        public static readonly List<ThingDef> AllRimForgeResources = new List<ThingDef>();

        private static readonly Dictionary<ThingDef, int> bagMap = new Dictionary<ThingDef, int>();
        private static readonly List<List<ThingDef>> bags = new List<List<ThingDef>>();
        private static readonly List<ThingDef> tempReturnList = new List<ThingDef>();

        /// <summary>
        /// Gets the melting point of this resource.
        /// Only intended to be used for things that could logically be melted
        /// down in a forge, such as metals or alloys.
        /// Unless a custom value has been defined using the <see cref="Extension"/>,
        /// this will return the default value <see cref="DEFAULT_MELTING_POINT"/>.
        /// </summary>
        /// <param name="def">The ThingDef to get the melting point for.</param>
        /// <returns>The melting point, measured in degrees celsius.</returns>
        public static float GetMeltingPoint(this ThingDef def)
        {
            var extension = def?.GetModExtension<Extension>();
            if (extension != null)
                return extension.meltingPoint ?? DEFAULT_MELTING_POINT;

            return DEFAULT_MELTING_POINT;
        }

        /// <summary>
        /// Gets a version of this thing's LabelCap, but including the name
        /// of the mod that adds it.
        /// For example, '[RimForge] Copper'
        /// </summary>
        public static string ModLabelCap(this ThingDef def)
        {
            if (def == null)
                return null;
            if (def.modContentPack == null)
                return def.LabelCap;
            return $"[{def.modContentPack.Name}] {def.LabelCap}";
        }

        /// <summary>
        /// Adds a logical equality between resources, for example 2 different types of copper.
        /// All the 'other' coppers should point back to the RimForge copper.
        /// </summary>
        /// <param name="resource">The 'other' resource, such as a different mod's copper.</param>
        /// <param name="equivalentTo">The resource that it is equivalent to, such as RimForge's copper.</param>
        public static void AddEquivalentResource(ThingDef resource, ThingDef equivalentTo)
        {
            if (resource == null || equivalentTo == null || resource == equivalentTo)
            {
                Core.Error($"Tried to add equivalent resources {resource?.ModLabelCap() ?? "<null>"} -> {equivalentTo?.ModLabelCap() ?? "<null>"}. Invalid.");
                return;
            }

            var bagA = GetBagContaining(resource, out int bagAIndex);
            var bagB = GetBagContaining(equivalentTo, out int bagBIndex);

            Core.Log($"Making equivalent: '{resource.ModLabelCap()}' => '{equivalentTo.ModLabelCap()}'");

            if (bagA == null && bagB == null)
            {
                // Create new bag, add these two.
                var set = new List<ThingDef>();
                set.Add(equivalentTo);
                set.Add(resource);
                int index = bags.Count;
                bagMap.Add(equivalentTo, index);
                bagMap.Add(resource, index);
                bags.Add(set);
            }
            else if (bagA == null)
            {
                // equivalentTo is already in a bag, so resource needs to join that bag.
                bagMap.Add(resource, bagBIndex);
                bagB.Add(resource);
            }
            else if (bagB == null)
            {
                // Target resource needs to be put into bag A.
                bagMap.Add(equivalentTo, bagAIndex);
                bagA.Insert(0, equivalentTo);
            }
            else if (bagA == bagB)
            {
                // They are already in the same bag...
                Core.Warn($"'{resource.ModLabelCap()}' is already equivalent to '{equivalentTo.ModLabelCap()}'");
            }
            else
            {
                // ERROR!
                // They are both in different bags!
                // I could merge the bags, but it's more likely that this was done by mistake, so I'll display an
                // error message.
                Core.Error($"Error adding equivalent resource: '{resource.ModLabelCap()}' is already in a resource bag, and '{equivalentTo.ModLabelCap()}' is in another.");
            }
        }

        public static IReadOnlyList<ThingDef> GetEquivalentResources(ThingDef def)
        {
            if (def == null)
                return null;

            if (bagMap.TryGetValue(def, out int index))
            {
                return bags[index];
            }

            tempReturnList.Clear();
            tempReturnList.Add(def);
            return tempReturnList;
        }

        private static List<ThingDef> GetBagContaining(ThingDef def, out int index)
        {
            if (def == null)
            {
                index = -1;
                return null;
            }

            for (int i = 0; i < bags.Count; i++)
            {
                var bag = bags[i];
                if (bag.Contains(def))
                {
                    index = i;
                    return bag;
                }
            }

            index = -1;
            return null;
        }
    }
}
