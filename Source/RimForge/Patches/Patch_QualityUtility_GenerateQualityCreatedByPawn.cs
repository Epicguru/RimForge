using HarmonyLib;
using RimWorld;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn", typeof(Pawn), typeof(SkillDef))]
    static class Patch_QualityUtility_GenerateQualityCreatedByPawn
    {
        static void Postfix(Pawn pawn, SkillDef relevantSkill, ref QualityCategory __result)
        {
            if (__result == QualityCategory.Legendary)
                return; // Can't improve.

            if (pawn?.story?.traits == null)
                return;
            if (!pawn.story.traits.HasTrait(RFDefOf.RF_BlessingOfZir))
                return;

            bool mod = relevantSkill == SkillDefOf.Construction || relevantSkill == SkillDefOf.Crafting;
            if (!mod)
                return;

            // Bump up quality one level.
            // For reference, an inspiration increases it 2 levels.
            __result += 1;
            Core.Log($"Running Blessing of Zir on {pawn.LabelCap}: increased quality to {__result}");
        }
    }
}
