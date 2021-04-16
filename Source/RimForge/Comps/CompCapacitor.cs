using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Comps
{
    [StaticConstructorOnStartup] // Shut up RimWorld
    public class CompCapacitor : ThingComp
    {
        public CompProperties_Capacitor Props => props as CompProperties_Capacitor;
        public CompPowerTrader Trader => trader ??= parent?.GetComp<CompPowerTrader>();
        public bool IsFull => percentageStored >= 1f;

        public float PercentageStored
        {
            get => percentageStored;
            set => percentageStored = Mathf.Clamp01(value);
        }

        public PowerNet PowerNet => Trader?.PowerNet;

        private CompPowerTrader trader;
        private float percentageStored;

        public override void CompTick()
        {
            base.CompTick();

            Trader.powerOutputInt = IsFull ? 0f : -Props.powerRequirement;

            if (Trader.PowerOn && !IsFull)
            {
                float chargePerTick = 1f / Props.ticksToCharge;
                percentageStored += chargePerTick;
                if (percentageStored > 1f)
                    percentageStored = 1f;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref percentageStored, "rf_capStoredPct");
            percentageStored = Mathf.Clamp01(percentageStored);
        }

        public override string CompInspectStringExtra()
        {
            string str = "RF.Cap.Stored".Translate((percentageStored * 100f).ToString("F0"));
            return str;
        }
    }
}
