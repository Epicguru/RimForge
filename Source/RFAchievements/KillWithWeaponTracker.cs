using AchievementsExpanded;
using Verse;

namespace RimForge.Achievements
{
    public class KillWithWeaponTracker : KillTracker
	{
        public ThingDef weaponDef;

        public KillWithWeaponTracker(){}

        public KillWithWeaponTracker(KillWithWeaponTracker reference)
            :base(reference)
        {
            this.weaponDef = reference.weaponDef;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref weaponDef, "weaponDef");

            base.ExposeData();
        }

        public override bool Trigger(Pawn pawn, DamageInfo? dinfo)
        {
            if (dinfo == null)
                return false;

            if (weaponDef != null && dinfo.Value.Weapon != weaponDef)
                return false;

            return base.Trigger(pawn, dinfo);
        }
    }
}