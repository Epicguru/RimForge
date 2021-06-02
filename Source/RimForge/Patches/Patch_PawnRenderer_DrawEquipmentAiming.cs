using HarmonyLib;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    static class Patch_PawnRenderer_DrawEquipmentAiming
    {
        // Comp oversized weapon from jec's tools cancels my prefix. Fuck off.
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
}
