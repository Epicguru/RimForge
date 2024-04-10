using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(DefOfHelper), "RebindAllDefOfs")]
    static class Patch_DefOfHelper_RebindAllDefOfs
    {
        static bool hasRun = false;

        static void Postfix()
        {
            if (hasRun)
                return;
            hasRun = true;

            try
            {
                if (!Settings.UseCustomTab)
                    return;

                var cat = MakeCategoryDef();
                DefDatabase<DesignationCategoryDef>.Add(cat);
                CategorizeBuildings(cat);
            }
            catch (Exception e)
            {
                Core.Error("Exception in pre-resolve-references", e);
            }
        }

        static DesignationCategoryDef MakeCategoryDef()
        {
            return new DesignationCategoryDef
            {
                defName = "RF_DesignationCategory_Generated",
                label = "RimForge",
                modContentPack = Core.ContentPack,
                showPowerGrid = false
            };
        }

        static void CategorizeBuildings(DesignationCategoryDef cat)
        {
            foreach (var def in Core.ContentPack.AllDefs)
            {
                if (def is ThingDef td && typeof(Building).IsAssignableFrom(td.thingClass))
                {
                    if (td.designationCategory != null)
                        td.designationCategory = cat;
                }
            }
        }
    }
}
