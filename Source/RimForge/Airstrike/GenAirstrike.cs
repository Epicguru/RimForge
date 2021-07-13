using System.Collections.Generic;
using RimForge.CombatExtended;
using RimForge.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Airstrike
{
    public static class GenAirstrike
    {
        private static IntVec3? debug_FirstPoint;
        private static List<SingleStrike> tempStrikes = new List<SingleStrike>();
        private static List<IntVec3> tempCells = new List<IntVec3>();

        private static void MoteAt(IntVec3 cell)
        {
#if V13
            FleckMaker.ThrowExplosionCell(cell, Find.CurrentMap, FleckDefOf.ExplosionFlash, Color.yellow);
#else
            MoteMaker.ThrowExplosionCell(cell, Find.CurrentMap, ThingDefOf.Mote_ExplosionFlash, Color.yellow);
#endif

            // Legacy 1.2:
            //MoteMaker.ThrowExplosionCell(cell, Find.CurrentMap, ThingDefOf.Mote_ExplosionFlash, Color.yellow);
        }

        [DebugAction("RimForge", "Airstrike (3)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DoStrike3() => Debug_DoStrike(3);

        [DebugAction("RimForge", "Airstrike (5)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DoStrike5() => Debug_DoStrike(5);

        [DebugAction("RimForge", "Airstrike (10)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DoStrike10() => Debug_DoStrike(10);

        [DebugAction("RimForge", "Airstrike (20)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DoStrike20() => Debug_DoStrike(20);

        [DebugAction("RimForge", "Airstrike (50)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DoStrike50() => Debug_DoStrike(50);

        private static void Debug_DoStrike(int count)
        {
            if (debug_FirstPoint == null)
            {
                debug_FirstPoint = UI.MouseCell();
                Messages.Message("First point selected, select second point.", MessageTypeDefOf.NeutralEvent);
                MoteAt(debug_FirstPoint.Value);
            }
            else
            {
                var bomb = CECompat.IsCEActive ? CECompat.GetProjectile(RFDefOf.Shell_HighExplosive) : RFDefOf.Shell_HighExplosive.projectileWhenLoaded;
                DoStrike(null, bomb, debug_FirstPoint.Value, UI.MouseCell(), count);
                debug_FirstPoint = null;
            }
        }

        public static void DoStrike(Thing instigator, ThingDef bombDef, IntVec3 start, IntVec3 end, int bombCount, int delayTicks = 0, SoundDef playAfterDelay = null)
        {
            bool isSingle = start == end;
            if (isSingle)
                bombCount = 1;

            int duration = GetAirstrikeDuration(start, end);
            tempStrikes.Clear();

            var map = Find.CurrentMap;

            int index = 0;
            int startDelay = 120 + delayTicks;
            foreach (var point in GeneratePoints(start, end, bombCount))
            {
                MoteAt(point);

                float p = isSingle ? 0f : index / (bombCount - 1f);
                int tick = Mathf.RoundToInt(duration * p) + startDelay;

                var strike = new SingleStrike();
                strike.Cell = point;
                strike.ExplodeOnTick = tick;
                strike.ProjectileDef = bombDef;
                tempStrikes.Add(strike);

                // Bomb drop effect.
                var bomb = new BombShadowEffect(point.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), tick);
                bomb.Spawn(map);

                index++;
            }

            map.GetComponent<AirstrikeComp>().Spawn(new AirstrikeInstance(tempStrikes) { Instigator = instigator, DelayTicks = delayTicks, SoundAfterDelay = playAfterDelay } );
            tempStrikes.Clear();

            // Make drone shadow.
            if(start == end)
                end += new IntVec3(Rand.InsideUnitCircleVec3 * 50f);
            IntVec3 droneStart = AtEdgeOfMap(map, start, end, 1);
            IntVec3 droneEnd = AtEdgeOfMap(map, start, end, -1);
            int time = Mathf.RoundToInt((droneStart - droneEnd).LengthHorizontal * 0.38f);

            var drone = new DroneShadowEffect(droneStart.ToVector3ShiftedWithAltitude(AltitudeLayer.Skyfaller), droneEnd.ToVector3ShiftedWithAltitude(AltitudeLayer.Skyfaller), time, delayTicks);
            drone.Spawn(map);
        }

        public static void DrawStrikePreview(IntVec3 start, IntVec3 end, Map map, int bombCount, float explosionRadius)
        {
            if (start == end)
            {
                GenExplosion.RenderPredictedAreaOfEffect(start, explosionRadius);
                return;
            }

            int index = 0;
            tempCells.Clear();
            foreach(var cell in GeneratePoints(start, end, bombCount))
            {
                tempCells.Add(cell);

                int t = (int)(Time.unscaledTime * 6f);
                t = t % bombCount;
                bool drawRadius = t == index;
                bool thickRoof = cell.GetRoof(map)?.isThickRoof ?? false;

                if (explosionRadius > 0 && drawRadius && !thickRoof)
                {
                    GenExplosion.RenderPredictedAreaOfEffect(cell, explosionRadius);
                }

                index++;
            }
            GenDraw.DrawFieldEdges(tempCells, Color.red);
        }

        private static IntVec3 AtEdgeOfMap(Map map, IntVec3 a, IntVec3 b, int sign)
        {
            Vector3 dir = (a.ToVector3Shifted() - b.ToVector3Shifted()).normalized;
            const float STEP = 10f;

            if (a == b)
                return a;

            var bounds = new BoundsInt(0, 0, 0, map.Size.x, map.Size.y, map.Size.z);

            Vector3 current = a.ToVector3Shifted();
            while (true)
            {
                current += dir * STEP * sign;
                IntVec3 pos = current.ToIntVec3();
                if (!bounds.Contains(new Vector3Int(pos.x, pos.y, pos.z)))
                    return pos;
            }
        }

        private static int GetAirstrikeDuration(IntVec3 from, IntVec3 to)
        {
            float dst = (from - to).LengthHorizontal;
            return Mathf.Max(Mathf.RoundToInt(dst * 1.5f), 0);
        }

        private static IEnumerable<IntVec3> GeneratePoints(IntVec3 start, IntVec3 end, int count)
        {
            if (start == end)
            {
                yield return start;
                yield break;
            }

            Vector3 startFloat = start.ToVector3();
            Vector3 endFloat = end.ToVector3();

            for (int i = 0; i < count; i++)
            {
                float p = i / (count - 1f);
                Vector3 lerped = Vector3.Lerp(startFloat, endFloat, p);
                yield return lerped.ToIntVec3();
            }
        }
    }
}
