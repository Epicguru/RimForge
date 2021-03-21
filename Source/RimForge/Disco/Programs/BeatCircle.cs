using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class BeatCircle : DiscoProgram
    {
        public override bool UseRandomTickOffset => false;

        public Color MiddleColor = Color.red, OtherColor = Color.white;
        public Vector3 Centre;
        public bool Circular = true;
        public float BaseRadius = 3;
        public float BlendDst = 1f;
        public float BeatVel = 1f;
        public float BeatRecovery = 0.98f;
        public float VelRecovery = 0.85f;
        public int BeatInterval = 30;

        private float vel;
        private float radius;
        private float radiusAdd;

        public BeatCircle(DiscoProgramDef def) : base(def) { }

        public override void Init()
        {
            MiddleColor = Def.colors[0];
            OtherColor = Def.colors[1];

            BeatInterval = Def.ints[0];

            Circular = Def.bools[0];

            BaseRadius = Def.floats[0];
            BlendDst = Def.floats[1];
            BeatVel = Def.floats[2];
            BeatRecovery = Def.floats[3];
            VelRecovery = Def.floats[4];
        }

        public override void Tick()
        {
            base.Tick();
            Centre = !Circular ? DJStand.FloorBounds.CenterCell.ToVector3() : DJStand.FloorBounds.CenterCell.ToVector3Shifted();

            if (TickCounter % BeatInterval == 0)
            {
                vel = BeatVel;
            }

            radiusAdd += vel;
            radius = BaseRadius + radiusAdd;

            vel *= VelRecovery;
            radiusAdd *= BeatRecovery;
        }

        public override Color ColorFor(IntVec3 cell)
        {
            float dstFromCentre = !Circular ? (cell - Centre.ToIntVec3()).LengthManhattan : (cell.ToVector3Shifted() - Centre).magnitude;
            float dstFromTarget = Mathf.Abs(radius - dstFromCentre);
            float lerp = DistanceToLerp(dstFromTarget);
            return Color.Lerp(MiddleColor, OtherColor, lerp);
        }

        public virtual float DistanceToLerp(float dst)
        {
            if (dst <= radius)
                return 0f;
            if (dst <= radius + BlendDst)
                return Mathf.Clamp01((dst - radius) / BlendDst);
            return 1f;
        }
    }
}
