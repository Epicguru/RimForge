using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace RimForge.Comps
{
    public class JobDriver_CastReflectVerb : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.CastVerb(TargetIndex.A, false);
        }
    }
}
