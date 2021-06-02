using HarmonyLib;
using RimWorld;
using System.Xml;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(SkillRequirement), "get_Summary")]
    static class Patch_SkillRequirement_Summary
    {
        static bool Prefix(SkillDef ___skill, ref string __result)
        {
            if (___skill != Core.BlessingSkillDef)
                return true;

            __result = "Blessing of Zir (trait)";
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillRequirement), "PawnSatisfies")]
    static class Patch_SkillRequirement_PawnSatisfies
    {
        static bool Prefix(SkillDef ___skill, Pawn pawn, ref bool __result)
        {
            if (___skill != Core.BlessingSkillDef)
                return true;

            __result = pawn?.story?.traits?.HasTrait(RFDefOf.RF_BlessingOfZir) ?? false;
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillRequirement), "LoadDataFromXmlCustom")]
    static class Patch_SkillRequirement_LoadDataFromXmlCustom
    {
        static bool Prefix(ref SkillDef ___skill, ref int ___minLevel, XmlNode xmlRoot)
        {
            if (xmlRoot.Name == "RF_BlessingOfZir")
            {
                ___skill = Core.BlessingSkillDef;
                ___minLevel = 1;
                return false;
            }
            return true;
        }
    }
}
