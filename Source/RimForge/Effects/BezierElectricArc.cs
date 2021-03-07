using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public class BezierElectricArc : ElectricArc
    {
        private static readonly Material redMaterial = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.MoteGlow, Color.red);

        public override Vector2 Start { get => P0; set => P0 = value; }
        public override Vector2 End   { get => P3; set => P3 = value; }

        public Vector2 P0, P1, P2, P3;
        public bool Red;

        public BezierElectricArc(int intermediatePoints) : base(intermediatePoints)
        {

        }

        private Vector2 EvalBezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // This hot garbage is from stack overflow and I really can't be arsed to clean it up.
            // It works, and that's enough.

            float tt = t * t;
            float ttt = t * t * t;
            float u = 1 - t;
            float uuu = u * u * u;
            float q3 = uuu;
            // q1 and q2 changed:
            float q2 = 3f * ttt - 6f * tt + 3f * t;
            float q1 = -3f * ttt + 3f * tt;
            float q0 = ttt;
            Vector2 p = (p0 * q3 +
                         p1 * q2 +
                         p2 * q1 +
                         p3 * q0);
            // No division by 6.

            return p;
        }

        public override Material GetMaterial()
        {
            return Red ? redMaterial : base.GetMaterial();
        }

        public override Vector2 GetNormal(float t)
        {
            // Really crappy way but it works, I think...
            Vector2 a = EvalBezier(t, P0, P1, P2, P3);
            Vector2 b = EvalBezier(t - 0.001f, P0, P1, P2, P3);
            Vector2 dir = (a - b);
            return new Vector2(dir.y, -dir.x).normalized;
        }

        public override Vector2 GetBasePosition(float t)
        {
            if (t == 0)
                return P0;
            if (t == 1)
                return P3;
            return EvalBezier(t, P0, P1, P2, P3);
        }
    }
}
