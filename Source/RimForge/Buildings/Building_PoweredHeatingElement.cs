using RimWorld;
using Verse;

namespace RimForge.Buildings
{
    public class Building_PoweredHeatingElement : HeatingElement
    {
        public const float IDLE_POWER_DRAW = 50;

        public CompPowerTrader PowerTrader => _power ??= GetComp<CompPowerTrader>();
        private CompPowerTrader _power;

        /// <summary>
        /// The coefficient for how much power is consumed. Higher values lead to more
        /// power consumption and increased heat output.
        /// Defaults to 1.
        /// </summary>
        public float PowerLevel { get; set; } = 1f;

        /// <summary>
        /// Is this powered heating element currently active?
        /// Active means that it is drawing power and producing heat for a connected forge.
        /// </summary>
        public bool IsActive { get; protected set; }

        private int ticksSinceActive = 100;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref ticksSinceActive, "hef_ticksSinceActive", 100);
        }

        public override void Tick()
        {
            ticksSinceActive++;
            IsActive = ticksSinceActive < 10;
            PowerTrader.powerOutputInt = -GetCurrentPowerDraw();

            base.Tick();
        }

        public virtual float GetCurrentPowerDraw()
        {
            if (!IsActive)
                return IDLE_POWER_DRAW;

            return HEDef.activePowerDraw * PowerLevel;
        }

        public override string GetInspectString()
        {
            return $"{base.GetInspectString()}\n{"RF.CurrentPowerLevel".Translate((PowerLevel * 100f).ToString("F0") + "%")}";
        }

        public override float GetPotentialHeatIncrease()
        {
            return PowerTrader.PowerOn ? HEDef.maxAddedHeat : 0;
        }

        public override float TickActive()
        {
            ticksSinceActive = 0;
            if (PowerTrader.PowerOn)
                return HEDef.maxAddedHeat;
            return 0;
        }
    }
}
