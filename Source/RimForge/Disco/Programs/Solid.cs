using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class Solid : DiscoProgram
    {
        public Color Color = Color.white;

        public Solid(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            Color = Def.colors[0];
        }

        public override Color ColorFor(IntVec3 cell)
        {
            return Color;
        }
    }
}
