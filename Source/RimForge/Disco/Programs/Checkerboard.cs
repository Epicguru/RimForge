using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class Checkerboard : DiscoProgram
    {
        public Color ColorA = Color.yellow, ColorB = Color.red;
        public int SwapInterval = 20;
        public int SwapTime = 10;

        private float lerp;
        private int counter;
        private bool flipFlop;

        public Checkerboard(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            ColorA = Def.colors[0];
            ColorB = Def.colors[1];
            SwapInterval = Def.ints[0];
            SwapTime = Def.ints[1];
        }

        public override void Tick()
        {
            base.Tick();

            counter++;
            if (counter > SwapInterval)
            {
                lerp = Mathf.Clamp01((counter - SwapInterval) / (float)SwapTime);
                if(counter > SwapInterval + SwapTime)
                {
                    counter = 0;
                    lerp = 0f;
                    flipFlop = !flipFlop;
                }
            }
            else
            {
                lerp = 0f;
            }
        }

        public override Color ColorFor(IntVec3 cell)
        {
            bool isEven = (cell.x + cell.z) % 2 == 0;
            float l = flipFlop ? lerp : 1f - lerp;
            float p = isEven ? l : 1f - l;
            return Color.Lerp(ColorA, ColorB, p);
        }
    }
}
