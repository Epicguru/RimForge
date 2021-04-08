using RimWorld;
using System.Collections.Generic;
using System.Text;
using RimForge.Comps;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimForge.Buildings
{
    public class Building_ForgeRewritten : Building_WorkTable, IConditionalGlower
    {
        public int ConnectedHeatingElementCount => heatingElements?.Count ?? 0;
        public bool IsBeingUsed { get; private set; }

        public override Graphic Graphic
        {
            get
            {
                if (Content.ForgeIdle == null)
                    Content.LoadForgeTextures(this);

                bool triple = (workAlloyDef?.input?.Count ?? 0) >= 3;
                return IsBeingUsed ? triple ? Content.ForgeGlowAll : Content.ForgeGlowSides : Content.ForgeIdle;
            }
        }

        private HashSet<Building_HeatingElement> heatingElements = new HashSet<Building_HeatingElement>();
        private int tickCounter;
        private int ticksSinceUsed = 100;
        private float workPercentage = 0f;
        private AlloyDef workAlloyDef;

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
                int index = 0;
                foreach(var cell in GetHeatingElementLookCells())
                {
                    var thing = map.thingGrid.ThingAt<Building_HeatingElement>(cell);
                    if (thing != null)
                    {
                        if(!heatingElements.Contains(thing))
                            heatingElements.Add(thing);
                        thing.ForgeCellIndex = index;
                    }
                    index++;
                }
            }

            ticksSinceUsed++;
            if (ticksSinceUsed > 2)
                IsBeingUsed = false;

            if (!IsBeingUsed)
            {
                workPercentage = 0f;
                workAlloyDef = null;
            }
        }

        public override void UsedThisTick()
        {
            base.UsedThisTick();

            IsBeingUsed = true;
            ticksSinceUsed = 0;

            SanitizeHeatingElements();

            float totalHeat = 0f;
            foreach (var element in heatingElements)
            {
                totalHeat += element.TickActive();
            }

            Pawn actor = null;
            var things = InteractionCell.GetThingList(Map);
            if (things != null)
            {
                foreach (var thing in things)
                {
                    if (thing is Pawn pawn && pawn.CurJob?.bill?.billStack?.billGiver == this)
                        actor = pawn;
                }
            }

            if (actor != null)
            {
                var job = actor.CurJob;
                float percentage = (float)(1.0 - ((JobDriver_DoBill)actor.jobs.curDriver).workLeft / (double)job.bill.recipe.WorkAmountTotal(null));

                workPercentage = percentage;
                workAlloyDef = job.bill.recipe.TryGetAlloyDef();
            }
            else
            {
                Core.Warn("Failed to find pawn in interaction cell that is using this forge, but forge.UsedThisTick is being called!");
            }
            
        }

        public override void Draw()
        {
            base.Draw();

            if (!IsBeingUsed || workAlloyDef == null)
                return;

            var pos = DrawPos;
            pos.y += 0.0001f;

            // 1: hidden
            // 0: full show
            float lerp = Mathf.Lerp(1f, 0f, workPercentage);

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

            Color? colorLeft = workAlloyDef.GetMoltenColor(1);
            Color? colorRight = workAlloyDef.GetMoltenColor(2);
            Color? colorMiddle = workAlloyDef.input.Count > 2 ? workAlloyDef.GetMoltenColor(0) : null;
            Color? colorOutput = workAlloyDef.GetMoltenColor(3);

            if (colorLeft != null)
                DrawMetal(Content.ForgeMetalLeft, colorLeft.Value, lerp);
            if (colorRight != null)
                DrawMetal(Content.ForgeMetalRight, colorRight.Value, lerp);
            if (colorMiddle != null)
                DrawMetal(Content.ForgeMetalMiddle, colorMiddle.Value, lerp);
            if (colorOutput != null)
                DrawMetal(Content.ForgeMetalOut, colorOutput.Value, lerp);
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
            heatingElements ??= new HashSet<Building_HeatingElement>();
            heatingElements.RemoveWhere(h => h.DestroyedOrNull());
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

        private static readonly StringBuilder str = new StringBuilder();
        public override string GetInspectString()
        {
            str.Clear();

            string providing = "RF.Forge.Providing".Translate();
            str.AppendLine("RF.Forge.HeatingElementsHeader".Translate());
            foreach (var element in heatingElements)
            {
                if (element.DestroyedOrNull())
                    continue;

                str.Append(" •").Append(element.LabelCap).Append(providing).AppendLine(element.GetPotentialHeatIncrease().ToStringTemperatureOffset("F0"));
            }
            if (heatingElements.Count == 0)
                str.AppendLine("RF.None".Translate().CapitalizeFirst());

            return $"{base.GetInspectString()}\nTemperature: {AmbientTemperature.ToStringTemperature("F0")} (capable of {GetPotentialHeat().ToStringTemperature("F0")})\n{str.ToString().TrimEnd()}".Trim();
        }

        public bool ShouldGlowNow()
        {
            return IsBeingUsed;
        }
    }
}
