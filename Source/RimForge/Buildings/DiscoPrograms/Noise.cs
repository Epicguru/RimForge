using UnityEngine;
using Verse;

namespace RimForge.Buildings.DiscoPrograms
{
    public class Noise : DiscoProgram
    {
        public float Scale = 2;
        public float Add = 0.5f;

        public Noise(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            Scale = Def.floats[0];
            if ((int) Scale == Scale)
                Scale += 0.02f;
            Add = Def.floats[1];
        }

        public override Color ColorFor(IntVec3 cell)
        {
            float perlin = Mathf.PerlinNoise((cell.x + 0.2451f) * Scale, (cell.z + 0.2451f) * Scale);
            float n = Mathf.Clamp01(perlin + Add);
            return new Color(n, n, n, 1);
        }
    }
}
