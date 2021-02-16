using RimWorld;

namespace RimForge.Buildings
{
    public class Building_FueledHeatingElement : HeatingElement
    {
        public CompRefuelable FuelComp => _fuelComp ??= GetComp<CompRefuelable>();
        private CompRefuelable _fuelComp;

        public bool IsBurningFuel { get; protected set; }

        public override void Tick()
        {
            IsBurningFuel = ShouldBurnFuelNow();
            FuelComp.Props.fuelConsumptionRate = IsBurningFuel ? HEDef.activeFuelBurnRate : 0f;
            if (IsBurningFuel)
                FuelComp.ConsumeFuel(FuelComp.Props.fuelConsumptionRate / 60000f);

            base.Tick();
        }

        public virtual bool ShouldBurnFuelNow()
        {
            return ConnectedForge?.WantsTemperatureIncrease ?? false;
        }

        public override float GetProvidedHeat()
        {
            return (FuelComp.HasFuel && IsBurningFuel) ? HEDef.maxAddedHeat : 0f;
        }
    }
}
