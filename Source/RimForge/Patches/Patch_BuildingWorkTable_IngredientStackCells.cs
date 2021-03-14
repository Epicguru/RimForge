using System.Collections.Generic;
using HarmonyLib;
using RimForge.Buildings;
using RimWorld;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Building_WorkTable), "get_IngredientStackCells")]
    static class Patch_BuildingWorkTable_IngredientStackCells
    {
        static bool Prefix(Building_WorkTable __instance, ref IEnumerable<IntVec3> __result)
        {
            if (__instance is Building_ForgeRewritten forge)
            {
                __result = MakeNewCells(forge);
                return false;
            }
            return true;
        }

        private static IEnumerable<IntVec3> MakeNewCells(Building_ForgeRewritten forge)
        {
            var pos = forge.Position;
            yield return pos + new IntVec3(-1, 0, 1);
            yield return pos + new IntVec3(0, 0, 1);
            yield return pos + new IntVec3(1, 0, 1);
        }
    }
}
