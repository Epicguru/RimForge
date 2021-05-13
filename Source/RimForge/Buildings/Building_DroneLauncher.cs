using System.Collections.Generic;
using RimForge.Airstrike;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_DroneLauncher : Building, ICustomTargetingUser
    {
        private bool drawAffectedCells;
        private IntVec3? firstPosition;
        private IntVec3? secondPosition;
        private bool keepGoing;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            drawAffectedCells = false;
            firstPosition = null;
            secondPosition = null;
            keepGoing = false;
        }

        public override void Draw()
        {
            base.Draw();

            Content.DroneEast.Draw(DrawPos, default, this);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
                yield return item;

            yield return new Command_TargetCustom()
            {
                times = 2,
                defaultLabel = "RF.DL.TargetLabel".Translate(),
                defaultDesc = "RF.DL.TargetDesc".Translate(),
                icon = Content.MissilesIcon,
                defaultIconColor = Color.yellow,
                disabled = drawAffectedCells,
                disabledReason = "RF.DL.AlreadyTargeting".Translate(),
                continueCheck = () => keepGoing,
                action = (t, i) =>
                {
                    if (!t.IsValid)
                        return;

                    if (i == 0)
                    {
                        firstPosition = t.Cell;
                        keepGoing = true;
                    }
                    else
                        secondPosition = t.Cell;

                    if (i == 1)
                    {
                        GenAirstrike.DoStrike(firstPosition.Value, secondPosition.Value, 10);
                        firstPosition = null;
                        secondPosition = null;
                    }
                },
                user = this,
                targetingParams = new TargetingParameters()
                {
                    canTargetLocations = true,
                    canTargetPawns = false,
                    canTargetFires = false,
                    mapObjectTargetsMustBeAutoAttackable = false
                }
            };
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (!drawAffectedCells)
                return;

            if (firstPosition == null || secondPosition == null)
                return;

            GenAirstrike.DrawStrikePreview(firstPosition.Value, secondPosition.Value, Map, 10, 6f);
        }

        public void OnStartTargeting(int index)
        {
            drawAffectedCells = true;
            if (index == 0)
            {
                firstPosition = null;
                secondPosition = null;
                keepGoing = false;
            }
        }

        public void OnStopTargeting(int index)
        {
            drawAffectedCells = false;
        }

        public void SetTargetInfo(LocalTargetInfo info, int index)
        {
            if (!info.IsValid)
                return;

            if (index == 0)
                firstPosition = info.Cell;
            else
                secondPosition = info.Cell;
        }
    }
}
