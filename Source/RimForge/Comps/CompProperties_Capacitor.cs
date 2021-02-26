using Verse;

namespace RimForge.Comps
{
    public class CompProperties_Capacitor : CompProperties
    {
        public float maxStoredWd = 2000;

        public CompProperties_Capacitor()
        {
            compClass = typeof(CompCapacitor);
        }
    }
}
