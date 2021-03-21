using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class Ripple : DiscoProgram
    {
        public Color ColorLow = new Color(0, 0, 0, 0), ColorHigh = Color.white;
        public Vector3 Centre;

        public float RadiusChangePerTick = 0.1f;
        public float DespawnAfterRadius = 10;
        public float Radius = 3f;
        public float Thickness = 2f;
        public bool Circular = true;

        public Ripple(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            ColorLow = Def.colors[0];
            ColorHigh = Def.colors[1];

            Radius = Def.floats[0];
            Thickness = Def.floats[1];
            RadiusChangePerTick = Def.floats[2];
            DespawnAfterRadius = Def.floats[3];

            Circular = Def.bools[0];
        }

        public override void Tick()
        {
            base.Tick();
            Centre = !Circular ? DJStand.FloorBounds.CenterCell.ToVector3() : DJStand.FloorBounds.CenterCell.ToVector3Shifted();

            Radius += RadiusChangePerTick;
            bool outwards = RadiusChangePerTick >= 0;
            if (outwards && Radius > DespawnAfterRadius)
                Remove();
            else if (!outwards && Radius < DespawnAfterRadius)
                Remove();
        }

        public override Color ColorFor(IntVec3 cell)
        {
            float dstFromCentre = !Circular ? (cell - Centre.ToIntVec3()).LengthManhattan : (cell.ToVector3Shifted() - Centre).magnitude;
            float dstFromTarget = Mathf.Abs(Radius - dstFromCentre);
            float lerp = DistanceToLerp(dstFromTarget);
            return Color.Lerp(ColorLow, ColorHigh, lerp);
        }

        public virtual float DistanceToLerp(float dst)
        {
            float pos = Thickness - dst;
            if (pos < 0f)
                return 0f;
            return pos / Thickness;
        }
    }
}
