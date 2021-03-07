using HarmonyLib;
using RimForge.Comps;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(CompGlower), "get_ShouldBeLitNow")]
    static class Patch_CompGlower_ShouldBeLitNow
    {
        static bool Prefix(CompGlower __instance, ref bool __result)
        {
            var ins = __instance;
            if (ins.parent is IConditionalGlower g)
            {
                __result = g.ShouldGlowNow();
                return false;
            }

            return true;
        }
    }
}
