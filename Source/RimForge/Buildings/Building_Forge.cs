using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_Forge : Building
    {
        public CompRefuelable FuelComp => _fuelComp ?? (_fuelComp = GetComp<CompRefuelable>());
        private CompRefuelable _fuelComp;

        public ThingDef CurrentFuelType
        {
            get => FuelComp.Props.fuelFilter.AllowedThingDefs.FirstOrFallback();
            set
            {
                var filter = FuelComp.Props.fuelFilter;
                filter.SetDisallowAll();
                if(value != null)
                    filter.SetAllow(value, true);

                // TODO allow anything within the equivalent-to types.
                // Then once inserted, convert to 'base' type.
            }
        }
        public IntVec3 OutputPos => InteractionCell;

        public AlloyDef CurrentAlloyDef;

        private Dictionary<ThingDef, int> storedResources = new Dictionary<ThingDef, int>();
        private int requestCount = 50;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            CurrentFuelType = RFDefOf.RF_GoldDore;
            FuelComp.Props.autoRefuelPercent = 0f;
            FuelComp.Props.drawFuelGaugeInMap = false;
            FuelComp.Props.drawOutOfFuelOverlay = false;

            CurrentAlloyDef = RFDefOf.RF_GoldDoreAlloy;
        }

        public override void Tick()
        {
            base.Tick();

            if (CurrentAlloyDef != null)
            {
                if (!CurrentAlloyDef.IsValid)
                {
                    Core.Error($"Tried to use invalid alloy def: {CurrentAlloyDef}");
                    CurrentAlloyDef = null;
                    return;
                }
            }

            if (FuelComp.Fuel > 0)
            {
                int count = Mathf.RoundToInt(FuelComp.Fuel);
                if (count <= 0)
                {
                    FuelComp.ConsumeFuel(1);
                    return;
                }

                bool accept = UponResourceInput(CurrentFuelType, count);
                FuelComp.ConsumeFuel(FuelComp.Fuel + 1);
                if(!accept)
                {
                    // Reject this input, spit it back out.
                    PlaceOutput(CurrentFuelType, count);
                }
            }
        }

        /// <summary>
        /// Called when a pawn has refueled this forge with a material.
        /// Return true to accept the material, and reset the fuel component ready to accept more fuel.
        /// Returning false will reject the input, placing it back into the world.
        /// </summary>
        public virtual bool UponResourceInput(ThingDef rawInput, int count)
        {
            // TODO check if this is the expected input.

            requestCount = Rand.RangeInclusive(10, 50);
            CurrentFuelType = Rand.Chance(0.5f) ? RFDefOf.Plasteel : RFDefOf.Steel;
            Core.Log($"Input: {rawInput.LabelCap} x{count}. Now requesting {CurrentFuelType.LabelCap} x{requestCount}");

            ChangeStored(rawInput, count);

            return true;
        }

        public void PlaceOutput(ThingDef def, int count)
        {
            if (count <= 0)
                return;

            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = count;
            GenPlace.TryPlaceThing(thing, OutputPos, this.Map, ThingPlaceMode.Near);
        }

        public virtual bool ShouldAutoRefuelNow()
        {
            return CurrentAlloyDef != null;
        }

        public virtual float TargetFuelLevel()
        {
            return requestCount;
        }

        public void ChangeStored(ThingDef def, int change)
        {
            if (def == null || change == 0)
                return;

            if (storedResources.ContainsKey(def))
                storedResources[def] += change;
            else if (change > 0)
                storedResources.Add(def, change);
            if (storedResources[def] <= 0)
                storedResources.Remove(def);
        }

        /// <summary>
        /// Gets the number of 'alloy conversion' that could be performed given the current stored materials,
        /// and a specific alloy recipe. Ignored required temperature.
        /// This is not the same as the total expected alloy output.
        /// For that, see <see cref="GetPossibleOutputCount(AlloyDef)"/>.
        /// </summary>
        public int GetPossibleIterationCount(AlloyDef def, out (ThingDef resource, int count) missing)
        {
            if (def == null || !def.IsValid)
            {
                missing = (null, 0);
                return 0;
            }

            int max = -1;
            foreach (var input in def.input)
            {
                int stored = storedResources.TryGetValue(input.resource, 0);
                if (stored < input.count)
                {
                    missing = (input.resource, input.count - stored);
                    return 0;
                }

                int divs = Mathf.FloorToInt((float)stored / input.count); // I think that this is the same as regular int division, but just in case...
                if (max == -1)
                    max = divs;
                if (max > divs)
                    max = divs;
            }

            missing = (null, 0);
            return max;
        }

        /// <summary>
        /// See <see cref="GetPossibleIterationCount(AlloyDef)"/>.
        /// This is the same as the iteration count, but returns the expected alloy output count.
        /// </summary>
        public int GetPossibleOutputCount(AlloyDef def, out (ThingDef resource, int count) missing)
        {
            if (def == null || !def.IsValid)
            {
                missing = (null, 0);
                return 0;
            }

            return GetPossibleIterationCount(def, out missing) * def.output.count;
        }

        public override string GetInspectString()
        {
            string root = base.GetInspectString().TrimEnd();
            string stored = "";
            int i = 0;
            foreach (var pair in storedResources)
            {
                if (pair.Key != null && pair.Value > 0)
                    stored += $"{pair.Key.LabelCap} x{pair.Value}";
                if (i != storedResources.Count - 1)
                    stored += ",\n";
                i++;
            }

            int expectedOutput = GetPossibleOutputCount(CurrentAlloyDef, out var missing);

            if (storedResources.Count == 0)
                stored = "None";

            return $"{root}\nCan output: {expectedOutput}{(missing.resource == null ? "" : $" (missing {missing.resource.LabelCap} x{missing.count})")}\nStored:\n{stored}";
        }
    }
}
