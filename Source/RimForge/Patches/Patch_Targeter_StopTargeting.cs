using HarmonyLib;
using RimWorld;
using System;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Targeter), "StopTargeting")]
    static class Patch_Targeter_StopTargeting
    {
        public static Action PerformOnce;

        static void Postfix()
        {
            if (PerformOnce == null)
                return;

            PerformOnce();
            PerformOnce = null;
        }
    }
}
