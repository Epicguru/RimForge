using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public class FollowBezierSpark : MapEffect
    {
        public Vector2 Position;
        public Vector2 Start, End;
        public int Ticks;
        public Color Color = Color.red;

        private Vector2[] points;
        private int tickCounter;
        private Vector2 oldPos;
        private Material mat;

        public override void Draw(bool tick, Map map)
        {
            if (mat == null)
                mat = RitualSparks.GetMat(Color, false);

            float y = AltitudeLayer.Projectile.AltitudeFor();
            Vector3 end = Position.FlatToWorld(y);
            Vector3 start = (Position + (oldPos - Position).normalized * 0.6f).FlatToWorld(y);
            GenDraw.DrawLineBetween(end, start, mat);
            //Core.Log($"Draw from {start} to {end} ({oldPos == Position})");
        }

        public override void TickAccurate()
        {
            base.TickAccurate();
            Tick();
            tickCounter++;
        }

        private void Tick()
        {
            if(tickCounter >= Ticks)
            {
                Destroy();
                return;
            }

            if (points == null)
            {
                float dst = (Start - End).magnitude;
                float height = Mathf.Lerp(0.2f, 2f, dst / 25f);
                Vector2 midA = Vector2.Lerp(Start, End, 0.3f) + new Vector2(0, height);
                Vector2 midB = Vector2.Lerp(Start, End, 0.7f) + new Vector2(0, height);
                int pointCount = Ticks;
                points = new Vector2[pointCount];

                for (int i = 0; i < pointCount; i++)
                {
                    float p = i / (pointCount - 1f);
                    points[i] = Bezier.Evaluate(p, Start, midA, midB, End);
                }
            }

            oldPos = tickCounter == 0 ? points[0] : points[tickCounter - 1];
            Position = points[tickCounter];
        }
    }
}
