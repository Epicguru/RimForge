using Verse;

namespace RimForge.Buildings
{
    public abstract class HeatingElement : Building
    {
        public HeatingElementDef HEDef => def as HeatingElementDef;

        public bool IsForgeRunning { get; private set; }
        public int ForgeCellIndex { get; internal set; }

        private int ticksSinceActive = 100;

        public virtual float GetPotentialHeatIncrease()
        {
            return HEDef.maxAddedHeat;
        }

        public override void Tick()
        {
            base.Tick();

            ticksSinceActive++;
            if (ticksSinceActive > 2)
                IsForgeRunning = false;
        }

        public virtual float TickActive()
        {
            IsForgeRunning = true;
            ticksSinceActive = 0;
            return 0f;
        }

        public override string GetInspectString()
        {
            return $"{base.GetInspectString()}\nIsForgeRunning: {IsForgeRunning}".Trim();
        }
    }
}
