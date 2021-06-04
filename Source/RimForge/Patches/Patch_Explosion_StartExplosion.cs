using System;
using HarmonyLib;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Explosion), "StartExplosion")]
    public static class Patch_Explosion_StartExplosion
    {
        public static bool Active = false;
        public static Action<Explosion> OnExplosionStart;

        static void Prefix(Explosion __instance)
        {
            if (!Active)
                return;

            Active = false;
            OnExplosionStart.Invoke(__instance);
        }
    }
}
