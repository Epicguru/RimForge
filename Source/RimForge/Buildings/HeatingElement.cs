using Verse;

namespace RimForge.Buildings
{
    public abstract class HeatingElement : Building
    {
        public HeatingElementDef HEDef => def as HeatingElementDef;

        public virtual float GetPotentialHeatIncrease()
        {
            return HEDef.maxAddedHeat;
        }

        public abstract float TickActive();
    }
}
