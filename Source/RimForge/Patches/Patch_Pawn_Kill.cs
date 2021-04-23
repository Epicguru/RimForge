using HarmonyLib;
using RimForge.Misc;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Pawn), "Kill")]
    static class Patch_Pawn_Kill
    {
        static void Prefix(Pawn __instance)
        {
            var trait = __instance.story?.traits?.GetTrait(RFDefOf.RF_ZirsCorruption);
            if (trait == null)
                return;
            
            //Core.Warn("PAWN WITH TRAIT DIED");
            WaveOfHate.StartAt(__instance);
        }
    }
}
