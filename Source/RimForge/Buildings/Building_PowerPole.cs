using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_PowerPole : Building_LongDistanceCabled
    {
        public override string Name => "RF.PowerPoleName".Translate();

        public override Vector2 GetFlatConnectionPoint()
        {
            // NESW are 0123

            Vector2 root = DrawPos.WorldToFlat() + new Vector2(0, 0.5f);
            switch (Rotation.AsInt)
            {
                case 0:
                    return root + new Vector2(0, -0.0578f);
                case 1:
                    return root + new Vector2(0.291f, 0.2f);
                case 2:
                    return root + new Vector2(0, -0.0578f);
                case 3:
                    return root + new Vector2(-0.291f, 0.2f);
            }
            return root;
        }

        public override bool CanLinkTo(Building_LongDistancePower other)
        {
            // Do not allow connections to things that under roofs. The cable can't go through the roof.
            return base.CanLinkTo(other) && !other.Position.Roofed(other.Map);
        }
    }
}
