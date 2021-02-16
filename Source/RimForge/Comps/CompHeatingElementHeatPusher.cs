using RimForge.Buildings;
using Verse;

namespace RimForge.Comps
{
    public class CompHeatingElementHeatPusher : CompHeatPusher
    {
        public const float HEAT_PUSH_COEF = 0.025f;

        public HeatingElement Heater { get; protected set; }
        protected override bool ShouldPushHeatNow => GetHeatToPush() != 0f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Heater = parent as HeatingElement;
        }

        public override void CompTick()
        {
            Props.heatPerSecond = GetHeatToPush();

            base.CompTick();
        }

        public virtual float GetHeatToPush()
        {
            return Heater.GetProvidedHeat() * HEAT_PUSH_COEF;
        }
    }
}
