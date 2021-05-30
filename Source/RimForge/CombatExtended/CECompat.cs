using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RimForge.CombatExtended
{
    public static class CECompat
    {
        public static bool IsCEActive { get; }

        private static readonly Dictionary<ThingDef, ThingDef> shells = new Dictionary<ThingDef, ThingDef>();

        static CECompat()
        {
            IsCEActive = ModLister.GetActiveModWithIdentifier("CETeam.CombatExtended") != null;
        }

        public static IEnumerable<ThingDef> GetCEMortarShells()
        {
            FieldInfo isMortarAmmo = null;
            PropertyInfo ammoSetDefs = null;
            FieldInfo ammoTypes = null;
            FieldInfo ammo = null;
            FieldInfo projectile = null;

            bool cache = shells.Count == 0;

            ThingDef GetProjectile(ThingDef ammoDef)
            {
                ammoSetDefs ??= ammoDef.GetType().GetProperty("AmmoSetDefs", BindingFlags.Public | BindingFlags.Instance);
                ammoTypes ??= GenTypes.GetTypeInAnyAssembly("CombatExtended.AmmoSetDef").GetField("ammoTypes", BindingFlags.Public | BindingFlags.Instance);
                ammo ??= GenTypes.GetTypeInAnyAssembly("CombatExtended.AmmoLink").GetField("ammo", BindingFlags.Public | BindingFlags.Instance);
                projectile ??= GenTypes.GetTypeInAnyAssembly("CombatExtended.AmmoLink").GetField("projectile", BindingFlags.Public | BindingFlags.Instance);

                IEnumerable list = ammoSetDefs.GetValue(ammoDef) as IEnumerable;
                foreach (object set in list)
                {
                    IEnumerable list2 = ammoTypes.GetValue(set) as IEnumerable;
                    foreach (var link in list2)
                    {
                        var foundAmmo = ammo.GetValue(link);
                        if (foundAmmo == ammoDef)
                        {
                            return projectile.GetValue(link) as ThingDef;
                        }
                    }
                }
                return null;
            }

            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.GetType().FullName != "CombatExtended.AmmoDef")
                    continue;

                isMortarAmmo ??= def.GetType().GetField("isMortarAmmo", BindingFlags.Public | BindingFlags.Instance);

                if ((bool)isMortarAmmo.GetValue(def) == false)
                    continue;

                if (!def.label.Contains("81mm"))
                    continue;

                if (cache)
                {
                    ThingDef proj;
                    try
                    {
                        proj = GetProjectile(def);
                    }
                    catch (Exception e)
                    {
                        Core.Error($"Exception extracting projectile for CE shell '{def.LabelCap}' ({def.defName}):", e);
                        continue;
                    }
                    Log.Message($"{def.defName} -> {proj?.defName ?? "<null>"}");
                    shells.Add(def, proj);
                }

                yield return def;
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
