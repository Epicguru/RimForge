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

                    exactPosition ??= AccessTools.Property(AccessTools.TypeByName("CombatExtended.ProjectileCE"), "ExactPosition");
                    impactCEMethod ??= AccessTools.Method("CombatExtended.ProjectileCE:ImpactSomething");
                    launchCEMethod ??= AccessTools.Method("CombatExtended.ProjectileCE:Launch", new Type[]{ typeof(Thing), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(float), typeof(Thing) });

                    try
                    {
                        exactPosition.SetValue(proj, Cell.ToVector3ShiftedWithAltitude(AltitudeLayer.Building));
                        launchCEMethod.Invoke(proj, new object[] { instance.Instigator, Cell.ToVector3Shifted().WorldToFlat(), -1f, -1f, 0f, -1f, null });
                        //impactCEMethod.Invoke(proj, emptyArgs);
                    }
                    catch
                    {
                        //Core.Error("Error in CE explosion invocation", e);
                    }
                }
                else
                {
                    // Regular game explosions.
                    var proj = GenSpawn.Spawn(ProjectileDef, Cell, instance.Map) as Projectile;
                    proj.Launch(instance.Instigator, Cell, Cell, ProjectileHitFlags.IntendedTarget);
                    impactMethod.Invoke(proj, emptyArgs);
                }
            }
        }
    }
}
