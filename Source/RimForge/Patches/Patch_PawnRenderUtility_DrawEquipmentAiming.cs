using HarmonyLib;
using Verse;

namespace RimForge.Patches;

[HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
static class Patch_PawnRenderUtility_DrawEquipmentAiming
{
    [HarmonyPriority(Priority.First)]
    static void Prefix(Thing eq, ref float aimAngle)
    {
        if (!(eq is ThingWithComps comps))
            return;

        var deflector = comps.GetDeflectorComp();
        if (deflector == null)
            return;

        aimAngle += deflector.DrawAngleOffset;
    }
}