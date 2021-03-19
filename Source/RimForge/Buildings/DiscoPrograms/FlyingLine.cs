using UnityEngine;
using Verse;

namespace RimForge.Buildings.DiscoPrograms
{
    public class FlyingLine : DiscoProgram
    {
        public Color LineColor = Color.white, DefaultColor = default;
        public int MoveInterval = 10;
        public bool Forwards = true;

        private Direction direction;
        private IntVec3 cacheMoveDir;
        private IntVec3 lineCell;

        public FlyingLine(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            LineColor = Def.colors[0];
            DefaultColor = Def.colors[1];

            MoveInterval = Def.ints[0];
            direction = (Direction)Def.ints[1];

            Forwards = Def.bools[0];

            var rect = DJStand.FloorBounds;
            var bl = new IntVec3(rect.minX - 1, 0, rect.minZ - 1);
            var br = new IntVec3(rect.maxX + 1, 0, rect.minZ - 1);
            var tl = new IntVec3(rect.minX - 1, 0, rect.maxZ + 1);
            var tr = new IntVec3(rect.maxX + 1, 0, rect.maxZ + 1);

            switch (direction)
            {
                case Direction.Horizontal:
                    cacheMoveDir = new IntVec3(1, 0, 0) * (Forwards ? 1 : -1);
                    lineCell = Forwards ? bl : br;
                    break;
                case Direction.Vertical:
                    cacheMoveDir = new IntVec3(0, 0, 1) * (Forwards ? 1 : -1);
                    lineCell = Forwards ? bl : tl;
                    break;
                case Direction.Diagonal:
                    cacheMoveDir = new IntVec3(1, 0, 1) * (Forwards ? 1 : -1);
                    lineCell = Forwards ? bl : tr;
                    break;
                case Direction.DiagonalInverted:
                    cacheMoveDir = new IntVec3(1, 0, -1) * (Forwards ? 1 : -1);
                    lineCell = Forwards ? tl : br;
                    break;
            }
        }

        public virtual bool IsOnLine(IntVec3 cell)
        {
            IntVec3 min = new IntVec3(DJStand.FloorBounds.minX, 0, DJStand.FloorBounds.minZ);
            IntVec3 localCell = cell - min;
            IntVec3 localLine = lineCell - min;
            switch (direction)
            {
                case Direction.Horizontal:
                    return cell.x == lineCell.x;
                case Direction.Vertical:
                    return cell.z == lineCell.z;
                case Direction.Diagonal:
                    return localCell.x == localCell.z && localCell.x == localLine.x;
                case Direction.DiagonalInverted:
                    return localCell.x == (DJStand.FloorBounds.maxZ - localCell.z) && localCell.x == localLine.x;
            }
            return false;
        }

        public override void Tick()
        {
            base.Tick();

            if (TickCounter % MoveInterval == 0)
            {
                lineCell += cacheMoveDir;
            }
        }

        public override Color ColorFor(IntVec3 cell)
        {
            return IsOnLine(cell) ? LineColor : DefaultColor;
        }

        private enum Direction
        {
            Horizontal,
            Vertical,
            Diagonal,
            DiagonalInverted
        }
    }
}
