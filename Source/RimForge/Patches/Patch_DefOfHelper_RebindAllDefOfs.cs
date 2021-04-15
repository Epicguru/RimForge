using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(DefOfHelper), "RebindAllDefOfs")]
    static class Patch_DefOfHelper_RebindAllDefOfs
    {
        static void Postfix()
        {
            try
            {
                var cat = MakeCategoryDef();
                DefDatabase<DesignationCategoryDef>.Add(cat);
                PostLoadDefs.CategorizeBuildings(cat);
            }
            catch (Exception e)
            {
                Core.Error("Exception in pre-resolve-references", e);
            }
        }

        static DesignationCategoryDef MakeCategoryDef()
        {
            return new DesignationCategoryDef()
            {
                defName = "RF_DesignationCategory_Generated",
                label = "RimForge",
                modContentPack = Core.ContentPack,
                showPowerGrid = false
            };
        }
    }
}
