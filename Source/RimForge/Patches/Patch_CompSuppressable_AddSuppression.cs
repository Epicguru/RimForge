using Verse;

namespace RimForge.Patches
{
    internal static class Patch_CompSuppressable_AddSuppression
    {
        public static bool Prefix(ThingComp __instance, ref float amount)
        {
            var pawn = __instance.parent as Pawn;
            if (pawn == null)
                return true;

            var comp = pawn.equipment?.Primary?.GetDeflectorComp();
            if (comp == null)
                return true;

            float deflectionChance = comp.Props.deflectChance;
            if (deflectionChance <= 0f)
                return true;

            if (deflectionChance > 1f)
                deflectionChance = 1f;

            float toApply = amount * (1f - deflectionChance);
            if (toApply == 0f)
                return false;

            amount = toApply;
            return true;
        }
    }
}
