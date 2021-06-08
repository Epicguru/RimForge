using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimForge.Comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge
{
    public static class AlloyHelper
    {
        public const float DEFAULT_MELTING_POINT = 800;

        /// <summary>
        /// A list of all RimForge compatible resources.
        /// These are resources (materials, metals) that have the ModExtension.
        /// </summary>
        public static readonly List<ThingDef> AllRimForgeResources = new List<ThingDef>();
        /// <summary>
        /// A list of all found alloy defs.
        /// </summary>
        public static readonly List<AlloyDef> AllAlloyDefs = new List<AlloyDef>();
        /// <summary>
        /// A map of all known alloy defs and their corresponding recipe def.
        /// Technically, there could be more than one recipe for a single alloy,
        /// but that isn't accounted for here.
        /// </summary>
        public static readonly Dictionary<ThingDef, AlloyDef> AllCraftableAlloys = new Dictionary<ThingDef, AlloyDef>();
        /// <summary>
        /// A dictionary where they key is an input material, and the value is a list of alloys that it is used in.
        /// </summary>
        public static readonly Dictionary<ThingDef, List<ThingDef>> UsedToMake = new Dictionary<ThingDef, List<ThingDef>>();

        private static readonly Dictionary<ThingDef, int> bagMap = new Dictionary<ThingDef, int>();
        private static readonly List<List<ThingDef>> bags = new List<List<ThingDef>>();
        private static readonly List<ThingDef> tempReturnList = new List<ThingDef>();
        private static readonly Dictionary<RecipeDef, AlloyDef> recipeToAlloy = new Dictionary<RecipeDef, AlloyDef>();


        /// <summary>
        /// Gets the melting point of this resource.
        /// Only intended to be used for things that could logically be melted
        /// down in a forge, such as metals or alloys.
        /// Unless a custom value has been defined using the <see cref="Extension"/>,
        /// this will return the default value <see cref="DEFAULT_MELTING_POINT"/>.
        /// </summary>
        /// <param name="def">The ThingDef to get the melting point for.</param>
        /// <returns>The melting point, measured in degrees celsius.</returns>
        public static float GetMeltingPoint(this ThingDef def)
        {
            var extension = def?.GetModExtension<Extension>();
            if (extension != null)
                return extension.meltingPoint ?? extension.equivalentTo?.GetMeltingPoint() ?? DEFAULT_MELTING_POINT;

            return DEFAULT_MELTING_POINT;
        }

        public static Color? GetMoltenColor(this ThingDef def)
        {
            return def?.stuffProps?.color;
        }

        /// <summary>
        /// Gets a version of this thing's LabelCap, but including the name
        /// of the mod that adds it.
        /// For example, '[RimForge] Copper'
        /// </summary>
        public static string ModLabelCap(this ThingDef def)
        {
            if (def == null)
                return null;
            if (def.modContentPack == null)
                return def.LabelCap;

            string modName = def.modContentPack?.Name ?? "Vanilla";

            return $"[{modName}] {def.LabelCap}";
        }

        /// <summary>
        /// Adds a logical equality between resources, for example 2 different types of copper.
        /// All the 'other' coppers should point back to the RimForge copper.
        /// </summary>
        /// <param name="resource">The 'other' resource, such as a different mod's copper.</param>
        /// <param name="equivalentTo">The resource that it is equivalent to, such as RimForge's copper.</param>
        public static void AddEquivalentResource(ThingDef resource, ThingDef equivalentTo)
        {
            if (resource == null || equivalentTo == null || resource == equivalentTo)
            {
                Core.Error($"Tried to add equivalent resources {resource?.ModLabelCap() ?? "<null>"} -> {equivalentTo?.ModLabelCap() ?? "<null>"}. Invalid.");
                return;
            }

            var bagA = GetBagContaining(resource, out int bagAIndex);
            var bagB = GetBagContaining(equivalentTo, out int bagBIndex);

            Core.Log($"Making equivalent: '{resource.ModLabelCap()}' => '{equivalentTo.ModLabelCap()}'");

            if (bagA == null && bagB == null)
            {
                // Create new bag, add these two.
                var set = new List<ThingDef>();
                set.Add(equivalentTo);
                set.Add(resource);
                int index = bags.Count;
                bagMap.Add(equivalentTo, index);
                bagMap.Add(resource, index);
                bags.Add(set);
            }
            else if (bagA == null)
            {
                // equivalentTo is already in a bag, so resource needs to join that bag.
                bagMap.Add(resource, bagBIndex);
                bagB.Add(resource);
            }
            else if (bagB == null)
            {
                // Target resource needs to be put into bag A.
                bagMap.Add(equivalentTo, bagAIndex);
                bagA.Insert(0, equivalentTo);
            }
            else if (bagA == bagB)
            {
                // They are already in the same bag...
                Core.Warn($"'{resource.ModLabelCap()}' is already equivalent to '{equivalentTo.ModLabelCap()}'");
            }
            else
            {
                // ERROR!
                // They are both in different bags!
                // I could merge the bags, but it's more likely that this was done by mistake, so I'll display an
                // error message.
                Core.Error($"Error adding equivalent resource: '{resource.ModLabelCap()}' is already in a resource bag, and '{equivalentTo.ModLabelCap()}' is in another.");
            }
        }

        public static void RegisterAlloyRecipe(RecipeDef recipe, AlloyDef def)
        {
            if (recipe == null || def == null)
            {
                Core.Error("Null input(s?) to RegisterAlloyRecipe");
                return;
            }

            if (recipe.ProducedThingDef != def.output.resource)
            {
                Core.Warn($"The recipe output '{recipe.ProducedThingDef.LabelCap}' does not match the alloy def output '{def.output.resource.LabelCap}'...\n" +
                           "Are you sure that you matched the right recipe with the alloy def?");
            }

            recipeToAlloy.Add(recipe, def);
        }

        public static AlloyDef TryGetAlloyDef(this RecipeDef recipe)
        {
            if (recipeToAlloy.TryGetValue(recipe, out var found))
                return found;
            return null;
        }

        public static IReadOnlyList<ThingDef> GetEquivalentResources(ThingDef def)
        {
            if (def == null)
                return null;

            if (bagMap.TryGetValue(def, out int index))
            {
                return bags[index];
            }

            tempReturnList.Clear();
            tempReturnList.Add(def);
            return tempReturnList;
        }

        private static List<ThingDef> GetBagContaining(ThingDef def, out int index)
        {
            if (def == null)
            {
                index = -1;
                return null;
            }

            for (int i = 0; i < bags.Count; i++)
            {
                var bag = bags[i];
                if (bag.Contains(def))
                {
                    index = i;
                    return bag;
                }
            }

            index = -1;
            return null;
        }

        public static Vector2 WorldToFlat(this Vector3 worldPos)
        {
            return new Vector2(worldPos.x, worldPos.z);
        }

        public static Vector3 FlatToWorld(this Vector2 flatPos, float y)
        {
            return new Vector3(flatPos.x, y, flatPos.y);
        }

        public static void DrawCustomOverlay(this Thing drawer)
        {
            if (drawer is not ICustomOverlayDrawer)
            {
                Core.Warn($"{drawer.LabelCap} cannot draw a custom overlay since it's building class does not implement the ICustomOverlayDrawer interface.");
                return;
            }

            drawer.Map.overlayDrawer.DrawOverlay(drawer, RimWorld.OverlayTypes.BrokenDown);
        }

        public static void TryGiveThought(this Pawn pawn, ThoughtDef def, Pawn otherPawn = null)
        {
            if (def == null)
                return;

            pawn?.needs?.mood?.thoughts?.memories?.TryGainMemory(def, otherPawn);
        }

        public static void TryGiveThought(this Pawn pawn, ThoughtDef def, int level, Pawn otherPawn = null)
        {
            if (def == null)
                return;

            pawn?.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtMaker.MakeThought(def, level), otherPawn);
        }

        /// <summary>
        /// This is more performant that doing GetComp<CompDeflector>()
        /// </summary>
        public static CompDeflector GetDeflectorComp(this ThingWithComps thing)
        {
            for (int i = 0; i < thing.AllComps.Count; i++)
            {
                var comp = thing.AllComps[i];
                if (comp is CompDeflector d)
                    return d;
            }
            return null;
        }

        /// <summary>
        /// Clones a object via shallow copy, using reflection to access MemberwiseClone.
        /// </summary>
        /// <typeparam name="T">Object Type to clone.</typeparam>
        /// <param name="obj">Object to clone.</param>
        /// <returns>The new Object reference.</returns>
        public static T CloneObject<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            var memberwiseClone = AccessTools.Method(typeof(T), "MemberwiseClone");
            return (T)memberwiseClone.Invoke(obj, new object[]{});
        }

        public static CompOversizedWeapon GetCompOversizedWeapon(this ThingWithComps thing)
        {
            var comps = thing.AllComps;
            for (int i = 0, count = comps.Count; i < count; i++)
            {
                if (comps[i] is CompOversizedWeapon comp)
                    return comp;
            }
            return null;
        }

        public static CompOversizedWeapon TryGetCompOversizedWeapon(this Thing thing)
        {
            return thing is ThingWithComps thingWithComps ? thingWithComps.GetCompOversizedWeapon() : null;
        }
    }
}
