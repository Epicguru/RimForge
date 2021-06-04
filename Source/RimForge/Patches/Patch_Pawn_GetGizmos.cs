using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    static class Patch_Pawn_GetGizmos
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Pawn __instance)
        {
            foreach (var value in values)
                    yield return value;

            if (!__instance.IsColonistPlayerControlled)
                yield break;

            var deflector = __instance.equipment?.Primary?.GetDeflectorComp();
            if (deflector == null)
                yield break;

            yield return deflector.GetToggleCommand();
        }
    }
}
