using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class EdgeDistance : DiscoProgram
    {
        public Color EdgeColor, MiddleColor;
        public float SolidDistance;
        public float FadeDistance;

        public EdgeDistance(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            EdgeColor = Def.Get("edgeColor", Color.white);
            MiddleColor = Def.Get("middleColor", new Color(0, 0, 0, 0));
            SolidDistance = Def.Get("solidDistance", 1f);
            FadeDistance = Def.Get("fadeDistance", 3f);
        }

        protected virtual float GetColorLerp(float dst)
        {
            // Where 0 is edge and 1 is middle.

            if (dst < SolidDistance)
                return 0f;

            if (dst >= SolidDistance + FadeDistance)
                return 1f;

            return (dst - SolidDistance) / FadeDistance;
        }

        public override Color ColorFor(IntVec3 cell)
        {
            float dst = DJStand.GetCellDistanceFromEdge(cell);
            return Color.Lerp(EdgeColor, MiddleColor, GetColorLerp(dst));
        }
    }
}
