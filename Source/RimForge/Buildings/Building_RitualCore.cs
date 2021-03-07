using System;
using System.Collections;
using System.Collections.Generic;
using RimForge.Effects;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_RitualCore : Building
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

        public float GearDrawSize = 12.2f, CircleDrawSize = 20, TextDrawSize = 8;
        public float GearDrawRot, CircleDrawRot, TextDrawRot;
        public float GearAlpha = 1, CircleAlpha = 1, TextAlpha = 1;

        public float GearTurnSpeed = -20f, TextTurnSpeed = 9f;
        public float CircleBaseAlpha = 0.7f, CircleAlphaMag = 0.06f;
        public float CircleAlphaFreq = 2, BallOffsetFreq = 0.24f;

        public float BallOffsetBase = 1;
        public float BallOffsetMag = 0.11f, BallDrawSize = 0.72f;

        private float ballOffset;
        private float timer;

        public override void Tick()
        {
            base.Tick();

            GearDrawRot += GearTurnSpeed / 60f;
            TextDrawRot += TextTurnSpeed / 60f;

            timer += 1f / 60f;

            CircleAlpha = Mathf.Sin(timer * Mathf.PI * 2f * CircleAlphaFreq) * CircleAlphaMag + CircleBaseAlpha;
            ballOffset = Mathf.Sin((timer + 12) * Mathf.PI * 2f * BallOffsetFreq) * BallOffsetMag + BallOffsetBase;

            var worldMousePos = UI.MouseMapPosition();

            var sparks = new RitualSparks();
            sparks.Position = DrawPos.WorldToFlat();
            sparks.GravitateTowards = worldMousePos.WorldToFlat();
            sparks.Velocity = Rand.InsideUnitCircle * 6f;
            sparks.Spawn(this.Map);
        }

        private IEnumerable<IntVec3> GetPillarPositions()
        {
            IntVec3 basePos = Position;

            yield return basePos + new IntVec3(7, 0, 0);
            yield return basePos - new IntVec3(7, 0, 0);

            yield return basePos + new IntVec3(0, 0, 7);
            yield return basePos - new IntVec3(0, 0, 7);

            yield return basePos + new IntVec3(5, 0, 5);
            yield return basePos - new IntVec3(5, 0, 5);

            yield return basePos + new IntVec3(-5, 0, 5);
            yield return basePos - new IntVec3(-5, 0, 5);
        }

        public override void Draw()
        {
            base.Draw();

            DrawGhosts();

            DrawRitualEffects();
        }

        private void DrawGhosts()
        {
            var map = this.Map;
            if (map == null)
                return;

            bool ShouldDrawAt(IntVec3 pos)
            {
                var thing = pos.GetFirstThing(map, RFDefOf.Column);
                if (thing != null && thing.Stuff == RFDefOf.RF_Copper)
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
    }
}
