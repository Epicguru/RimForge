using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public abstract class DiscoProgram
    {
        public virtual bool UseRandomTickOffset => true;

        public readonly DiscoProgramDef Def;
        public Building_DJStand DJStand { get; set; }
        public int TickCounter { get; set; }
        public bool ShouldRemove { get; private set; }
        public bool OneMinus = false;
        public bool OneMinusAlpha = false;
        public Color? Tint = null;

        protected DiscoProgram(DiscoProgramDef def)
        {
            this.Def = def;
            if (UseRandomTickOffset)
                TickCounter = Rand.Range(0, 1000);
        }

        public void Remove()
        {
            ShouldRemove = true;
        }

        public abstract void Init();

        public virtual void Tick()
        {

        }

        public abstract Color ColorFor(IntVec3 cell);
    }
}
