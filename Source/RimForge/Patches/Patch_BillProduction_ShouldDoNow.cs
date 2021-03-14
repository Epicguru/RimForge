using HarmonyLib;
using RimForge.Buildings;
using RimWorld;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Bill_Production), "ShouldDoNow")]
    static class Patch_BillProduction_ShouldDoNow
    {
        static void Postfix(Bill_Production __instance, ref bool __result)
        {
            if (__result && __instance.billStack?.billGiver is Building_ForgeRewritten forge)
            {
                __result = forge.CanDoBillNow(__instance);
            }
        }
    }
}
