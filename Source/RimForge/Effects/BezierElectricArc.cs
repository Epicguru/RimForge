using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public class BezierElectricArc : ElectricArc
    {
        private static readonly Material redMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.MoteGlow, new Color(1f, 115f / 255f, 0));

        public override Vector2 Start { get => P0; set => P0 = value; }
        public override Vector2 End   { get => P3; set => P3 = value; }

        public Vector2 P0, P1, P2, P3;
        public bool Yellow;

        public BezierElectricArc(int intermediatePoints) : base(intermediatePoints)
        {

        }

        public override Material GetMaterial()
        {
            return Yellow ? redMaterial : base.GetMaterial();
        }

        public override Vector2 GetNormal(float t)
        {
            // Really crappy way but it works, I think...
            Vector2 a = Bezier.Evaluate(t, P0, P1, P2, P3);
            Vector2 b = Bezier.Evaluate(t - 0.001f, P0, P1, P2, P3);
            Vector2 dir = (a - b);
            return new Vector2(dir.y, -dir.x).normalized;
        }

        public override Vector2 GetBasePosition(float t)
        {
            if (t == 0)
                return P0;
            if (t == 1)
                return P3;
            return Bezier.Evaluate(t, P0, P1, P2, P3);
        }
    }
}
