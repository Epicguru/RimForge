using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class Fade : DiscoProgram
    {
        public int Duration;
        public bool FadeIn;

        private float p;
        private int counter;
        private Color currentColor;

        public Fade(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            Duration = Def.ints[0];
            FadeIn = Def.bools[0];
        }

        public override void Tick()
        {
            base.Tick();

            if (counter > Duration)
                Remove();

            p = Mathf.Clamp01((float)counter / Duration);
            if (!FadeIn)
                p = 1f - p;

            currentColor = Color.Lerp(default, Color.white, p);

            counter++;
        }

        public override Color ColorFor(IntVec3 cell)
        {
            return currentColor;
        }
    }
}
