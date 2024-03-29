﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using RimForge.Misc;
using UnityEngine;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "GenerateRaidLoot")]
    public class Patch_IncidentWorker_RaidEnemy_GenerateRaidLoot
    {
        static void Postfix(IncidentParms parms, List<Pawn> pawns)
        {
            // TODO check if player has pawns.

            if (pawns != null && pawns.Count > 0)
            {
                int blessedCount = 0;
                foreach (var _ in TraitTracker.Current.GetBlessedPawns(pawns[0].Map, Faction.OfPlayerSilentFail.def))
                {
                    blessedCount++;
                }
                if (blessedCount == 0)
                    return;

                int toChange = Mathf.RoundToInt(blessedCount * Settings.CursedRaidersNumberScale * Rand.Range(Settings.CursedRaidersNumberMultiplierMin, Settings.CursedRaidersNumberMultiplierMax));
                if (toChange <= 0)
                    return;
                List<Pawn> changed = new List<Pawn>(toChange);
                foreach (var converted in CursedPawnUtil.TryMakeCursed(pawns, toChange))
                {
                    Core.Log($"{converted.LabelShortCap} is now a cursed raider");
                    changed.Add(converted);
                }
                if (changed.Count > 0 && Settings.SendRaiderLetter)
                {
                    // Send letter.
                    SendLetter(parms, changed);
                }
            }
        }

        private static void SendLetter(IncidentParms parms, List<Pawn> pawns)
        {
            StringBuilder str = new StringBuilder();
            foreach (var t in pawns)
                str.AppendLine(t.NameShortColored);
            
            TaggedString text = "RF.Raiders.WarningContent".Translate(parms.faction.NameColored, pawns.Count, parms.faction.def.pawnsPlural, str);
            ChoiceLetter choiceLetter = LetterMaker.MakeLetter("RF.Raiders.WarningTitle".Translate(), text, RFDefOf.RF_CursedRaiders, pawns, parms.faction);
            Find.LetterStack.ReceiveLetter(choiceLetter);
        }
    }
}