using HarmonyLib;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Thing), "PreApplyDamage")]
    static class Patch_Thing_PreApplyDamage
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Thing __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            //absorbed = true;
            if (absorbed)
                return;

            if (!(__instance is Pawn pawn))
                return;

            var deflector = pawn.equipment?.Primary?.GetDeflectorComp();
            deflector?.WielderPostPreApplyDamage(dinfo, out absorbed);
        }
    }
}
