using UnityEngine;
using Verse;

namespace RimForge.Buildings.DiscoPrograms
{
    public class ColorCycle : DiscoProgram
    {
        public int FadeTicks;

        private int currentIndex;
        private Color currentColor;
        private int counter;

        public ColorCycle(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            FadeTicks = Def.ints[0];
        }

        public override void Tick()
        {
            base.Tick();

            counter++;
            if (counter >= FadeTicks)
            {
                counter = 0;
                currentIndex++;
                currentIndex %= Def.colors.Count;
            }
            float p = Mathf.Clamp01((float)counter / FadeTicks);

            Color colorNow = Def.colors[currentIndex];
            int nextIndex = currentIndex == Def.colors.Count - 1 ? 0 : currentIndex + 1;
            Color nextColor = Def.colors[nextIndex];

            currentColor = Color.Lerp(colorNow, nextColor, p);
        }

        public override Color ColorFor(IntVec3 cell)
        {
            return currentColor;
        }
    }
}
