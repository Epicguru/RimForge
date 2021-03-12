using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimForge.Buildings
{
    public class Building_ForgeRewritten : Building_WorkTable
    {
        public int ConnectedHeatingElementCount => heatingElements?.Count ?? 0;

        private HashSet<HeatingElement> heatingElements = new HashSet<HeatingElement>();
        private bool hasHeat = false;
        private int tickCounter;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref heatingElements, "rf_forgeHeatingElements", LookMode.Reference);
            SanitizeHeatingElements();
            tickCounter = Rand.Range(0, 60);
        }

        public virtual IEnumerable<IntVec3> GetHeatingElementLookCells()
        {
            yield return Position - new IntVec3(2, 0, 0);
            yield return Position + new IntVec3(2, 0, 0);
        }

        public override void Tick()
        {
            base.Tick();

            tickCounter++;
            if (tickCounter % 30 == 0)
            {
                var map = Map;
                SanitizeHeatingElements();
                foreach(var cell in GetHeatingElementLookCells())
                {
                    var thing = map.thingGrid.ThingAt<HeatingElement>(cell);
                    if (thing != null && !heatingElements.Contains(thing))
                        heatingElements.Add(thing);
                }
            }
        }

        public float GetPotentialHeat()
        {
            float heat = AmbientTemperature;
            SanitizeHeatingElements();
            foreach (var item in heatingElements)
            {
                if (item.DestroyedOrNull())
                    continue;
                heat += item.GetPotentialHeatIncrease();
            }
            return heat;
        }

        private void SanitizeHeatingElements()
        {
            heatingElements ??= new HashSet<HeatingElement>();
            heatingElements.RemoveWhere(h => h.DestroyedOrNull());
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Command_Action()
            {
                defaultLabel = $"toggle heat\ncurrent: {hasHeat}",
                action = () =>
                {
                    hasHeat = !hasHeat;
                }
            };
        }

        public virtual bool CanDoBillNow(Bill_Production bill)
        {
            var alloyDef = bill?.recipe?.TryGetAlloyDef();
            if (alloyDef == null)
            {
                Core.Warn($"Bill '{bill}' was passed into Forge.CanDoBillNow(), but an alloy def was not found for it.");
                return false;
            }

            float maxTheoreticalHeat = GetPotentialHeat();
            return maxTheoreticalHeat >= alloyDef.MinTemperature;
        }

        public override string GetInspectString()
        {
            return $"{base.GetInspectString()}\nTemperature: {AmbientTemperature.ToStringTemperature()} (capable of {GetPotentialHeat().ToStringTemperature()})";
        }
    }
}
