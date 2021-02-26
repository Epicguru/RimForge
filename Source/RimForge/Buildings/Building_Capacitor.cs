using RimForge.Comps;
using RimWorld;
using Verse;

namespace RimForge.Buildings
{
    public class Building_Capacitor : Building
    {
        public CompCapacitor CompCap => _power ??= GetComp<CompCapacitor>();
        private CompCapacitor _power;

        public override string GetInspectString()
        {
            return CompCap.CompInspectStringExtra();
        }
    }
}
