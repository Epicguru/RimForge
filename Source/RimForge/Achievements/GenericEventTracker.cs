using AchievementsExpanded;
using Verse;

namespace RimForge.Achievements
{
    public class GenericEventTracker : Tracker<AchievementEvent>
    {
        public static void Fire(AchievementEvent trigger)
        {
            if (trigger == AchievementEvent.None)
                return;

            var cards = AchievementPointManager.GetCards<GenericEventTracker>();
            if (cards == null)
                return;

            foreach (var item in cards)
            {
                if(item.tracker is GenericEventTracker ge)
                {
                    if (ge.Trigger(trigger))
                    {
                        item.UnlockCard();
                    }
                }
            }
        }

        public override string Key => nameof(GenericEventTracker);
        protected override string[] DebugText => new string[] { nameof(GenericEventTracker) };

        public AchievementEvent trigger;

        public GenericEventTracker() { }

        public GenericEventTracker(GenericEventTracker other)
            :base(other)
        {
            this.trigger = other.trigger;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref trigger, "trigger");
        }

        public override bool Trigger(AchievementEvent trigger)
        {
            return this.trigger == trigger;
        }
    }

    public enum AchievementEvent
    {
        None,
        CoilsFire,
        DroneAntimatter,
        DroneAntimatterFull
    }
}
