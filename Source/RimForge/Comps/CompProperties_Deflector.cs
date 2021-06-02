using Verse;

namespace RimForge.Comps
{
    public class CompProperties_Deflector : CompProperties
    {
        /// <summary>
        /// Base chance for the weapon to block a bullet.
        /// If the bullet is blocked, weather it will be reflected or simply deflected is determined by
        /// <see cref="reflectChance"/>.
        /// </summary>
        public float deflectChance = 0.25f;

        /// <summary>
        /// In the case that a bullet is blocked (see <see cref="deflectChance"/>),
        /// this is the probability that the bullet will be reflected back towards the shooter.
        /// If the deflected bullet is not reflected, it will simply be stopped (think of it as being deflected into the ground).
        /// </summary>
        public float reflectChance = 0.5f;

        /// <summary>
        /// The time, in ticks, after deflecting a projectile that another projectile can
        /// not be deflected.
        /// </summary>
        public int cooldown = 0;

        /// <summary>
        /// The damage that the weapon takes every time it deflects or reflects a bullet.
        /// The weapon will never be destroyed (it will stop damaging at 1 hp).
        /// </summary>
        public int damageToWeapon = 0;

        public float accuracyMultiTouch = 1f;
        public float accuracyMultiShort = 1f;
        public float accuracyMultiMedium = 1f;
        public float accuracyMultiLong = 1f;

        public CompProperties_Deflector()
        {
            compClass = typeof(CompDeflector);
        }
    }
}
