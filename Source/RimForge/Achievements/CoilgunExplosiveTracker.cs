using AchievementsExpanded;
using Verse;

namespace RimForge.Achievements
{
    public class CoilgunExplosiveTracker : Tracker2<Explosion, int>
    {
        public override string Key => nameof(CoilgunExplosiveTracker);
        protected override string[] DebugText => new string[] {nameof(CoilgunExplosiveTracker) };

        public int minKills;

        public CoilgunExplosiveTracker() {}

        public CoilgunExplosiveTracker(CoilgunExplosiveTracker other)
            :base(other)
        {
            this.minKills = other.minKills;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref minKills, "minKills");
        }

        public override bool Trigger(Explosion e, int kills)
        {
            return e != null && kills >= minKills;
        }
    }
}
