using AchievementsExpanded;
using Verse;

namespace RimForge.Achievements
{
    public abstract class CoilgunKillTracker : Tracker3<int, Pawn, CoilgunShellDef>
    {
        public override string Key => nameof(CoilgunKillTracker);
        protected override string[] DebugText => new string[] {nameof(CoilgunKillTracker)};

        protected CoilgunKillTracker() {}

        protected CoilgunKillTracker(CoilgunKillTracker other)
            :base(other)
        {

        }
    }
}
