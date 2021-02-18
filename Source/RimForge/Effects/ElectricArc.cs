using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    // An arc of electricity. ZZZT!
    public abstract class ElectricArc : MapEffect
    {
        public Vector2 Start;
        public Vector2 End;
        public readonly int IntermediatePointCount;
        public Vector2[] AllPoints;
        public Vector2 Amplitude = new Vector2(0.05f, 0.5f);
        public float Depth;

        protected ElectricArc(int intermediatePointCount)
        {
            this.IntermediatePointCount = Mathf.Max(1, intermediatePointCount);
            AllPoints = new Vector2[2 + IntermediatePointCount];
            Depth = AltitudeLayer.VisEffects.AltitudeFor();
        }

        public abstract Vector2 GetBasePosition(float t);
        public abstract Vector2 GetNormal(float t);

        public virtual float GetOffsetAt(float t)
        {
            return Rand.Range(Amplitude.x, Amplitude.y) * (Rand.Value < 0.5f ? 1f : -1f);
        }

        private static readonly Material mat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.MoteGlow, Color.cyan);
        public override void Draw(bool tick, Map map)
        {

            // Tick, then draw.
            if (tick)
            {
                for (int i = 1; i < AllPoints.Length - 1; i++)
                {
                    float t = i / (AllPoints.Length - 1f);
                    Vector2 basePos = GetBasePosition(t);
                    Vector2 normal = GetNormal(t);
                    float offset = GetOffsetAt(t);

                    Vector2 finalPos = basePos + normal * offset;
                    AllPoints[i] = finalPos;
                }
            }

            AllPoints[0] = Start;
            AllPoints[AllPoints.Length - 1] = End;
            Vector3 last = AllPoints[0].FlatToWorld(Depth);
            for (int i = 1; i < AllPoints.Length; i++)
            {
                Vector2 raw = AllPoints[i];
                Vector3 current = raw.FlatToWorld(Depth);
                GenDraw.DrawLineBetween(last, current, mat);
                last = current;
            }
        }
    }
}
