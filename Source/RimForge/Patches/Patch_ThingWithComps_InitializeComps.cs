using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(ThingWithComps), "InitializeComps")]
    public static class Patch_ThingWithComps_InitializeComps
    {
        static void Postfix(ThingWithComps __instance, List<ThingComp> ___comps)
        {
            if (__instance.Stuff == null)
                return;

            var ex = __instance.Stuff.GetStuffCompGiverExtension();
            ex?.TryGiveComp(__instance, ___comps, ex.allowDuplicate, ex.exactDuplicate);
        }

        // This is faster than the default implementation of Def.GetModExtension, when looking for a specific extension.
        public static StuffCompGiver GetStuffCompGiverExtension(this Def def)
        {
            var modExtensions = def.modExtensions;
            if (modExtensions == null)
                return null;
            for (int i = 0, count = modExtensions.Count; i < count; i++)
            {
                if (modExtensions[i] is StuffCompGiver modExtension)
                    return modExtension;
            }
            return null;
        }
    }
}
