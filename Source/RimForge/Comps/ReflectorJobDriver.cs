using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace RimForge.Comps
{
    public class ReflectorJobDriver : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.CastVerb(TargetIndex.A, false);
        }
    }
}
