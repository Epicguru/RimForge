using RimWorld;
using Verse;

namespace RimForge.Damage
{
    public class DamageWorker_Electrical : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageResult damageResult = base.Apply(dinfo, victim);
            if (!damageResult.deflected && !dinfo.InstantPermanentInjury && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(victim) * 0.25f))
            {
                victim.TryAttachFire(Rand.Range(0.15f, 0.25f), dinfo.Instigator);
            }
            return damageResult;
        }
    }
}
