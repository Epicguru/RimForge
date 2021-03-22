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
        public static ShaderTypeDef Transparent;
        public static SoundDef RF_Sound_CoilgunFire;
        public static ThingDef RF_Motes_MuzzleFlash;
        public static ThingDef RF_Motes_RitualDistort;
        public static TraitDef RF_BlessingOfZir;
        public static ThingDef RF_Forge;
        public static ThoughtDef RF_RitualBlessed, RF_RitualBadThought;
        public static TerrainDef RF_DiscoFloor;
        public static ThoughtDef RF_AttendedDiscoThought;
        public static TaleDef RF_AttendedDiscoTale;
        public static GatheringDef RF_DiscoGathering;
        public static JobDef RF_Job_StandAtDJPlatform;
        public static JobDef RF_Job_Dance_Breakdance;

        // Vanilla ones.
        public static ThingDef Column;
        public static BodyPartDef Spine;
    }
}
