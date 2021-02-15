using RimWorld;
using Verse;

namespace RimForge
{
    [DefOf]
    public static class RFDefOf
    {
        static RFDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RFDefOf));
        }

        public static AlloyDef RF_GoldDoreAlloy;
        public static ThingDef RF_GoldDore;
        public static ThingDef Gold;
        public static ThingDef Silver;
    }
}
