using UnityEngine;
using Verse;

namespace RimForge.Comps
{
    public class CompProperties_OversizedWeapon : CompProperties
    {
        public Vector3 northOffset = new Vector3(0, 0, 0);
        public Vector3 eastOffset = new Vector3(0, 0, 0);
        public Vector3 southOffset = new Vector3(0, 0, 0);
        public Vector3 westOffset = new Vector3(0, 0, 0);

        public CompProperties_OversizedWeapon()
        {
            compClass = typeof(CompOversizedWeapon);
        }
    }
}