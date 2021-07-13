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

            // Vanilla code.
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(___map);
            if (!___strikeLoc.IsValid)
                ___strikeLoc = CellFinderLoose.RandomCellWith((sq => sq.Standable(___map) && !___map.roofGrid.Roofed(sq)), ___map);
            ___boltMesh = LightningBoltMeshPool.RandomBoltMesh;

            // New strike pos calculation!
            bool caught = false;
            var list = ___map.GetComponent<RodTracker>()?.RodsReadOnly;
            if (list != null)
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
                    // New strike pos is lightning rod position + 1 upwards, due to how tall the rod is.
                    ___strikeLoc = found.Position + new IntVec3(0, 0, 1);
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
            // End new strike pos calculation

            // Vanilla code.
            if (!___strikeLoc.Fogged(___map))
            {
                if(!caught) // Do not cause explosion damage if caught by lightning rod.
                    GenExplosion.DoExplosion(___strikeLoc, ___map, 1.9f, DamageDefOf.Flame, (Thing)null);

                Vector3 vector3Shifted = ___strikeLoc.ToVector3Shifted();
                for (int index = 0; index < 4; ++index)
                {
#if V13
                    FleckMaker.ThrowSmoke(vector3Shifted, ___map, 1.5f);
                    FleckMaker.ThrowMicroSparks(vector3Shifted, ___map);
                    FleckMaker.ThrowLightningGlow(vector3Shifted, ___map, 1.5f);
#else
                    MoteMaker.ThrowSmoke(vector3Shifted, ___map, 1.5f);
                    MoteMaker.ThrowMicroSparks(vector3Shifted, ___map);
                    MoteMaker.ThrowLightningGlow(vector3Shifted, ___map, 1.5f);
#endif
                }
            }
            SoundInfo info = SoundInfo.InMap(new TargetInfo(___strikeLoc, ___map));
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            return false;
        }
    }
}
