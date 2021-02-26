using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Comps
{
    [StaticConstructorOnStartup] // Shut up RimWorld
    public class CompCapacitor : ThingComp
    {
        public const int TICK_RATE = 16;
        public static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f));
        public static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0, 0, 0, 0));
        public static readonly Material FuelBarUnfilledMatSolid = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));
        public static readonly Vector2 FuelBarSize = new Vector2(0.41f, 0.28f);

        public CompProperties_Capacitor Props => props as CompProperties_Capacitor;
        public float StoredWd
        {
            get => _storedWd;
            set => _storedWd = Mathf.Clamp(value, 0, Props.maxStoredWd);
        }
        public PowerNet PowerNet => parent.GetComp<CompPower>().PowerNet;

        private float _storedWd;
        private int tickOffset = -1;
        private int tickCounter;
        private int warning;

        public override void CompTick()
        {
            base.CompTick();

            if (tickOffset == -1)
                tickOffset = Rand.RangeInclusive(0, 100);

            tickCounter++;
            if ((tickCounter + tickOffset) % TICK_RATE == 0)
            {
                PullPower();
            }
        }

        public void PullPower()
        {
            warning = 0;
            // Steals power from batteries.
            var net = PowerNet;
            if (net == null)
            {
                warning = 1;
                return;
            }

            float total = Props.maxStoredWd;
            float toPull = total - StoredWd;
            if (toPull <= 0)
                return;

            if(net.batteryComps == null || net.batteryComps.Count == 0)
            {
                warning = 2;
                return;
            }

            float old = toPull;
            foreach (var bat in net.batteryComps)
            {
                if (bat == null)
                    continue;

                var stored = bat.StoredEnergy;
                // No power, lame.
                if (stored == 0)
                    continue;

                // Not enough power, so just take it all.
                if(stored <= toPull)
                {
                    StoredWd += stored;
                    toPull -= stored;
                    bat.SetStoredEnergyPct(0);
                    continue;
                }

                // More than enough power, take as much as required.
                float pctToPull = toPull / bat.Props.storedEnergyMax;
                bat.SetStoredEnergyPct(bat.StoredEnergyPct - pctToPull);
                StoredWd += toPull;
                toPull = 0;
                break;
            }

            if (old == toPull)
                warning = 3;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref _storedWd, "cap_storedWd");
        }

        public override void PostDraw()
        {
            base.PostDraw();

            switch (warning)
            {
                case 1:
                    this.parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.NeedsPower);
                    break;
                case 2:
                    this.parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.QuestionMark);
                    break;
                case 3:
                    this.parent.Map.overlayDrawer.DrawOverlay(parent, OverlayTypes.NeedsPower);
                    break;
            }

            GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest();
            r.center = this.parent.DrawPos + new Vector3(0, 0, -0.01f);
            r.size = FuelBarSize;
            r.fillPercent = StoredWd / Props.maxStoredWd;
            r.filledMat = FuelBarFilledMat;
            r.unfilledMat = FuelBarUnfilledMat;
            r.margin = 0f;
            r.rotation = Rot4.East;
            GenDraw.DrawFillableBar(r);
        }

        public override string CompInspectStringExtra()
        {
            float pct = StoredWd / Props.maxStoredWd;
            string str = "RF.Cap.Stored".Translate(StoredWd.ToString("F0"), (pct * 100f).ToString("F0"));

            string warn = null;
            switch (warning)
            {
                case 1:
                    warn = "RF.Cap.NotConnected".Translate();
                    break;
                case 2:
                    warn = "RF.Cap.NoBatteries".Translate();
                    break;
                case 3:
                    warn = "RF.Cap.NoPower".Translate();
                    break;
            }

            if (warn != null)
                str += $"\n{warn}";

            return str;
        }
    }
}
