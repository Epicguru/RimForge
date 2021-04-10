using HarmonyLib;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class Patch_Pawn_SpawnSetup
    {
        static void Postfix(Pawn __instance)
        {
            TraitTracker.Current?.TryAdd(__instance);
        }
    }
}
