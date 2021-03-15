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

        public static DamageDef RF_Electrocution;
        public static DamageDef RF_CoilgunDamage;
        public static ThingDef RF_Copper;
        public static ThingDef RF_Tin;
        public static StatCategoryDef RF_RimForgeStats;
        public static ShaderTypeDef TransparentPostLight;
        public static SoundDef RF_Sound_CoilgunFire;
        public static ThingDef RF_Motes_MuzzleFlash;
        public static ThingDef RF_Motes_RitualDistort;
        public static TraitDef RF_BlessingOfZir;
        public static ThingDef RF_Forge;
        public static ThingDef RF_CoilgunShellAP, RF_CoilgunShellSP;

        // Vanilla ones.
        public static ThingDef Column;
    }
}
