using RimForge.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Thoughts
{
    public class ThoughtWorker_SpecialWeapon : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            GetTraits(p, out bool blessed, out bool cursed);

            if (cursed)
            {
                // Spawn particles.
                var sparks = new RitualSparks();
                Vector2 target = p.Position.ToVector3Shifted().WorldToFlat();
                sparks.Position = target + Rand.InsideUnitCircle * 1.5f;
                sparks.GravitateTowards = target;
                sparks.Velocity = Rand.InsideUnitCircle.normalized * 1.5f;
                sparks.Color = Color.black;
                sparks.Spawn(p.Map);
            }

            var holdingDef = p.equipment?.Primary?.def;
            if (holdingDef == null)
                return false;

            // Blessed weapons
            if (holdingDef == RFDefOf.RF_SwordOfRapture)
            {
                if(blessed) // Bonus for blessed pawns holding blessed sword
                    return ThoughtState.ActiveAtStage(1, "RF.Thoughts.CarryingBlessed".Translate(holdingDef.LabelCap));

                if(cursed)
                    return false; // Cursed pawns are unaffected by the blessed weapon.

                // Regular pawn bonus.
                return ThoughtState.ActiveAtStage(0, "RF.Thoughts.CarryingBlessed".Translate(holdingDef.LabelCap));
            }

            // Cursed weapons.
            if (holdingDef == RFDefOf.RF_SwordOfDarkness)
            {
                if (cursed)
                    return false; // Cursed pawns are unaffected by the cursed weapon.

                return ThoughtState.ActiveAtStage(2, "RF.Thoughts.CarryingCursed".Translate(holdingDef.LabelCap));
            }

            return false;
        }

        private void GetTraits(Pawn pawn, out bool hasBlessing, out bool hasCurse)
        {
            hasBlessing = false;
            hasCurse = false;
            var traits = pawn?.story?.traits;
            if (traits == null)
                return;

            hasBlessing = traits.HasTrait(RFDefOf.RF_BlessingOfZir);
            hasCurse = traits.HasTrait(RFDefOf.RF_ZirsCorruption);
        }
    }
}
