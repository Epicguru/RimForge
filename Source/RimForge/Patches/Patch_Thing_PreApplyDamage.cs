using System;
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

            if (__instance is not Pawn pawn)
                return;

            try
            {
                var deflector = pawn.equipment?.Primary?.GetDeflectorComp();
                deflector?.WielderPostPreApplyDamage(dinfo, out absorbed);
            }
            catch (Exception e)
            {
                Verse.Log.Error(e.ToString());
            }
            
        }
    }
}
