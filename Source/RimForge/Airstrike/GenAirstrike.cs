using System.Collections.Generic;
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

        private static void MoteAt(IntVec3 cell)
        {
            MoteMaker.ThrowExplosionCell(cell, Find.CurrentMap, ThingDefOf.Mote_ExplosionFlash, Color.yellow);
        }

        [DebugAction("RimForge", "Airstrike (3)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DrawPoints3() => Debug_DrawPoints(3);

        [DebugAction("RimForge", "Airstrike (5)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DrawPoints5() => Debug_DrawPoints(5);

        [DebugAction("RimForge", "Airstrike (10)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DrawPoints10() => Debug_DrawPoints(10);

        [DebugAction("RimForge", "Airstrike (20)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DrawPoints20() => Debug_DrawPoints(20);

        [DebugAction("RimForge", "Airstrike (50)", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Debug_DrawPoints50() => Debug_DrawPoints(50);

        private static void Debug_DrawPoints(int count)
        {
            if (debug_FirstPoint == null)
            {
                debug_FirstPoint = UI.MouseCell();
                Messages.Message("First point selected, select second point.", MessageTypeDefOf.NeutralEvent);
                MoteAt(debug_FirstPoint.Value);
            }
            else
            {
                IntVec3 final = UI.MouseCell();
                int duration = GetAirstrikeDuration(debug_FirstPoint.Value, final);
                tempStrikes.Clear();

                var map = Find.CurrentMap;

                int index = 0;
                int startDelay = 120;
                foreach (var point in GeneratePoints(debug_FirstPoint.Value, final, count))
                {
                    MoteAt(point);

                    float p = index / (count - 1f);
                    int tick = Mathf.RoundToInt(duration * p) + startDelay;

                    var strike = new SingleStrike();
                    strike.Cell = point;
                    strike.ExplodeOnTick = tick;
                    tempStrikes.Add(strike);

                    // Bomb drop effect.
                    var bomb = new BombShadowEffect(point.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead), tick);
                    bomb.Spawn(map);

                    index++;
                }

                map.GetComponent<AirstrikeComp>().Spawn(new AirstrikeInstance(tempStrikes));
                tempStrikes.Clear();

                // Make drone shadow.
                IntVec3 droneStart = AtEdgeOfMap(map, debug_FirstPoint.Value, final, 1);
                IntVec3 droneEnd = AtEdgeOfMap(map, debug_FirstPoint.Value, final, -1);
                int time = Mathf.RoundToInt((droneStart - droneEnd).LengthHorizontal * 0.38f);

                var drone = new DroneShadowEffect(droneStart.ToVector3ShiftedWithAltitude(AltitudeLayer.Skyfaller), droneEnd.ToVector3ShiftedWithAltitude(AltitudeLayer.Skyfaller), time);
                drone.Spawn(map);

                debug_FirstPoint = null;
            }
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
            return Mathf.Max(Mathf.RoundToInt(dst * 1.5f), 1);
        }

        private static IEnumerable<IntVec3> GeneratePoints(IntVec3 start, IntVec3 end, int count)
        {
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
