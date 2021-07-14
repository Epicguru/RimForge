using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(QualityUtility), "GenerateQualityCreatedByPawn", typeof(Pawn), typeof(SkillDef))]
    static class Patch_QualityUtility_GenerateQualityCreatedByPawn
    {
        static void Postfix(Pawn pawn, SkillDef relevantSkill, ref QualityCategory __result)
        {
            if (__result == QualityCategory.Legendary)
                return; // Can't improve.

            bool mod = relevantSkill == SkillDefOf.Construction || relevantSkill == SkillDefOf.Crafting;
            if (!mod)
                return; // Doesn't affect art or other work types...

            if (pawn?.story?.traits == null)
                return;
            if (!pawn.story.traits.HasTrait(RFDefOf.RF_BlessingOfZir))
                return;

            // Bump up quality one level.
            // For reference, an inspiration increases it 2 levels.

            bool didIncrease = false;

            var old = __result;
            if (__result < QualityCategory.Normal)
            {
                __result = QualityCategory.Normal;
                didIncrease = true;
            }
            else
            {
                bool canIncrease = true;
                if (__result >= QualityCategory.Excellent && !Settings.BlessingCreateMasterwork)
                    canIncrease = false;
                if (__result >= QualityCategory.Masterwork && !Settings.BlessingCreateLegendary)
                    canIncrease = false;

                if (Rand.Chance(Settings.BlessingIncreaseChance) && canIncrease)
                {
                    __result += 1;
                    didIncrease = true;
                }
            }

            if (pawn.IsColonist && didIncrease)
            { 
                Vector3 pos = pawn.DrawPos + new Vector3(0, 0, 1);
                pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                MoteMaker.ThrowText(pos, pawn.Map, "RF.Blessing.IncreasedMessage".Translate(__result.GetLabel()), Color.green);
            }

            //Core.Log($"Running Blessing of Zir on {pawn.LabelCap}: increased quality to {__result} from {old}");
        }
    }
}
