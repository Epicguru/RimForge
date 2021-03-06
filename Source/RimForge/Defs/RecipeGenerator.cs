﻿using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Verse;

namespace RimForge
{
    public static class RecipeGenerator
    {
        public static List<RecipeDef> GenAlloySmeltDefs(IReadOnlyList<AlloyDef> alloys)
        {
            if (alloys == null)
                return null;

            var field = typeof(RecipeDef).GetField("ingredientValueGetterInt", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Core.Error("Failed to find RecipeDef.ingredientValueGetterInt field. Has RimWorld updated and broken everything?");
                return null;
            }

            StringBuilder str = new StringBuilder(128);
            string MakeDesc(AlloyDef alloy, int multi)
            {
                str.Clear();
                foreach (var item in alloy.input)
                {
                    var equivalents = AlloyHelper.GetEquivalentResources(item.resource);
                    if(equivalents.Count == 1)
                    {
                        str.Append(" •");
                        str.Append(item.resource.LabelCap);
                        str.Append(" x");
                        str.Append(item.count * multi).AppendLine();
                    }
                    else
                    {
                        str.Append(" •");
                        for (int i = 0; i < equivalents.Count; i++)
                        {
                            var eq = equivalents[i];
                            string modName = eq.modContentPack == null ? "???" : eq.modContentPack.IsCoreMod ? null : eq.modContentPack.Name;
                            if(modName != null)
                                str.Append("<i>").Append(modName).Append("</i> ");
                            str.Append(eq.LabelCap);
                            if (i != equivalents.Count - 1)
                                str.Append(" or ");
                        }
                        str.Append(" x");
                        str.Append(item.count * multi).AppendLine();

                    }
                }
                str.Append("\nRequires forge temperature: <color=#ff5555ff>").Append(alloy.MinTemperature.ToStringTemperature()).Append("</color>");
                return $"{alloy.description?.TrimEnd()}\n\nMake {alloy.output.resource.label} x{alloy.output.count * multi} using:\n\n{str.ToString().TrimEnd()}".TrimStart();
            }

            RecipeDef CreateDef(AlloyDef alloy, bool bulk)
            {
                var output = alloy.output.resource;

                int multi = bulk ? alloy.bulkMultiplier : 1;

                RecipeDef def = new RecipeDef();
                def.defName = $"RF_Smelt_{output.defName}{(bulk ? "_Bulk" : "")}_AutoGenerated";
                def.label = $"Create {output.label}";
                if (bulk) def.label += " (bulk)";
                def.description = MakeDesc(alloy, multi);
                def.workAmount = alloy.baseWork * multi;
                def.jobString = $"Creating {output.label} at the forge.";
                def.modContentPack = alloy.modContentPack ?? output.modContentPack;
                def.fileName = "fuckoffitsautogeneratedwhatdoyouexpect.xml";
                def.generated = true;
                field.SetValue(def, new IngredientValueGetter_IgnoreVolume());

                // Add output.
                def.products.Add(new ThingDefCountClass(alloy.output.resource, alloy.output.count * multi));

                // Add inputs.
                ThingFilter masterFilter = new ThingFilter();
                masterFilter.SetDisallowAll();
                foreach (var item in alloy.input)
                {
                    var ingredient = new IngredientCount();
                    var allowedInputs = AlloyHelper.GetEquivalentResources(item.resource);
                    foreach (var item2 in allowedInputs)
                    {
                        ingredient.filter.SetAllow(item2, true);
                        masterFilter.SetAllow(item2, true);
                    }

                    ingredient.SetBaseCount(item.count * multi);
                    def.ingredients.Add(ingredient);
                }
                def.fixedIngredientFilter = masterFilter;
                return def;
            }

            var list = new List<RecipeDef>(alloys.Count);
            foreach (var alloy in alloys)
            {
                if (alloy == null || !alloy.IsValid)
                    continue;

                var simple = CreateDef(alloy, false);
                list.Add(simple);
                AlloyHelper.RegisterAlloyRecipe(simple, alloy);

                if (alloy.allowBulk)
                {
                    var bulk = CreateDef(alloy, true);
                    list.Add(bulk);
                    AlloyHelper.RegisterAlloyRecipe(bulk, alloy);
                }
            }

            DefGenHelper.TryPostGenerate(list);


            return list;
        }
    }
}
