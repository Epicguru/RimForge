using RimForge.Comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_FueledHeatingElement : Building_HeatingElement, IConditionalGlower
    {
        public CompRefuelable FuelComp => _fuelComp ??= GetComp<CompRefuelable>();
        private CompRefuelable _fuelComp;

        private int tickCounter = 0;

        public override Graphic Graphic
        {
            get
            {
                if (Content.HEFueledIdle == null)
                    Content.LoadFueledHeatingElementGraphics(this);

                return IsForgeRunning && FuelComp.HasFuel ? Content.HEFueledGlow : Content.HEFueledIdle;
            }
        }

        public override float GetPotentialHeatIncrease()
        {
            return (FuelComp.HasFuel) ? HEDef.maxAddedHeat : 0f;
        }

        public override float TickActive()
        {
            base.TickActive();

            bool hasFuel = FuelComp.HasFuel;

            FuelComp.Props.fuelConsumptionRate = hasFuel ? HEDef.activeFuelBurnRate : 0f;
            if (!hasFuel)
                return 0f;

            tickCounter++;
            if (tickCounter % 15 == 0)
            {
                FleckMaker.ThrowSmoke(Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead) + new Vector3(0.5f, 0, 2f), Map, 0.65f);
            }

            FuelComp.ConsumeFuel(FuelComp.Props.fuelConsumptionRate / 60000f);
            return HEDef.maxAddedHeat;
        }

        public bool ShouldGlowNow()
        {
            return FuelComp.HasFuel && IsForgeRunning;
        }
    }
}
