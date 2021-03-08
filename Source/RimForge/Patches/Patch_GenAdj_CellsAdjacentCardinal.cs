using HarmonyLib;
using System;
using System.Collections.Generic;
using RimForge.Buildings;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(GenAdj), "CellsAdjacentCardinal", new Type[] { typeof(Thing) })]
    static class Patch_GenAdj_CellsAdjacentCardinal
    {
        static void Postfix(Thing t, ref IEnumerable<IntVec3> __result)
        {
            if (t is Building_PowerPole pp)
            {
                __result = AddConnectedPoles(__result, pp);
            }
        }

        static IEnumerable<IntVec3> AddConnectedPoles(IEnumerable<IntVec3> existing, Building_PowerPole pp)
        {
            foreach (var pos in existing)
                yield return pos;

            if (pp.LinkedPoles != null)
            {
                foreach (var pole in pp.LinkedPoles)
                {
                    if (!pole.DestroyedOrNull())
                        yield return pole.Position;
                }
            }
            foreach (var pole in pp.BackLinkedPoles)
            {
                if (!pole.DestroyedOrNull())
                    yield return pole.Position;
            }
        }
    }
}
