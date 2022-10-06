using AchievementsExpanded;
using RimForge;
using RimForge.Achievements;
using System;
using Verse;

namespace Rimforge.Achievements
{
    public class ACore : Mod
    {
        public ACore(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                Core.CoilgunHitPawn += OnCoilgunHitPawn;
                Core.CoilgunPostFire += OnCoilgunPostFire;
                Core.CoilgunExplosion += OnCoilgunExplosion;
                Core.GenericAchievementEvent += GenericEventTracker.Fire;
                Core.Log("Hooked achievements!");
            });
        }

        private static void OnCoilgunHitPawn(Pawn pawn, CoilgunShellDef shellDef, int penDepth)
        {
            foreach (var card in AchievementPointManager.GetCards<CoilgunKillTracker>())
            {
                try
                {
                    if ((card.tracker as CoilgunKillTracker).Trigger(penDepth, pawn, shellDef))
                    {
                        card.UnlockCard();
                    }
                }
                catch (Exception ex)
                {
                    Core.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
                    card.UnlockCard();
                }
            }
        }

        private static void OnCoilgunPostFire(int pawnKills, float totalDamage, CoilgunShellDef shellDef)
        {
            foreach (var card in AchievementPointManager.GetCards<CoilgunPostFireTracker>())
            {
                try
                {
                    if ((card.tracker as CoilgunPostFireTracker).Trigger(pawnKills, totalDamage, shellDef))
                    {
                        card.UnlockCard();
                    }
                }
                catch (Exception ex)
                {
                    Core.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
                    card.UnlockCard();
                }
            }
        }

        private static void OnCoilgunExplosion(Explosion e, int count)
        {
            foreach (var card in AchievementPointManager.GetCards<CoilgunExplosiveTracker>())
            {
                try
                {
                    if ((card.tracker as CoilgunExplosiveTracker).Trigger(e, count))
                    {
                        card.UnlockCard();
                    }
                }
                catch (Exception ex)
                {
                    Core.Error($"Unable to trigger event for card validation. To avoid further errors {card.def.LabelCap} has been automatically unlocked.\n\nException={ex.Message}");
                    card.UnlockCard();
                }
            }
        }
    }
}
