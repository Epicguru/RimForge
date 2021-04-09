using HarmonyLib;
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
            
            Core.Warn("PAWN WITH TRAIT DIED");

            Map map = __instance.Map;
            if (map == null)
            {
                Core.Warn("Pawn with corruption trait died on null map.");
                return;
            }

            // Look into:
            //GenExplosion.NotifyNearbyPawnsOfDangerousExplosive
            if (Settings.CorruptionWaveRadius <= 0f)
                return;

            foreach (var cell in GenRadial.RadialCellsAround(__instance.Position, Settings.CorruptionWaveRadius, true))
            {
                foreach (var thing in map.thingGrid.ThingsListAt(cell))
                {
                    if (thing.DestroyedOrNull() || !(thing is Pawn pawn) || pawn.Dead)
                        continue;

                    // Only alive pawns, with human-like intelligence.
                    if ((pawn.RaceProps?.intelligence ?? Intelligence.Humanlike) < Intelligence.Humanlike)
                        continue;

                    if (pawn.RaceProps?.IsMechanoid ?? false)
                        continue;

                    pawn.TryGiveThought(RFDefOf.RF_WaveOfHate);
                }
            }
        }
    }
}
