using LudeonTK;
using RimWorld;
using Verse;

namespace RimForge.Buildings
{
    public class Building_LightningRod : Building
    {
        [DebugAction("RimForge", "List Lightning Rods")]
        private static void Debug_MapRods()
        {
            var tracker = Find.CurrentMap?.GetComponent<RodTracker>();
            if (tracker == null)
            {
                Core.Error("Failed to find current map or current map rod tracker component.");
                return;
            }

            Core.Log($"There are {tracker.RodsReadOnly.Count} lightning rods on this map:");
            foreach (var thing in tracker.RodsReadOnly)
            {
                Core.Log(thing.Position.ToString());
            }
        }

        public const float Radius = 100;
        public const float LightningCharge = 2000; // Each strike charges 2000 Wd.

        public override void PostMapInit()
        {
            base.PostMapInit();
            Register();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Register(map);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Remove();
            base.Destroy(mode);
        }

        public override void Notify_MyMapRemoved()
        {
            Remove();
            base.Notify_MyMapRemoved();
        }

        private void Register(Map overrideMap = null)
        {
            var map = overrideMap ?? Map;
            var tracker = map?.GetComponent<RodTracker>();
            if (tracker == null)
            {
                Core.Error("Failed to register, null map or tracker component.");
                return;
            }

            tracker.Register(this);
        }

        private void Remove()
        {
            var map = Map;
            var tracker = map?.GetComponent<RodTracker>();
            if (tracker == null)
            {
                Core.Error("Failed to un-register, null map or tracker component.");
                return;
            }

            tracker.UnRegister(this);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(Position, Radius);
        }

        /// <summary>
        /// Called when struck by lightning.
        /// </summary>
        public virtual void UponStruck()
        {
            //Core.Log("Zap! Lightning rod got struck by lightning!");

            var power = PowerComp;
            var net = power?.PowerNet;
            if (net == null)
            {
                Messages.Message("RF.LR.FailNoPowerNet".Translate(), MessageTypeDefOf.NegativeEvent);
                return;
            }

            var batteries = net.batteryComps;
            if (batteries == null || batteries.Count == 0)
            {
                Messages.Message("RF.LR.FailNoBatteries".Translate(), MessageTypeDefOf.NegativeEvent);
                return;
            }

            float toCharge = LightningCharge;
            float added = 0;
            foreach(var bat in batteries)
            {
                if (bat == null || bat.parent.DestroyedOrNull())
                    continue;

                float canAdd = bat.AmountCanAccept;
                if (canAdd == 0)
                    continue;

                if(canAdd < toCharge)
                {
                    toCharge -= canAdd;
                    added += canAdd;
                    bat.AddEnergy(canAdd);
                    continue;
                }
                if(canAdd >= toCharge)
                {
                    added += toCharge;
                    bat.AddEnergy(toCharge);
                    break;
                }
            }
            Messages.Message("RF.LR.Success".Translate(added.ToString("F0")), MessageTypeDefOf.SilentInput);
        }
    }
}
