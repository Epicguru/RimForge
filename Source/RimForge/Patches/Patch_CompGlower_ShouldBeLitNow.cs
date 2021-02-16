using HarmonyLib;
using RimForge.Buildings;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(CompGlower), "get_ShouldBeLitNow")]
    static class Patch_CompGlower_ShouldBeLitNow
    {
        static bool Prefix(CompGlower __instance, ref bool __result)
        {
            var ins = __instance;
            if (ins.parent is Building_WirelessPowerPylon c)
            {
                __result = c.IsActive;
                return false;
            }

            return true;
        }
    }
}
