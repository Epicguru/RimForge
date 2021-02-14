using HarmonyLib;
using RimForge.Buildings;
using RimWorld;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(CompRefuelable), "get_ShouldAutoRefuelNow")]
    internal static class Patch_Refuelable_ShouldAutoRefuelNow
    {
        static bool Prefix(CompRefuelable __instance, ref bool __result)
        {
            if (__instance.parent is Building_Forge forge)
            {
                __result = forge.ShouldAutoRefuelNow();
                return false;
            }
            return true;
        }
    }
}
