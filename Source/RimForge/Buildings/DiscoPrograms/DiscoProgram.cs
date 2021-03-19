using UnityEngine;
using Verse;

namespace RimForge.Buildings.DiscoPrograms
{
    public abstract class DiscoProgram
    {
        public readonly DiscoProgramDef Def;
        public Building_DJStand DJStand { get; set; }
        public int TickCounter { get; set; }

        protected DiscoProgram(DiscoProgramDef def)
        {
            this.Def = def;
        }

        public abstract void Init();

        public virtual void Tick()
        {

        }

        public abstract Color ColorFor(IntVec3 cell);
    }
}
