using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Misc
{
    public static class CursedPawnUtil
    {
        public static Color[] ClothesColors = new Color[]
        {
            new Color32(59, 156, 47, 255),
            new Color32(48, 48, 48, 255)
        };

        [DebugAction("RimForge", "Make cursed raider", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DebugMakeCursed()
        {
            foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
            {
                if (thing is Pawn {Dead: false} pawn)
                    MakeCursed(pawn);
            }
        }

        public static IEnumerable<Pawn> TryMakeCursed(IEnumerable<Pawn> pawns, int count)
        {
            if (pawns == null || count <= 0)
                yield break;

            HashSet<Pawn> excluded = new HashSet<Pawn>(count);
            for (int i = 0; i < count; i++)
            {
                var best = SelectBestPawn(pawns, excluded);
                if (best == null)
                    yield break; ;

                MakeCursed(best);
                excluded.Add(best);
                yield return best;
            }
        }

        public static void MakeCursed(Pawn pawn)
        {
            if (pawn == null)
                return;

            GiveTrait(pawn);
            TintClothes(pawn);
            ReplaceWeapon(pawn, Rand.Chance(0.3333f) ? RFDefOf.RF_SwordOfDarkness : RFDefOf.RF_CursedKhopesh);
            GiveTempMoodBoost(pawn);
        }

        public static Pawn SelectBestPawn(IEnumerable<Pawn> pawns, HashSet<Pawn> except)
        {
            if (pawns == null)
                return null;

            Pawn best = null;
            Validity valid = Validity.Invalid;

            foreach (var pawn in pawns)
            {
                if (pawn == null)
                    continue;

                if (except != null && except.Contains(pawn))
                    continue;

                var v = GetPawnValidity(pawn);
                if (v > valid)
                {
                    valid = v;
                    best = pawn;
                }
            }

            return best;
        }

        public static Validity GetPawnValidity(Pawn pawn)
        {
            if (pawn.DestroyedOrNull() || pawn.Dead || pawn.InMentalState || pawn.Downed)
                return Validity.Invalid;

            // Min intelligence.
            if ((pawn.RaceProps?.intelligence ?? Intelligence.Animal) < Intelligence.Humanlike)
                return Validity.Invalid;

            // Not mechanoids.
            if (pawn.RaceProps?.IsMechanoid ?? false)
                return Validity.Invalid;

            var primary = pawn?.equipment?.Primary;

            // Pawns who are not carrying any weapon are invalid because they may be incapable of violence or perhaps have no arms.
            if (primary == null)
                return Validity.Invalid;

            // Make sure they do not have the cursed trait.
            if (pawn.story?.traits?.HasTrait(RFDefOf.RF_ZirsCorruption) ?? false)
                return Validity.Invalid;

            // Make sure they do not have the blessed trait.
            if (pawn.story?.traits?.HasTrait(RFDefOf.RF_BlessingOfZir) ?? false)
                return Validity.Invalid;

            // Pawns who are already carrying melee weapons are ideal candidates.
            if (primary.def.thingCategories.Contains(RFDefOf.WeaponsMelee))
                return Validity.Ideal;

            // They are carrying something in their hands...
            // Let's hope it's a weapon and they can actually fight.
            return Validity.Valid;
        }

        public static void GiveTempMoodBoost(Pawn pawn)
        {
            pawn?.TryGiveThought(RFDefOf.RF_ZirKiller);
        }

        public static void GiveTrait(Pawn pawn)
        {
            pawn?.story?.traits?.GainTrait(new Trait(RFDefOf.RF_ZirsCorruption));
        }

        public static void ReplaceWeapon(Pawn pawn, ThingDef newWeapon, ThingDef stuff = null, QualityCategory? quality = null)
        {
            if (pawn?.equipment == null || newWeapon == null)
            {
                Core.Error("Null pawn or weapon def in ReplaceWeapon");
                return;
            }

            var holding = pawn.equipment.GetDirectlyHeldThings();
            if (pawn.equipment.Primary != null)
            {
                holding.Remove(pawn.equipment.Primary);
            }

            var created = ThingMaker.MakeThing(newWeapon, stuff);
            if (created == null)
            {
                Core.Error("Created weapon for replace, returned null!");
                return;
            }

            if (quality != null)
                created.TryGetComp<CompQuality>()?.SetQuality(quality.Value, ArtGenerationContext.Outsider);

            holding.TryAdd(created);
        }

        public static void TintClothes(Pawn pawn)
        {
            if (pawn?.apparel == null)
                return;

            bool changedAny = false;
            int colorIndex = 0;
            foreach (var item in pawn.apparel.WornApparel)
            {
                var comp = item?.GetComp<CompColorable>();
                if (comp == null)
                    continue;

                Color color = ClothesColors[colorIndex % ClothesColors.Length];
                colorIndex++;

                comp.Color = color;
                changedAny = true;
            }

            if (!changedAny)
                return;

            // Update apparel graphics.
            pawn.Drawer.renderer.graphics.SetApparelGraphicsDirty();
            PortraitsCache.SetDirty(pawn);
        }

        public enum Validity
        {
            Invalid,
            Valid,
            Ideal
        }
    }
}
