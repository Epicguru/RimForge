using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimForge.Disco
{
    public class JobDriver_StandAtDJPlatform : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; 
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var reachPlatform = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            reachPlatform.finishActions ??= new List<System.Action>();
            reachPlatform.finishActions.Add(() =>
            {
                LookTargets t = new LookTargets(reachPlatform.actor);
                Messages.Message("RF.Disco.DJArrived".Translate(), t, MessageTypeDefOf.PositiveEvent);

                var lordJob = pawn.Map?.lordManager?.LordOf(pawn)?.LordJob;
                if (lordJob is LordJob_Joinable_Disco discoLord)
                {
                    var stand = discoLord.DJStand;
                    stand.PickSequenceIfNull = true;
                }
            });
            yield return reachPlatform;

            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate ()
            {
                Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.pawn.Position);
                pawn.pather.StopDead();
                pawn.rotationTracker.FaceCell(TargetB.Cell);
            };
            toil.finishActions ??= new List<System.Action>();
            toil.finishActions.Add(() =>
            {
                var lordJob = pawn.Map?.lordManager?.LordOf(pawn)?.LordJob;
                if (lordJob is LordJob_Joinable_Disco discoLord)
                {
                    var stand = discoLord.DJStand;
                    stand.PickSequenceIfNull = false;
                }
            });
            yield return toil;
        }
    }
}
