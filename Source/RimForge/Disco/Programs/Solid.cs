using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class Solid : DiscoProgram
    {
        public Color Color;

        public Solid(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            Color = Def.Get("color", Color.white);
        }

        public override Color ColorFor(IntVec3 cell)
        {
            return Color;
        }
    }
}
