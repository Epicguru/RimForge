using Verse;

namespace RimForge.Buildings
{
    public abstract class HeatingElement : Building
    {
        public HeatingElementDef HEDef => def as HeatingElementDef;

        /// <summary>
        /// Gets the current heat offset that this provides to a forge,
        /// in degrees celsius. Should return 0 when inactive.
        /// </summary>
        public abstract float GetProvidedHeat();
    }
}
