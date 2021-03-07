using System;
using System.Collections;
using System.Collections.Generic;
using RimForge.Comps;
using RimForge.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_RitualCore : Building, IConditionalGlower
    {
        [DebugAction("RimForge", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MakeRed()
        {
            MakeColor(Color.red);
        }

        [DebugAction("RimForge", null, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MakeDarkGrey()
        {
            MakeColor(new Color(0.2f, 0.2f, 0.2f ,1f));
        }

        private static void MakeColor(Color color)
        {
            foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
            {
                if (thing.TryGetComp<CompColorable>() != null)
                    thing.SetColor(color);
            }
        }

        [TweakValue("_RimForge", 0, 1)]
        private static float ArcDuration = 1;
        [TweakValue("_RimForge", 0, 1)]
        private static float ArcMag = 0.55f;
        [TweakValue("_RimForge", 0, 2)]
        private static float SymbolDrawSize = 1.2f;
        [TweakValue("_RimForge", 0, 1)]
        private static float SymbolDrawAlpha = 1f;
        [TweakValue("_RimForge", 0, 6)]
        private static float SymbolDrawOffset = 6f;
        [TweakValue("_RimForge", 0, 90f)]
        private static float SymbolDrawBA = 22.5f;

        public float GearDrawSize = 12.2f, CircleDrawSize = 20, TextDrawSize = 8;
        public float GearDrawRot, CircleDrawRot, TextDrawRot;
        public float GearAlpha = 1, CircleAlpha = 1, TextAlpha = 1;

        public float GearTurnSpeed = -20f, TextTurnSpeed = 9f;
        public float CircleBaseAlpha = 0.55f, CircleAlphaMag = 0.06f;
        public float CircleAlphaFreq = 2, BallOffsetFreq = 0.24f;

        public float BallOffsetBase = 1;
        public float BallOffsetMag = 0.11f, BallDrawSize = 0.72f;

        public bool DrawGuide = true;

        private float ballOffset;
        private float timer;
        private List<string> missing = new List<string>();
        private int tickCounter = -1;
        private List<(BezierElectricArc arc, float age)> arcs = new List<(BezierElectricArc arc, float age)>();
        private float timeToSparks = 1;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref DrawGuide, "rc_drawGuide", true);
        }

        public override void Tick()
        {
            base.Tick();

            // Update the missing display once per second.
            if (tickCounter == -1)
                tickCounter = Rand.RangeInclusive(0, 60);
            tickCounter++;
            if (tickCounter % 60 == 0)
                UpdateMissing();
            if (tickCounter % (60 * 5) == 0)
                SpawnDistorsion();

            // Turn the ritual gears.
            GearDrawRot += GearTurnSpeed / 60f;
            TextDrawRot += TextTurnSpeed / 60f;

            timer += 1f / 60f;

            CircleAlpha = Mathf.Sin(timer * Mathf.PI * 2f * CircleAlphaFreq) * CircleAlphaMag + CircleBaseAlpha;
            ballOffset = Mathf.Sin((timer + 12) * Mathf.PI * 2f * BallOffsetFreq) * BallOffsetMag + BallOffsetBase;

            bool spawnSparks = false;
            if (timeToSparks >= 0f)
            {
                timeToSparks -= 1f / 60f;
                if (timeToSparks <= 0f)
                {
                    spawnSparks = true;
                    timeToSparks = Rand.Range(2.2f, 3f);
                }
            }

            if (spawnSparks)
            {
                int a = Rand.Range(0, 8);
                int b = a + (Rand.RangeInclusive(1, 2) * (Rand.Chance(0.5f) ? 1 : -1));
                var start = GetPillarPosition(a).ToVector3().WorldToFlat() + new Vector2(0.5f, 1.2f);
                var end = GetPillarPosition(b).ToVector3().WorldToFlat() + new Vector2(0.5f, 1.2f);

                for (int i = 0; i < 2; i++)
                {
                    var arc = new BezierElectricArc(25);
                    Vector2 midA = Vector2.Lerp(start, end, 0.3f);
                    Vector2 midB = Vector2.Lerp(start, end, 0.7f);

                    arc.P0 = start;
                    arc.P1 = midA + new Vector2(0, 3);
                    arc.P2 = midB + new Vector2(0, 3);
                    arc.P3 = end;
                    arc.Yellow = true;

                    arc.Spawn(this.Map);
                    arcs.Add((arc, 0));
                }

                for (int i = 0; i < 2; i++)
                {
                    MoteMaker.ThrowLightningGlow(start, Map, 0.8f);
                    MoteMaker.ThrowLightningGlow(end, Map, 0.8f);
                }

                Vector2 gravTowards = Position.ToVector3Shifted().WorldToFlat();
                for (int i = 0; i < 15; i++)
                {
                    var sparks = new RitualSparks();
                    sparks.Position = start;
                    sparks.GravitateTowards = gravTowards;
                    sparks.Velocity = Rand.InsideUnitCircle.normalized * Rand.Range(0.5f, 6.5f);
                    sparks.Spawn(this.Map);

                    sparks = new RitualSparks();
                    sparks.Position = end;
                    sparks.GravitateTowards = gravTowards;
                    sparks.Velocity = Rand.InsideUnitCircle.normalized * Rand.Range(0.5f, 6.5f);
                    sparks.Spawn(this.Map);
                }
            }

            TickArcs();
        }

        private void TickArcs()
        {
            for(int i = 0; i < arcs.Count; i++)
            {
                var pair = arcs[i];

                float age = pair.age;
                var arc = pair.arc;

                age += 1f / 60f;
                arcs[i] = (arc, age);
                if(age > ArcDuration)
                {
                    arc.Destroy();
                    arcs.RemoveAt(i);
                    i--;
                    continue;
                }

                float amp = Mathf.Lerp(ArcMag, ArcMag * 0.2f, age / ArcDuration);

                arc.Amplitude = new Vector2(amp * 0.5f, amp);
            }
        }

        public IntVec3 GetPillarPosition(int index)
        {
            if (index < 0)
                index += 8000; // Just don't pass in index < 8000 please :)
            index %= 8;
            int i = 0;
            foreach (var item in GetPillarPositions())
            {
                if (i == index)
                    return item;
                i++;
            }

            return default;
        }

        public IEnumerable<IntVec3> GetPillarPositions()
        {
            IntVec3 basePos = Position;

            yield return basePos + new IntVec3(7, 0, 0);
            yield return basePos + new IntVec3(5, 0, 5);
            yield return basePos + new IntVec3(0, 0, 7);
            yield return basePos + new IntVec3(-5, 0, 5);
            yield return basePos + new IntVec3(-7, 0, 0);
            yield return basePos + new IntVec3(-5, 0, -5);
            yield return basePos + new IntVec3(0, 0, -7);
            yield return basePos + new IntVec3(5, 0, -5);

        }

        public bool IsPillarPresent(IntVec3 pos)
        {
            var thing = pos.GetFirstThing(Map, RFDefOf.Column);
            if (thing != null && thing.Stuff == RFDefOf.RF_Copper)
                return true;
            return false;
        }

        public void UpdateMissing()
        {
            missing.Clear();

            int missingPillars = 0;
            foreach (var pos in GetPillarPositions())
            {
                bool isThere = IsPillarPresent(pos);
                if (!isThere)
                    missingPillars++;
            }

            if(missingPillars > 0)
                missing.Add("RF.Ritual.Missing".Translate($"{RFDefOf.RF_Copper.LabelCap} {RFDefOf.Column.label}", missingPillars));
        }

        public override void Draw()
        {
            base.Draw();

            if(DrawGuide)
                DrawGhosts();

            DrawRitualEffects();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            string label = DrawGuide ? "RF.Ritual.DrawGuideHideLabel".Translate() : "RF.Ritual.DrawGuideShowLabel".Translate();
            string desc  = DrawGuide ? "RF.Ritual.DrawGuideHideDesc".Translate()  : "RF.Ritual.DrawGuideShowDesc".Translate();

            yield return new Command_Action()
            {
                defaultLabel = label,
                defaultDesc = desc,
                action = () =>
                {
                    DrawGuide = !DrawGuide;
                }
            };

            if (!Prefs.DevMode)
                yield break;

            yield return new Command_Action()
            {
                defaultLabel = "spawn distort mote",
                action = () =>
                {
                    SpawnDistorsion();
                }
            };
        }

        public void SpawnDistorsion()
        {
            Mote mote = (Mote)ThingMaker.MakeThing(RFDefOf.RF_Motes_RitualDistort, null);
            mote.Scale = 1;
            mote.exactRotation = 0;
            mote.exactPosition = DrawPos;
            GenSpawn.Spawn(mote, Position, Map, WipeMode.Vanish);
        }

        private void DrawGhosts()
        {
            var map = this.Map;
            if (map == null)
                return;

            bool ShouldDrawAt(IntVec3 pos)
            {
                if (IsPillarPresent(pos))
                    return false;

                int index = map.cellIndices.CellToIndex(pos);
                var bps = map.blueprintGrid.InnerArray[index];
                if (bps != null)
                {
                    foreach (var bp in bps)
                    {
                        if (bp == null)
                            continue;
                        
                        if (bp.def.entityDefToBuild == RFDefOf.Column)
                            return false;
                    }
                }

                int time = (int) (Time.realtimeSinceStartup * 2);
                return time % 2 == 0;
            }

            Color color = Color.Lerp(Color.red, Color.yellow, 0.5f);
            foreach (var pos in GetPillarPositions())
            {
                if (ShouldDrawAt(pos))
                    DrawGhost(pos, color);
            }
        }

        private void DrawGhost(IntVec3 at, Color color)
        {
            ThingDef blueprintDef = RFDefOf.Column.blueprintDef;
            GraphicDatabase.Get(blueprintDef.graphic.GetType(), blueprintDef.graphic.path, blueprintDef.graphic.Shader, blueprintDef.graphic.drawSize, color, Color.white, blueprintDef.graphicData, null).DrawFromDef(at.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), Rot4.North, RFDefOf.Column.blueprintDef);
        }

        private void DrawRitualEffects()
        {
            if (Content.RitualCircle == null)
                Content.LoadRitualGraphics(this);

            Vector3 drawPos = DrawPos + new Vector3(0, 0, -0.5f); // Because of the draw size of the ritual core.
            drawPos.y = AltitudeLayer.DoorMoveable.AltitudeFor();

            Content.RitualGear.drawSize = new Vector2(GearDrawSize, GearDrawSize);
            Content.RitualGear.MatNorth.color = new Color(0.9f, 0.15f, 0.15f, GearAlpha);
            Content.RitualGear.Draw(drawPos, Rot4.North, this, GearDrawRot);

            for (int i = 0; i < 8; i++)
            {
                var symbol = i % 2 == 0 ? Content.RitualSymbolA : Content.RitualSymbolB;
                symbol.drawSize = new Vector2(SymbolDrawSize, SymbolDrawSize);
                symbol.MatNorth.color = new Color(0.9f, 0.35f, 0.15f, SymbolDrawAlpha);
                float angle = (SymbolDrawBA + i * (360f / 8f) - GearDrawRot) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * SymbolDrawOffset;
                symbol.Draw(drawPos + offset, Rot4.North, this, 0f);
            }
            

            Content.RitualCircleText.drawSize = new Vector2(TextDrawSize, TextDrawSize);
            Content.RitualCircleText.MatNorth.color = new Color(0.9f, 0.15f, 0.15f, TextAlpha);
            Content.RitualCircleText.Draw(drawPos, Rot4.North, this, TextDrawRot);

            Content.RitualCircle.drawSize = new Vector2(CircleDrawSize, CircleDrawSize);
            Content.RitualCircle.MatNorth.color = new Color(0.9f, 0.15f, 0.15f, CircleAlpha);
            Content.RitualCircle.Draw(drawPos, Rot4.North, this, CircleDrawRot);

            drawPos.y = AltitudeLayer.VisEffects.AltitudeFor();
            Content.RitualBall.drawSize = new Vector2(BallDrawSize, BallDrawSize);
            Content.RitualBall.MatNorth.color = new Color(1f, 145f / 255f, 0f, 1f);
            Content.RitualBall.Draw(drawPos + new Vector3(0f, 0f, ballOffset), Rot4.North, this, 0f);
        }

        public bool ShouldGlowNow()
        {
            return true;
        }
    }
}
