using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class Stripes : DiscoProgram
    {
        public int EveryX = 2;
        public int ShiftInterval = 20;
        public int ShiftDirection = 1;
        public bool Horizontal = false;

        private int offset;

        public Stripes(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            EveryX = Def.ints[0];
            if (EveryX < 1)
                EveryX = 1;
            ShiftInterval = Def.ints[1];
            ShiftDirection = Def.ints[2];
            Horizontal = Def.bools[0];
        }

        public override void Tick()
        {
            base.Tick();

            if (TickCounter % ShiftInterval == 0)
            {
                offset += ShiftDirection;
            }
        }

        public override Color ColorFor(IntVec3 cell)
        {
            int coord = Horizontal ? cell.z : cell.x;
            coord += offset;
            if (coord % EveryX != 0)
                return default;

            return Def.colors[(coord / EveryX) % Def.colors.Count];
        }
    }
}
