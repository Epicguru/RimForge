using System.Collections.Generic;
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

                int index = 0;
                foreach (var point in GeneratePoints(debug_FirstPoint.Value, final, count))
                {
                    MoteAt(point);

                    float p = index / (count - 1f);
                    int tick = Mathf.RoundToInt(duration * p);

                    var strike = new SingleStrike();
                    strike.Cell = point;
                    strike.ExplodeOnTick = tick;
                    tempStrikes.Add(strike);

                    index++;
                }
                debug_FirstPoint = null;

                Find.CurrentMap.GetComponent<AirstrikeComp>().Spawn(new AirstrikeInstance(tempStrikes));
                tempStrikes.Clear();
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
