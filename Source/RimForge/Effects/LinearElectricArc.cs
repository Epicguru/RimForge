using UnityEngine;

namespace RimForge.Effects
{
    public class LinearElectricArc : ElectricArc
    {
        public LinearElectricArc(int intermediatePoints) : base(intermediatePoints)
        {

        }

        public override Vector2 GetNormal(float t)
        {
            Vector2 dir = (Start - End).normalized;
            return new Vector2(dir.y, -dir.x);
        }

        public override Vector2 GetBasePosition(float t)
        {
            return Vector2.Lerp(Start, End, t);
        }
    }
}
