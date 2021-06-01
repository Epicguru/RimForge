using Verse;

namespace RimForge.Comps
{
    // Heavily inspired by Jec's Tools deflection system.
    // So why not use the Jec's version?
    // Because the code is a mess and it doesn't work as advertised (always reflects bullets, instead of deflecting).
    // It has half-implemented systems (accuracy roll) and several other quirks that I didn't feel happy
    // adding to my weapons.
    // This is a simplified version of that system, without the mess and spaghetti.
    public class CompDeflector : ThingComp
    {
        public Pawn Wielder => parent as Pawn;

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
        }
    }
}
