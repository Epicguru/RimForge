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

        [TweakValue("_RimForge", 0, 24)]
        public static float GearDrawSize = 12.2f, CircleDrawSize = 20, TextDrawSize = 8;
        [TweakValue("_RimForge", 0, 360)]
        public static float GearDrawRot, CircleDrawRot, TextDrawRot;
        [TweakValue("_RimForge", 0, 1)]
        public static float GearAlpha = 1, CircleAlpha = 1, TextAlpha = 1;

        [TweakValue("_RimForge", -360, 360)]
        public static float GearTurnSpeed = -20f, TextTurnSpeed = 9f;
        [TweakValue("_RimForge", 0f, 1f)]
        public static float CircleBaseAlpha = 0.7f, CircleAlphaMag = 0.06f;
        [TweakValue("_RimForge", 0f, 20f)]
        public static float CircleAlphaFreq = 2, BallOffsetFreq = 0.24f;

        [TweakValue("_RimForge", 0f, 5f)]
        public static float BallOffsetBase = 1;
        [TweakValue("_RimForge", 0f, 5f)]
        public static float BallOffsetMag = 0.11f, BallDrawSize = 0.72f;

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
            sparks.Velocity = Rand.InsideUnitCircle * 15f;
            sparks.FixedLength = 1f;
            sparks.Spawn(this.Map);
        }

        public override void Draw()
        {
            base.Draw();

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
