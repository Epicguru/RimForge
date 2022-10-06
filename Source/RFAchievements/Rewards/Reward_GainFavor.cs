using AchievementsExpanded;
using RimForge.Buildings;
using RimWorld;
using Verse;

namespace RimForge.Achievements.Rewards
{
    public class Reward_GainFavor : AchievementReward
    {
        public override string Disabled
        {
            get
            {
                string parent = base.Disabled;
                if (RitualTracker.Current == null || RitualTracker.Current.PlayerPerformedRituals <= 0)
                    parent += '\n' + "RF.AC.AlreadyGoodStanding".Translate();
                return parent;
            }
        }

        public override bool TryExecuteEvent()
        {
            if (RitualTracker.Current == null || RitualTracker.Current.PlayerPerformedRituals <= 0)
                return false;

            int count = --RitualTracker.Current.PlayerPerformedRituals;
            float chance = Building_RitualCore.ChanceToFailRitual(count);
            Messages.Message("RF.AC.IncreasedStanding".Translate($"{chance * 100f:F0}%"), MessageTypeDefOf.PositiveEvent);

            return true;
        }
    }
}
