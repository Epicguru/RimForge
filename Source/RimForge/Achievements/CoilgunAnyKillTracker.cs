using AchievementsExpanded;
using Verse;

namespace RimForge.Achievements
{
    public class CoilgunAnyKillTracker : KillTracker
    {
        public CoilgunAnyKillTracker() {}

        public CoilgunAnyKillTracker(CoilgunAnyKillTracker other)
            : base(other)
        { }

        public override bool Trigger(Pawn pawn, DamageInfo? dinfo)
        {
            if (pawn == null)
                return false;

            if (killedThings.Contains(pawn.GetUniqueLoadID()))
                return false;

            if (dinfo.HasValue)
                Core.Log($"Instigator: {dinfo.Value.Instigator} ({dinfo?.Instigator?.def == RFDefOf.RF_Coilgun})");

            if (dinfo?.Instigator?.def != RFDefOf.RF_Coilgun)
                return false;

            bool result = base.Trigger(pawn, dinfo);
            return result;
        }
    }
}
