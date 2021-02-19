using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_Forge : Building
    {
        // 0.437
        [TweakValue("RimForge", 0, 1)]
        private static float forgeLerp;

        public CompRefuelable FuelComp => _fuelComp ??= GetComp<CompRefuelable>();
        private CompRefuelable _fuelComp;

        public override Graphic Graphic => GetCurrentGraphic();

        public ThingDef CurrentFuelType
        {
            get
            {
                // If there are more than one equivalent inputs, return the one from RimForge.
                var found = FuelComp.Props.fuelFilter.AllowedThingDefs.FirstOrFallback(t => t.modContentPack == Core.ContentPack);
                return found ?? FuelComp.Props.fuelFilter.AllowedThingDefs.FirstOrFallback();
            }
            set
            {
                var filter = FuelComp.Props.fuelFilter;
                filter.SetDisallowAll();
                if (value != null)
                {
                    var equivalents = AlloyHelper.GetEquivalentResources(value);
                    foreach (var item in equivalents)
                    {
                        filter.SetAllow(item, true);
                    }
                }
            }
        }
        public IntVec3 OutputPos => InteractionCell;

        public AlloyDef CurrentAlloyDef;
        public float CurrentTemperature { get; protected set; }
        public bool WantsTemperatureIncrease { get; protected set; }

        private readonly Dictionary<ThingDef, int> storedResources = new Dictionary<ThingDef, int>();
        private int requestCount = 50;
        private HeatingElement elementA, elementB;
        private int graphic;

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

            UpdateTemperature();

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

            forgeLerp += Time.deltaTime * 0.2f;
            if (forgeLerp >= 1.4f && CurrentAlloyDef != null)
            {
                forgeLerp = -0.5f;
                PlaceOutput(CurrentAlloyDef.output.resource, 100);
            }
        }

        public virtual void UpdateTemperature()
        {
            float fromA = GetLeftHeatingElement()?.GetProvidedHeat() ?? 0f;
            float fromB = GetRightHeatingElement()?.GetProvidedHeat() ?? 0f;

            float ambient = AmbientTemperature;

            // Final forge temperature is ambient + created.

            float finalTemp = ambient + fromA + fromB;

            CurrentTemperature = Mathf.MoveTowards(CurrentTemperature, finalTemp, 3);
        }

        /// <summary>
        /// Converts a resource into the most suitable type.
        /// For example, converts other mod's copper into RimForge copper.
        /// Uses the EquivalentResource system. See <see cref="AlloyHelper.AddEquivalentResource(ThingDef, ThingDef)"/>.
        /// If there are no equivalent resources available, or 
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public virtual ThingDef ConvertResource(ThingDef def)
        {
            if (def == null)
                return null;

            // TODO optimize.
            var options = AlloyHelper.GetEquivalentResources(def);
            if (options.Count == 1)
                return def;

            var found = options.FirstOrFallback(t => t.modContentPack == Core.ContentPack);
            return found ?? options.FirstOrFallback(); // Return the first item, which is generally the 'best fit'.
        }

        /// <summary>
        /// Called when a pawn has refueled this forge with a material.
        /// Return true to accept the material, and reset the fuel component ready to accept more fuel.
        /// Returning false will reject the input, placing it back into the world.
        /// </summary>
        public virtual bool UponResourceInput(ThingDef rawInput, int count)
        {
            // TODO check if this is the expected input.

            // Convert the type...
            ThingDef newType = ConvertResource(rawInput);

            requestCount = Rand.RangeInclusive(10, 50);
            CurrentFuelType = Rand.Chance(0.5f) ? RFDefOf.Gold : RFDefOf.Silver;
            Core.Log($"Input: {newType.LabelCap} x{count}. Now requesting {CurrentFuelType.LabelCap} x{requestCount}");

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

        /// <summary>
        /// Dumps all stored resources onto the ground.
        /// This is potentially a cheeky way to convert equivalent resources.
        /// </summary>
        public void DumpAllStored()
        {
            foreach (var pair in storedResources)
            {
                PlaceOutput(pair.Key, pair.Value);
            }
            storedResources.Clear();
        }

        public virtual Graphic GetCurrentGraphic()
        {
            bool isIdle = graphic == 0;
            bool isMeltingSides = graphic == 1;
            bool isMeltingAll = graphic == 2;

            if (Content.ForgeIdle == null)
                Content.LoadForgeTextures(this);

            if (isIdle)
                return Content.ForgeIdle;
            if (isMeltingSides)
                return Content.ForgeGlowSides;
            if (isMeltingAll)
                return Content.ForgeGlowAll;

            return Content.ForgeIdle;
        }

        public override void Draw()
        {
            base.Draw();

            if (CurrentAlloyDef == null || !CurrentAlloyDef.IsValid)
                return;

            var pos = DrawPos;
            pos.y += 0.0001f;

            // 1: hidden
            // 0: full show
            float lerp = Mathf.Lerp(1f, 0f, forgeLerp);

            // TODO make shader 
            void DrawMetal(Graphic graphic, Color color, float lerp)
            {
                color.a = 0.7f;
                graphic.MatSouth.color = color;
                float worldOffset = 3f * -lerp;
                Vector2 texOffset = new Vector2(0f, lerp);
                graphic.MatSouth.SetTextureOffset("_MainTex", texOffset);
                graphic.Draw(pos + new Vector3(0, 0, -worldOffset), Rotation, this);
            }

            Color? colorLeft = CurrentAlloyDef.GetMoltenColor(1);
            Color? colorRight = CurrentAlloyDef.GetMoltenColor(2);
            Color? colorMiddle = CurrentAlloyDef.input.Count > 2 ? CurrentAlloyDef.GetMoltenColor(0) : null;
            Color? colorOutput = CurrentAlloyDef.GetMoltenColor(3);

            if(colorLeft != null)
                DrawMetal(Content.ForgeMetalLeft, colorLeft.Value, lerp);
            if (colorRight != null)
                DrawMetal(Content.ForgeMetalRight, colorRight.Value, lerp);
            if (colorMiddle != null)
                DrawMetal(Content.ForgeMetalMiddle, colorMiddle.Value, lerp);
            if (colorOutput != null)
                DrawMetal(Content.ForgeMetalOut, colorOutput.Value, lerp);
        }

        public HeatingElement GetLeftHeatingElement()
        {
            return GetHeatingElement(ref elementA, false);
        }

        public HeatingElement GetRightHeatingElement()
        {
            return GetHeatingElement(ref elementB, true);
        }

        private HeatingElement GetHeatingElement(ref HeatingElement element, bool right)
        {
            if (element != null)
            {
                if (element.Destroyed)
                    element = null;
                else if (!element.Spawned)
                    element = null;
            }

            if (element == null)
            {
                // Find spot.
                IntVec3 spot = Position + Rotation.RighthandCell * (right ? 2 : -2);
                var heater = Map.thingGrid.ThingAt<HeatingElement>(spot);
                if (heater != null && (heater.ConnectedForge == null || heater.ConnectedForge == this))
                {
                    element = heater;
                    element.ConnectedForge = this;
                }
                else
                {
                    element = null;
                }
            }

            return element;
        }

        public virtual void ProcessCurrentRecipe(int maxIterations = int.MaxValue)
        {
            if (CurrentAlloyDef == null || !CurrentAlloyDef.IsValid)
            {
                Core.Error("Cannot process current recipe, recipe is null.");
                return;
            }

            int max = GetPossibleIterationCount(CurrentAlloyDef, out _);
            int realMax = Mathf.Min(max, maxIterations);
            if (realMax <= 0)
            {
                Core.Warn($"Called ProcessCurrentRecipe, but it is not possible to do even a single processing iteration (requested {maxIterations}).");
                return;
            }

            foreach (var resource in CurrentAlloyDef.input)
            {
                int toRemove = resource.count * realMax;
                ChangeStored(resource.resource, -toRemove);
                Core.Log($"Removed {resource.resource.ModLabelCap()} x{toRemove}...");
            }
            int toPlace = CurrentAlloyDef.output.count * realMax;
            PlaceOutput(CurrentAlloyDef.output.resource, toPlace);
            Core.Log($"Placed output: {CurrentAlloyDef.output.resource} x{toPlace}");
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

            if (storedResources.Count == 0)
                stored = "None";

            string connectedStatus =
                $"Connected:\n -{GetLeftHeatingElement()?.GetInspectorQuickString() ?? "None"}\n -{GetRightHeatingElement()?.GetInspectorQuickString() ?? "None"}";

            return $"{root}\nForge Temp.: {CurrentTemperature.ToStringTemperature()}\n{connectedStatus}\nStored:\n{stored}";
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    action = DumpAllStored,
                    defaultLabel = "dump all",
                    defaultDesc = "dumps all stored resources on to the ground."
                };
                yield return new Command_Action()
                {
                    action = () => ProcessCurrentRecipe(),
                    defaultLabel = "place output",
                    defaultDesc = "processes the current alloy recipe."
                };
                yield return new Command_Action()
                {
                    action = () => { WantsTemperatureIncrease = !WantsTemperatureIncrease; },
                    defaultLabel = "toggle wants temperature increase",
                    defaultDesc = $"Current: {WantsTemperatureIncrease}"
                };
                yield return new Command_Action()
                {
                    action = () =>
                    {
                        graphic++;
                        graphic %= 3;
                    },
                    defaultLabel = "next graphic",
                    defaultDesc = "dumps all stored resources on to the ground."
                };
                yield return new Command_Action()
                {
                    action = () =>
                    {
                        CurrentAlloyDef = AlloyHelper.AllAlloyDefs.RandomElement();
                    },
                    defaultLabel = "next alloy",
                    defaultDesc = "dumps all stored resources on to the ground."
                };
                yield return new Command_Action()
                {
                    action = () =>
                    {
                        forgeLerp = -1f;
                    },
                    defaultLabel = "reset lerp",
                    defaultDesc = "dumps all stored resources on to the ground."
                };
            }
        }
    }
}
