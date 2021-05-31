using AchievementsExpanded;
using Verse;

namespace RimForge.Achievements
{
    public class ItemCraftTrackerWithCount : ItemCraftTracker
    {
        public ItemCraftTrackerWithCount() { }

        public ItemCraftTrackerWithCount(ItemCraftTrackerWithCount other)
            : base(other)
        {

        }

        public override bool Trigger(Thing thing)
        {
            bool done = false;
            for (int i = 0; i < thing.stackCount; i++)
            {
                if (base.Trigger(thing))
                    done = true;
            }
            return done;
        }
    }
}
