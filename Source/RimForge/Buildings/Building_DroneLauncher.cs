using Verse;

namespace RimForge.Buildings
{
    public class Building_DroneLauncher : Building, ICustomTargetingUser
    {
        public bool DrawAffectedCells;
        public LocalTargetInfo CurrentTargetInfo;
        public IntVec3? FirstPosition;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            DrawAffectedCells = false;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (!DrawAffectedCells)
                return;

            
        }

        public void OnStartTargeting()
        {
            
        }

        public void OnStopTargeting()
        {
            
        }

        public void SetTargetInfo(LocalTargetInfo info)
        {
            
        }
    }
}
