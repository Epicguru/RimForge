using RimWorld;

namespace RimForge.Buildings
{
    public class Building_FueledHeatingElement : HeatingElement
    {
        public CompRefuelable FuelComp => _fuelComp ??= GetComp<CompRefuelable>();
        private CompRefuelable _fuelComp;

        public override float GetPotentialHeatIncrease()
        {
            return (FuelComp.HasFuel) ? HEDef.maxAddedHeat : 0f;
        }

        public override float TickActive()
        {
            bool hasFuel = FuelComp.HasFuel;

            FuelComp.Props.fuelConsumptionRate = hasFuel ? HEDef.activeFuelBurnRate : 0f;
            if (hasFuel)
            {
                FuelComp.ConsumeFuel(FuelComp.Props.fuelConsumptionRate / 60000f);
                return HEDef.maxAddedHeat;
            }

            return 0f;
        }
    }
}
