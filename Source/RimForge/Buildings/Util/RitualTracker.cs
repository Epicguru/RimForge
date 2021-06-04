using RimWorld.Planet;
using Verse;

namespace RimForge.Buildings
{
    public class RitualTracker : WorldComponent
    {
        public static RitualTracker Current => Find.World?.GetComponent<RitualTracker>();

        public int PlayerPerformedRituals;

        public RitualTracker(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref PlayerPerformedRituals, "playerPerformedRituals", 0);
        }
    }
}
