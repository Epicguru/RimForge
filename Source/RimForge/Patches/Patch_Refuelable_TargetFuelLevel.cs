using HarmonyLib;
using RimForge.Buildings;
using RimWorld;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(CompRefuelable), "get_TargetFuelLevel")]
    internal static class Patch_Refuelable_TargetFuelLevel
    {
        static bool Prefix(CompRefuelable __instance, ref float __result)
        {
            if (__instance.parent is Building_Forge forge)
            {
                __result = forge.TargetFuelLevel();
                return false;
            }
            return true;
        }
    }
}
