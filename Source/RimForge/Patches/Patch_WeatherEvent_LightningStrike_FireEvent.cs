using System;
using HarmonyLib;
using RimForge.Buildings;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike), "FireEvent")]
    static class Patch_WeatherEvent_LightningStrike_FireEvent
    {
        static bool Prefix(Map ___map, ref Mesh ___boltMesh, ref IntVec3 ___strikeLoc)
        {
            if (___map == null)
                return false;

            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(___map);
            if (!___strikeLoc.IsValid)
                ___strikeLoc = CellFinderLoose.RandomCellWith((sq => sq.Standable(___map) && !___map.roofGrid.Roofed(sq)), ___map);
            ___boltMesh = LightningBoltMeshPool.RandomBoltMesh;

            bool caught = false;
            if (Building_LightningRod.MapRods.TryGetValue(___map.uniqueID, out var list))
            {
                Building_LightningRod found = null;
                float maxDst = Building_LightningRod.Radius * Building_LightningRod.Radius;
                foreach (var rod in list)
                {
                    if (rod == null || rod.Destroyed || !rod.Spawned)
                        continue;

                    float sqrDst = (___strikeLoc - rod.Position).LengthHorizontalSquared;
                    if (sqrDst > maxDst)
                        continue;

                    found = rod;
                    break;
                }

                if (found != null)
                {
                    ___strikeLoc = found.Position;
                    try
                    {
                        found.UponStruck();
                    }
                    catch (Exception e)
                    {
                        Core.Error("LightningRod.UponStruck threw an exception!", e);
                    }
                    caught = true;
                }
            }

            if (!___strikeLoc.Fogged(___map))
            {
                if(!caught)
                    GenExplosion.DoExplosion(___strikeLoc, ___map, 1.9f, DamageDefOf.Flame, (Thing)null);
                Vector3 vector3Shifted = ___strikeLoc.ToVector3Shifted();
                for (int index = 0; index < 4; ++index)
                {
                    MoteMaker.ThrowSmoke(vector3Shifted, ___map, 1.5f);
                    MoteMaker.ThrowMicroSparks(vector3Shifted, ___map);
                    MoteMaker.ThrowLightningGlow(vector3Shifted, ___map, 1.5f);
                }
            }
            SoundInfo info = SoundInfo.InMap(new TargetInfo(___strikeLoc, ___map));
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            return false;
        }
    }
}
