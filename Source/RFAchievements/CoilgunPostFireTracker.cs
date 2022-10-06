using AchievementsExpanded;
using Verse;

namespace RimForge.Achievements
{
    public abstract class CoilgunPostFireTracker : Tracker3<int, float, CoilgunShellDef>
    {
        public override string Key => nameof(CoilgunPostFireTracker);
        protected override string[] DebugText => new string[] {nameof(CoilgunPostFireTracker) };

        protected CoilgunPostFireTracker() {}

        protected CoilgunPostFireTracker(CoilgunPostFireTracker other)
            :base(other)
        {

        }

        public override bool Trigger(int kills, float totalDamage, CoilgunShellDef shellDef)
        {
            return false;
        }
    }
}
