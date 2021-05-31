using HarmonyLib;
using RimWorld;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class Patch_TraitSet_GainTrait
    {
        static void Postfix(Pawn ___pawn, Trait trait)
        {
            if (trait.def == RFDefOf.RF_ZirsCorruption || trait.def == RFDefOf.RF_BlessingOfZir)
            {
                TraitTracker.Current?.TryAdd(___pawn);
            }
        }
    }
}
