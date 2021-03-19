using UnityEngine;
using Verse;

namespace RimForge.Buildings.DiscoPrograms
{
    public abstract class DiscoProgram
    {
        public readonly DiscoProgramDef Def;
        public Building_DJStand DJStand { get; set; }
        public int TickCounter { get; set; } = Rand.Range(0, 1000);
        public bool ShouldRemove { get; private set; }
        public bool OneMinus = false;
        public Color? Tint = null;

        protected DiscoProgram(DiscoProgramDef def)
        {
            this.Def = def;
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
