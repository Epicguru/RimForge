using RimWorld;
using Verse;

namespace RimForge.Thoughts
{
    public class ThoughtWorker_Corrupted : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return IsCursed(p);
        }

        private bool IsCursed(Pawn pawn)
        {
            var traits = pawn?.story?.traits;
            if (traits == null)
                return false;

            return traits.HasTrait(RFDefOf.RF_ZirsCorruption);
        }
    }
}
