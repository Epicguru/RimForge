using RimWorld;
using Verse;

namespace RimForge.Buildings
{
    public class Building_WoodHeatingElement : HeatingElement
    {
        public CompRefuelable FuelComp => _fuelComp ??= GetComp<CompRefuelable>();
        private CompRefuelable _fuelComp;

        public override float GetProvidedHeat()
        {
            return FuelComp.HasFuel ? HEDef.maxAddedHeat : 0f;
        }
    }
}
