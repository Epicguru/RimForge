using RimForge.Comps;
using RimForge.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_PoweredHeatingElement : HeatingElement, IConditionalGlower
    {
        public const float IDLE_POWER_DRAW = 50;

        public override Graphic Graphic
        {
            get
            {
                if (Content.HEPoweredIdle == null)
                    Content.LoadPoweredHeatingElementGraphics(this);

                bool hasPower = PowerTrader.PowerOn;
                bool isRunning = IsForgeRunning;

                if (!hasPower)
                    return Content.HEPoweredIdle;
                return isRunning ? Content.HEPoweredGlow : Content.HEPoweredPowerOn;
            }
        }

        public CompPowerTrader PowerTrader => _power ??= GetComp<CompPowerTrader>();
        private CompPowerTrader _power;

        /// <summary>
        /// The coefficient for how much power is consumed. Higher values lead to more
        /// power consumption and increased heat output.
        /// Defaults to 1.
        /// </summary>
        public float PowerLevel { get; set; } = 1f;

        private BezierElectricArc arc;

        [TweakValue("_RimForge", -2, 2)]
        private static float OffE = 0.3f, OffF = 1f, OffG = 0.25f, OffH = 0.4f;

        public override void Tick()
        {
            PowerTrader.powerOutputInt = -GetCurrentPowerDraw();
            base.Tick();

            if (PowerTrader.PowerOn && IsForgeRunning)
            {
                SustainArc();
            }
            else if(arc != null)
            {
                arc.Destroy();
                arc = null;
            }
        }

        protected virtual void SustainArc()
        {
            if(arc == null)
            {
                arc = new BezierElectricArc(20);
                arc.Spawn(Map);
            }

            var basePos = DrawPos.WorldToFlat();
            arc.P0 = basePos + new Vector2(0, 1.479f);
            arc.P3 = basePos + new Vector2(ForgeCellIndex == 0 ? 1.524f : -1.524f, 1.633f);
            arc.P1 = Vector2.Lerp(arc.P0, arc.P3, 0.3f) + new Vector2(0f, 0.65f);
            arc.P2 = Vector2.Lerp(arc.P0, arc.P3, 0.7f) + new Vector2(0f, 0.65f);
            arc.Amplitude = new Vector2(0, 0.28f);
        }

        public virtual float GetCurrentPowerDraw()
        {
            if (!IsForgeRunning)
                return IDLE_POWER_DRAW;

            return HEDef.activePowerDraw * PowerLevel;
        }

        public override string GetInspectString()
        {
            return $"{base.GetInspectString()}\n{"RF.HeatingElement.CurrentPowerLevel".Translate((PowerLevel * 100f).ToString("F0") + "%")}";
        }

        public override float GetPotentialHeatIncrease()
        {
            return PowerTrader.PowerOn ? HEDef.maxAddedHeat * PowerLevel : 0;
        }

        public override float TickActive()
        {
            base.TickActive();

            if (PowerTrader.PowerOn)
                return HEDef.maxAddedHeat * PowerLevel;
            return 0;
        }

        public bool ShouldGlowNow()
        {
            return PowerTrader.PowerOn && IsForgeRunning;
        }
    }
}
