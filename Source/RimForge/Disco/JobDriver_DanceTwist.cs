using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimForge.Disco
{
    public class JobDriver_DanceTwist : JobDriver_Dance
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var goToFloor = GoToDanceFloorOrMoveSlightly();
            if (goToFloor != null)
                yield return goToFloor;

            yield return Toils_General.StopDead();

            IntVec2 lastOff = default;
            for (int i = 0; i < 15; i++)
            {
                IntVec2 look = lastOff;
                while (look == lastOff)
                    look = Rand.Chance(0.5f)
                        ? new IntVec2(Rand.Chance(0.5f) ? 1 : -1, 0)
                        : new IntVec2(0, Rand.Chance(0.5f) ? 1 : -1);
                lastOff = look;
                yield return FaceDir(look.x, look.z);
                yield return Toils_General.Wait(20);
            }
        }
    }
}
