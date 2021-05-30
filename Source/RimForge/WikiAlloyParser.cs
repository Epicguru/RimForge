using System.Collections.Generic;
using System.Text;
using InGameWiki;
using RimForge.Comps;
using Verse;

namespace RimForge
{
    internal static class WikiAlloyParser
    {
        static StringBuilder str = new StringBuilder();
        static int currentCat;
        static int lastCat;

        public static bool AlloyCheck(ThingDef thing)
        {
            return thing != null && thing.HasComp(typeof(CompShowAlloyInfo));
        }

        static IEnumerable<WikiElement> Parse(CustomElementArgs args)
        {
            WikiPage page = args.Page;
            lastCat = 0;
            currentCat = 0;

            var def = page.Def as ThingDef;

            str.Clear();
            str.AppendLine("<color=cyan><b>Important stats:</b></color>");

            ShowStat("Market value", GetStat(def, "MarketValue") * 0.01f, null, '$');
            currentCat++;

            ShowStat("Armor (sharp)", GetStat(def, "StuffPower_Armor_Sharp"), 1f, '%');
            ShowStat("Armor (blunt)", GetStat(def, "StuffPower_Armor_Blunt"), 1f, '%');
            ShowStat("Armor (heat)", GetStat(def, "StuffPower_Armor_Heat"), 1f, '%');
            currentCat++;

            ShowStat("Melee sharp damage", GetStat(def, "SharpDamageMultiplier"), 1f, '%');
            ShowStat("Melee attack cooldown", GetStuffStat(def, "MeleeWeapon_CooldownMultiplier"), 1f, '%', true);
            ShowStat("Melee blunt damage", GetStat(def, "BluntDamageMultiplier"), 1f, '%');
            currentCat++;

            ShowStat("Max hit points", GetStuffStat(def, "MaxHitPoints"), 1f, '%');
            ShowStat("Beauty", GetStuffStat(def, "Beauty"), 1f, '%');
            currentCat++;

            ShowStat("Work to make (items)", GetStuffStat(def, "WorkToMake"), 1f, '%', true);
            ShowStat("Work to build", GetStuffStat(def, "WorkToBuild"), 1f, '%', true);

            yield return WikiElement.Create(str.ToString());

            if (AlloyHelper.AllCraftableAlloys.TryGetValue(def, out var found) && found != null)
            {

            }
        }

        static float? GetStat(ThingDef def, string defName)
        {
            if (def.statBases == null)
                return null;

            foreach (var stat in def.statBases)
            {
                if (stat.stat.defName == defName)
                    return stat.value;
            }

            return 0f;
        }

        static float? GetStuffStat(ThingDef def, string defName)
        {
            if (def.stuffProps?.statFactors == null)
                return null;

            foreach (var stat in def.stuffProps.statFactors)
            {
                if (stat.stat.defName == defName)
                    return stat.value;
            }

            return 0f;
        }

        static void ShowStat(string name, float? value, float? expected, char? append, bool invertedPositive = false)
        {
            if (value == null)
                return;

            if (value == expected)
                return;

            bool isAnything = expected != null;
            bool positive = value > (expected ?? 0f);
            if (invertedPositive)
                positive = !positive;

            bool newCat = lastCat != currentCat;
            lastCat = currentCat;

            if (newCat)
                str.AppendLine();

            str.AppendLine();
            str.Append("<b>");
            str.Append(name);
            str.Append(": ");
            str.Append("</b>");
            if(isAnything)
                str.Append("<color=").Append(positive ? "#71ff1f" : "#ff6e63").Append(">");
            str.Append((value.Value * 100f).ToString("F0"));
            if(append != null)
                str.Append(append.Value);
            if(isAnything)
                str.Append("</color>");
        }
    }
}
