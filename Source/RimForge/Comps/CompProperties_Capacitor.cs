using Verse;

namespace RimForge.Comps
{
    public class CompProperties_Capacitor : CompProperties
    {
        public float powerRequirement = 2000;
        public int ticksToCharge = 2500;

        public CompProperties_Capacitor()
        {
            compClass = typeof(CompCapacitor);
        }
    }
}
