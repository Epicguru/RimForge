using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimForge.Damage
{
    public class DamageWorker_AddInjuryForShellHE : DamageWorker_AddInjury
    {
        public static Action<Explosion, Pawn> OnExplosionKillPawn;

        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            var result =  base.Apply(dinfo, thing);
            if(thing is Pawn {Dead: true})
            {

            }
            return result;
        }

        protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
        {
            // Clone of the vanilla code, I don't want to fuck with transpilers right now.
            // It's a custom worker so it's fine.

            if (t.def.category == ThingCategory.Mote || t.def.category == ThingCategory.Ethereal)
            {
                return;
            }
            if (damagedThings.Contains(t))
            {
                return;
            }
            damagedThings.Add(t);
            if (ignoredThings != null && ignoredThings.Contains(t))
            {
                return;
            }
            if (t.def == ThingDefOf.Fire && !t.Destroyed)
            {
                t.Destroy(DestroyMode.Vanish);
                return;
            }
            float angle;
            if (t.Position == explosion.Position)
            {
                angle = Rand.RangeInclusive(0, 359);
            }
            else
            {
                angle = (t.Position - explosion.Position).AngleFlat;
            }
            DamageInfo dinfo = new DamageInfo(this.def, (float)explosion.GetDamageAmountAt(cell), explosion.GetArmorPenetrationAt(cell), angle, explosion.instigator, null, explosion.weapon, DamageInfo.SourceCategory.ThingOrUnknown, explosion.intendedTarget);
            if (this.def.explosionAffectOutsidePartsOnly)
            {
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            }
            BattleLogEntry_ExplosionImpact battleLogEntry_ExplosionImpact = null;
            Pawn pawn = t as Pawn;
            if (pawn != null)
            {
                battleLogEntry_ExplosionImpact = new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, this.def);
                Find.BattleLog.Add(battleLogEntry_ExplosionImpact);
            }
            DamageResult damageResult = t.TakeDamage(dinfo);
            damageResult.AssociateWithLog(battleLogEntry_ExplosionImpact);
            if (pawn != null && damageResult.wounded && pawn.stances != null)
            {
#if V14
                pawn.stances.stagger.StaggerFor(95);
#else
                pawn.stances.StaggerFor(95);
#endif
            }

            if (pawn?.Dead ?? false)
            {
                OnExplosionKillPawn?.Invoke(explosion, pawn);
            }
		}
    }
}
