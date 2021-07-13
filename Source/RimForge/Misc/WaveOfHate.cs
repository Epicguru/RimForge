using System;
using RimWorld;
using Verse;

namespace RimForge.Misc
{
    public static class WaveOfHate
    {
        public static void StartAt(Pawn pawn)
        {
            if (pawn == null)
                return;

            // Look into:
            //GenExplosion.NotifyNearbyPawnsOfDangerousExplosive

            TraitTracker.Current.StartExplosionEffect(new TraitTracker.ExplosionEffect()
            {
                TicksAlive = 0,
                Center = pawn.DrawPos.WorldToFlat(),
                MapId = pawn.Map.uniqueID
            });
        }

        public static void WorkAt(Map map, IntVec3 position, float radius, int stunDuration)
        {
            if (radius <= 0f)
                return;

            GenExplosion.DoExplosion(position, map, radius, DamageDefOf.Bomb, null, 0, 0);

            foreach (var cell in GenRadial.RadialCellsAround(position, radius, true))
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

                    // Give thought.
                    pawn.TryGiveThought(RFDefOf.RF_WaveOfHate);

                    // Do stun, if applicable
                    if (stunDuration <= 0)
                        continue;
                    try
                    {
                        var stunner = pawn.stances?.stunner;
                        if (stunner != null)
                            stunner.StunFor(stunDuration, null);
                        else
                            Core.Warn($"Failed to stun pawn '{pawn.LabelCap}' because they do not have a <stance?.stunner>");
                    }
                    catch (Exception e)
                    {
                        Core.Error($"Exception stunning pawn '{pawn.LabelShortCap}'", e);
                    }
                }
            }
        }
    }
}
