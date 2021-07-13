using HarmonyLib;
using System.Reflection;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(LogEntry), "ToGameStringFromPOV_Worker")]
    static class Patch_LogEntry_ToGameStringFromPOV_Worker
    {
        private static FieldInfo weaponDef;
        private static FieldInfo projectileDef;
        private static FieldInfo recipientThing;
        private static FieldInfo initiatorThing;

        static bool Prefix(LogEntry __instance, ref string __result)
        {
            if (!(__instance is BattleLogEntry_RangedImpact ranged))
                return true;

            weaponDef ??= AccessTools.Field(typeof(BattleLogEntry_RangedImpact), "weaponDef");
            projectileDef ??= AccessTools.Field(typeof(BattleLogEntry_RangedImpact), "projectileDef");
            recipientThing ??= AccessTools.Field(typeof(BattleLogEntry_RangedImpact), "recipientPawn");
            initiatorThing ??= AccessTools.Field(typeof(BattleLogEntry_RangedImpact), "initiatorPawn");

            ThingDef wd = weaponDef.GetValue(ranged) as ThingDef;
            
            if (wd == null)
            {
                ThingDef pd = projectileDef.GetValue(ranged) as ThingDef;
                Pawn rec = recipientThing.GetValue(ranged) as Pawn;
                Pawn init = initiatorThing.GetValue(ranged) as Pawn;

                __result = $"{init?.LabelShortCap} deflected {rec?.LabelShortCap}'s {pd?.LabelCap} right back at them!";

                return false;
            }
            return true;
        }
    }
}
