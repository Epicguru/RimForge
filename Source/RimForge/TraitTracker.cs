﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimForge.Effects;
using RimForge.Misc;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimForge
{
    public class TraitTracker : WorldComponent
    {
        public static TraitTracker Current => Find.World?.GetComponent<TraitTracker>();

        [TweakValue("_RimForge", 0, 10)]
        private static float GearRotSpeed = 2;

        [DebugAction("RimForge", "List pawns with traits", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
        private static void DebugLog()
        {
            var current = Current;
            if (current == null)
                return;

            Core.Log($"{current.cursedPawns.Count} pawns with curse:");
            foreach (var pawn in current.cursedPawns)
            {
                Core.Log(pawn == null ? "<null>" : $"{pawn.LabelShortCap} on {pawn.Map}");
            }
        }

        private HashSet<Pawn> cursedPawns = new HashSet<Pawn>();
        private List<ExplosionEffect> explosionEffects = new List<ExplosionEffect>();
        private float gearRot;

        public TraitTracker(World world) : base(world)
        {
        }

        public void TryAdd(Pawn pawn)
        {
            if (pawn?.story?.traits?.HasTrait(RFDefOf.RF_ZirsCorruption) ?? false)
                Add(pawn);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasTrait(Pawn pawn, TraitDef def)
            => pawn?.story?.traits?.HasTrait(def) ?? false;

        private void Sanitize()
        {
            cursedPawns?.RemoveWhere(p => p.DestroyedOrNull() || p.Dead || !HasTrait(p, RFDefOf.RF_ZirsCorruption));
        }

        private void Add(Pawn pawn)
        {
            cursedPawns.Add(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref cursedPawns, "rf_cursedPawns", LookMode.Reference);
            cursedPawns ??= new HashSet<Pawn>();
            Sanitize();

            Scribe_Collections.Look(ref explosionEffects, "rf_explosionEffects", LookMode.Deep);
            explosionEffects ??= new List<ExplosionEffect>();
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            const float TICKS_TO_EXPLODE = 480;
            for (int i = 0; i < explosionEffects.Count; i++)
            {
                var item = explosionEffects[i];
                float p = item.TicksAlive / TICKS_TO_EXPLODE;

                if (p >= 1f)
                {
                    if (item.MapId >= 0 && item.MapId < Find.Maps.Count)
                    {
                        Map map2 = Find.Maps[item.MapId];
                        WaveOfHate.WorkAt(map2, new IntVec3((int)item.Center.x, 0, (int)item.Center.y), Settings.HateWaveRadius, Settings.HateWaveStunDuration);
                    }
                   
                    explosionEffects.RemoveAt(i);
                    i--;
                    continue;
                }
                item.TicksAlive++;

                if (item.MapId < 0 || item.MapId >= Find.Maps.Count)
                    continue;
                Map map = Find.Maps[item.MapId];

                int toSpawn = (int)Mathf.Lerp(1, 4, p);
                float vel = Mathf.Lerp(1.5f, 11f, p);

                for (int j = 0; j < toSpawn; j++)
                {
                    // Spawn particles.
                    var sparks = new RitualSparks();
                    Vector2 target = item.Center;
                    sparks.Position = target + Rand.InsideUnitCircle.normalized * 0.6f;
                    sparks.GravitateTowards = target;
                    sparks.Velocity = Rand.InsideUnitCircle.normalized * vel;
                    sparks.Color = Rand.Chance(0.5f) ? new Color(0.25f, 0.18f, 0.18f, 0.65f) : new Color(0.18f, 0.25f, 0.18f, 0.6f);
                    sparks.Spawn(map);
                }
            }

            gearRot += GearRotSpeed;

            if (!Rand.Chance(0.15f))
                return;

            // Cursed pawn effects.
            foreach (var pawn in cursedPawns)
            {
                var map = pawn?.Map;
                if (map == null)
                    continue;
                if (pawn.DestroyedOrNull() || pawn.Dead)
                    continue;

                // Spawn particles.
                var sparks = new RitualSparks();
                Vector2 target = pawn.DrawPos.WorldToFlat();
                sparks.Position = target + Rand.InsideUnitCircle * 2f;
                sparks.GravitateTowards = target;
                sparks.Velocity = Rand.InsideUnitCircle.normalized * 1.5f;
                sparks.Color = Rand.Chance(0.5f) ? new Color(0.25f, 0.18f, 0.18f, 0.65f) : new Color(0.18f, 0.25f, 0.18f, 0.6f);
                sparks.Spawn(map);
            }
        }

        private Material mat;
        public void OnDrawLate(Map map)
        {
            // Disabled for now, don't like how it looked.

            //foreach (var pawn in cursedPawns)
            //{
            //    if (pawn?.Map != map)
            //        continue;
            //    if (pawn.DestroyedOrNull() || pawn.Dead)
            //        continue;

            //    Vector3 drawPos = pawn.DrawPos;
            //    drawPos.y = AltitudeLayer.ItemImportant.AltitudeFor();
            //    if (mat == null)
            //    {
            //        mat = new Material(ShaderTypeDefOf.Cutout.Shader);
            //        mat.SetTexture("_MainTex", Content.RitualGearTexture);
            //        mat.color = Color.black;
            //    }
                
            //    Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos, Quaternion.Euler(0, gearRot + pawn.thingIDNumber * 40, 0), Vector3.one), mat, 0);
            //}
        }

        public void StartExplosionEffect(ExplosionEffect e)
        {
            if(e != null && !explosionEffects.Contains(e))
                explosionEffects.Add(e);
        }

        public class ExplosionEffect : IExposable
        {
            public Vector2 Center;
            public int TicksAlive;
            public int MapId;

            public void ExposeData()
            {
                Scribe_Values.Look(ref Center, "rf_center");
                Scribe_Values.Look(ref TicksAlive, "rf_ticksAlive");
                Scribe_Values.Look(ref MapId, "rf_mapId");
            }
        }
    }
}