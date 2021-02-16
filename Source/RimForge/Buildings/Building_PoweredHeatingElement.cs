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

        public override void Tick()
        {
            IsActive = ShouldBeActive();
            PowerTrader.powerOutputInt = -GetCurrentPowerDraw();

            base.Tick();
        }

        public virtual bool ShouldBeActive()
        {
            return (ConnectedForge?.WantsTemperatureIncrease ?? false) && PowerTrader.PowerOn;
        }

        public virtual float GetCurrentPowerDraw()
        {
            if (!IsActive)
                return IDLE_POWER_DRAW;

            return HEDef.activePowerDraw * PowerLevel;
        }

        public override float GetProvidedHeat()
        {
            return IsActive ? HEDef.maxAddedHeat * PowerLevel : 0f;
        }

        public override string GetInspectString()
        {
            return $"{base.GetInspectString()}\n{"RF.CurrentPowerLevel".Translate((PowerLevel * 100f).ToString("F0") + "%")}";
        }
    }
}
