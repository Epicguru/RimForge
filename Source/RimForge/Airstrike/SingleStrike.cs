using RimWorld;
using Verse;

namespace RimForge.Airstrike
{
    public class SingleStrike : IExposable
    {
        public bool IsDone { get; private set; }
        public IntVec3 Cell;
        public int ExplodeOnTick;

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref Cell, "cell");
            Scribe_Values.Look(ref ExplodeOnTick, "tick");
        }

        public virtual void Tick(AirstrikeInstance instance, int tick)
        {
            if (IsDone)
                return;

            IsDone = tick >= ExplodeOnTick;
            if (IsDone)
            {
                // Check for overhead mountain.
                var roof = Cell.GetRoof(instance.Map);
                if (roof != null && roof.isThickRoof)
                    return;

                // Explode!
                GenExplosion.DoExplosion(Cell, instance.Map, 6, DamageDefOf.Bomb, instance.Instigator);
            }
        }
    }
}
