using System;
using System.Reflection;
using HarmonyLib;
using RimForge.CombatExtended;
using UnityEngine;
using Verse;

namespace RimForge.Airstrike
{
    public class SingleStrike : IExposable
    {
        private static readonly MethodInfo impactMethod = typeof(Projectile).GetMethod("ImpactSomething", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly object[] emptyArgs = new object[0];
        private static MethodInfo impactCEMethod;
        private static MethodInfo launchCEMethod;
        private static PropertyInfo exactPosition;

        public bool IsDone { get; private set; }
        public IntVec3 Cell;
        public int ExplodeOnTick;
        public ThingDef ProjectileDef;

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref Cell, "cell");
            Scribe_Values.Look(ref ExplodeOnTick, "tick");
            Scribe_Defs.Look(ref ProjectileDef, "projectileDef");
        }

        public virtual void Tick(AirstrikeInstance instance, int tick)
        {
            if (IsDone)
                return;

            IsDone = tick >= ExplodeOnTick;
            if (IsDone)
            {
                // Check for overhead mountain.
                var roof = Cell.GetRoof(instance.Map);
                if (roof != null && roof.isThickRoof)
                    return;

                // Explode!
                //GenExplosion.DoExplosion(Cell, instance.Map, 6, DamageDefOf.Bomb, instance.Instigator, Settings.AirstrikeExplosionDamage);
                if (ProjectileDef == null)
                {
                    Core.Warn("Null projectile def, explosion not spawned...");
                    return;
                }

                if (CECompat.IsCEActive)
                {
                    // Combat extended explosions.
                    var proj = ThingMaker.MakeThing(ProjectileDef, null);
                    GenSpawn.Spawn(proj, Cell, instance.Map);
                    Core.Log(proj.def.defName);
                    Core.Log(proj.GetType().FullName);

                    exactPosition ??= AccessTools.Property(AccessTools.TypeByName("CombatExtended.ProjectileCE"), "ExactPosition");
                    impactCEMethod ??= AccessTools.Method("CombatExtended.ProjectileCE:ImpactSomething");
                    launchCEMethod ??= AccessTools.Method("CombatExtended.ProjectileCE:Launch", new Type[]{ typeof(Thing), typeof(Vector2), typeof(Thing) });

                    Core.Log(impactCEMethod.Name);
                    Core.Log(exactPosition.Name);
                    Core.Log(launchCEMethod.Name);

                    try
                    {
                        exactPosition.SetValue(proj, Cell.ToVector3ShiftedWithAltitude(AltitudeLayer.Building));
                        launchCEMethod.Invoke(proj, new object[] { null, Cell.ToVector3Shifted().WorldToFlat(), null });
                        impactCEMethod.Invoke(proj, emptyArgs);
                    }
                    catch (Exception e)
                    {
                        Core.Error("Error", e);
                    }
                }
                else
                {
                    // Regular game explosions.
                    var proj = GenSpawn.Spawn(ProjectileDef, Cell, instance.Map) as Projectile;
                    impactMethod.Invoke(proj, emptyArgs);
                }
            }
        }
    }
}
