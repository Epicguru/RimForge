using System;
using System.Collections.Generic;
using RimForge.Damage;
using Verse;

namespace RimForge.Buildings
{
    public static class HEShellKillTracker
    {
        public static Action<Explosion, int> ReportKills;

        private static Dictionary<Explosion, int> activeExplosions = new Dictionary<Explosion, int>();
        private static List<Explosion> bin = new List<Explosion>();

        public static void Init()
        {
            Patches.Patch_Explosion_StartExplosion.OnExplosionStart += OnExplosionStart;
            DamageWorker_AddInjuryForShellHE.OnExplosionKillPawn += OnExplosionKill;
        }

        private static void OnExplosionStart(Explosion e)
        {
            activeExplosions.Add(e, 0);
        }

        private static void OnExplosionKill(Explosion e, Pawn p)
        {
            if (e == null || p == null)
                return;

            if (activeExplosions.ContainsKey(e))
                activeExplosions[e]++;
        }

        public static void BeginCapture()
        {
            Patches.Patch_Explosion_StartExplosion.Active = true;
        }

        public static void Tick()
        {
            bin.Clear();
            foreach (var pair in activeExplosions)
            {
                var ex = pair.Key;
                if (ex == null || ex.Destroyed || !ex.Spawned || ex.Map == null)
                    bin.Add(ex);
            }
            foreach (var item in bin)
            {
                if (item != null)
                {
                    try
                    {
                        ReportKills?.Invoke(item, activeExplosions[item]);
                    }
                    catch(Exception e)
                    {
                        Core.Warn($"Exception in report explosion method invocation:\n{e}");
                    }
                    activeExplosions.Remove(item);
                }
            }
            bin.Clear();
        }
    }
}
