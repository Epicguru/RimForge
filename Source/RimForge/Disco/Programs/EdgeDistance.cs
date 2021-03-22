using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class EdgeDistance : DiscoProgram
    {
        public Color EdgeColor = Color.white, MiddleColor = new Color(1, 1, 1, 0);
        public float SolidDistance = 1;
        public float FadeDistance = 3;

        public EdgeDistance(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            
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
