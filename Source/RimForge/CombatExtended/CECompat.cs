using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RimForge.CombatExtended
{
    public static class CECompat
    {
        public static bool IsCEActive { get; }
        public static IEnumerable<ThingDef> LoadableShells => shells.Keys;

        private static readonly Dictionary<ThingDef, ThingDef> shells = new Dictionary<ThingDef, ThingDef>();
        private static readonly HashSet<string> includeShells = new HashSet<string>() // There are vanilla shells that don't have 81mm in the def name, but must be included.
        {
            "Shell_HighExplosive",
            "Shell_Incendiary",
            "Shell_EMP",
            "Shell_Firefoam",
            "Shell_Smoke",
            "Shell_AntigrainWarhead"
        };

        static CECompat()
        {
            IsCEActive = ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null;
        }

        public static void FindCEMortarShells()
        {
            var ammoSetDefType = AccessTools.TypeByName("CombatExtended.AmmoSetDef");
            FieldInfo ammoTypes = ammoSetDefType.GetField("ammoTypes", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo ammo = AccessTools.TypeByName("CombatExtended.AmmoLink").GetField("ammo", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo projectile = AccessTools.TypeByName("CombatExtended.AmmoLink").GetField("projectile", BindingFlags.Public | BindingFlags.Instance);

            var allAmmoDefs = GenGeneric.GetStaticPropertyOnGenericType(typeof(DefDatabase<>), ammoSetDefType, "AllDefsListForReading") as IList;
            Core.Log($"Starting scan of {allAmmoDefs.Count} CE AmmoSetDef...");

            var ammoDef = GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), ammoSetDefType, "GetNamed", "AmmoSet_81mmMortarShell", true) as Def;

            var types = ammoTypes.GetValue(ammoDef) as IList;

            Core.Log($"Scanning: {ammoDef.defName} ({ammoDef.label}) ({types?.Count ?? -1})");
            foreach(var item in types)
            {
                if (item == null)
                    continue;

                var ammoThingDef = ammo.GetValue(item) as ThingDef;
                var proj = projectile.GetValue(item) as ThingDef;
                shells[ammoThingDef] = proj;
                Core.Log($"{ammoThingDef.defName} -> {proj.defName}");
            }            
        }

        public static ThingDef GetProjectile(ThingDef shell)
        {
            if (shells.TryGetValue(shell, out var found))
                return found;
            return null;
        }

        public static float GetExplosionRadius(ThingDef def)
        {
            return GetProjectile(def)?.projectile.explosionRadius ?? -1;
        }

        public static void Explode()
        {

        }
    }
}
