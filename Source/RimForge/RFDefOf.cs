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
        public static DamageDef RF_RitualDamage;
        public static ThingDef RF_Copper;
        public static ThingDef RF_Tin;
        public static StatCategoryDef RF_RimForgeStats;
        public static ShaderTypeDef TransparentPostLight;
        public static SoundDef RF_Sound_CoilgunFire;
        public static ThingDef RF_Motes_MuzzleFlash;
        public static ThingDef RF_Motes_RitualDistort;
        public static TraitDef RF_BlessingOfZir;
        public static TraitDef RF_ZirsCorruption;
        public static ThingDef RF_Forge;
        public static ThoughtDef RF_RitualBlessed;
        public static ThoughtDef RF_RitualBadThought;
        public static ThoughtDef RF_WaveOfHate;
        public static ThoughtDef RF_ZirKiller;
        public static ThingDef RF_SwordOfRapture;
        public static ThingDef RF_SwordOfDarkness;
        public static LetterDef RF_CursedRaiders;
        public static ThingDef RF_MercuryUltraweave;
        public static ThingDef RF_Motes_Growth;

        // Vanilla ones.
        public static ThingDef Column;
        public static BodyPartDef Spine;
        public static ThingCategoryDef WeaponsMelee;
    }
}
