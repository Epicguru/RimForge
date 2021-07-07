using System;
using RimForge.Effects;
using RimWorld;
using System.Collections.Generic;
using RimForge.Achievements;
using RimForge.Comps;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_TeslaCoil : Building
    {
        private static readonly List<Pawn> pawns = new List<Pawn>(128);

        public Building_TeslaCoil LinkedTo
        {
            get
            {
                if (_linkedTo != null && _linkedTo.Destroyed)
                    _linkedTo = null;
                
                return _linkedTo;
            }
            set
            {
                if (value == this)
                {
                    Core.Error("Cannot link coil to self!");
                    return;
                }
                _linkedTo = value;
            }
        }
        public string BlockedBy { get; protected set; }
        public Vector3 TopPos => DrawPos + new Vector3(0, 0, 0.55f);

        public bool HasPower
        {
            get
            {
                if (Power != null)
                    return Power.PowerOn;
                return Wireless?.IsActive ?? false;
            }
        }

        public CompPowerTrader Power { get; private set; }
        public CompWirelessPower Wireless { get; private set; }
        public bool IsOnCooldown => CooldownTicksRemaining > 0;
        public int CooldownTicksRemaining;

        private Building_TeslaCoil _linkedTo;
        private int random;
        private int tickCounter;

        private float arcLifeProgress => Mathf.Clamp01(arcTicksAlive / 60f);
        private LinearElectricArc[] arcs = new LinearElectricArc[4];
        private int arcTicksAlive;
        private List<IntVec3> affectedCells;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            Power = GetComp<CompPowerTrader>();
            Wireless = GetComp<CompWirelessPower>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (_linkedTo != null && _linkedTo.Destroyed)
                _linkedTo = null;
            Scribe_References.Look(ref _linkedTo, "teslaLinkedTo");
            Scribe_Values.Look(ref CooldownTicksRemaining, "teslaCooldown");
            Scribe_Collections.Look(ref affectedCells, "teslaAffectedCells");

            random = Rand.Range(0, 100);
        }

        public override void Tick()
        {
            base.Tick();

            if(CooldownTicksRemaining > 0)
                CooldownTicksRemaining--;
            tickCounter++;
            if ((tickCounter + random) % Settings.TeslaTickInterval == 0)
            {
                // Scan even if blocked, because scanning actually detects blocking (or lack of)
                if (HasPower && LinkedTo != null && !IsOnCooldown)
                    ScanTick();
            }

            bool arcsActive = arcs[0] != null;
            if (arcsActive)
                arcTicksAlive++;
            for(int i = 0; i < arcs.Length; i++)
            {
                var arc = arcs[i];
                if (arc == null)
                    continue;

                arc.Amplitude = Vector2.Lerp(new Vector2(0.05f, 0.35f * i), new Vector2(0, 0.1f), arcLifeProgress);
                if (arcLifeProgress < 1f)
                    continue;

                arc.Destroy();
                arcs[i] = null;
            }
        }

        public virtual void ScanTick()
        {
            BlockedBy = null;
            pawns.Clear();
            bool isBlocked = false;
            foreach (var cell in GetAffectedCells())
            {
                bool keepGoing = ScanCell(cell, pawns);
                if (!keepGoing)
                {
                    isBlocked = true;
                    break;
                }
            }
            if (!isBlocked && pawns.Count > 0)
            {
                AttackNow(pawns);
            }
            pawns.Clear();
        }

        public virtual bool ScanCell(IntVec3 cell, List<Pawn> pawns)
        {
            var things = Map.thingGrid.ThingsListAtFast(cell);
            foreach (var thing in things)
            {
                if (thing == null || thing.Destroyed || !thing.Spawned)
                    continue;

                var alt = thing.def?.altitudeLayer ?? AltitudeLayer.Terrain;
                if (alt == AltitudeLayer.Building      ||
                    alt == AltitudeLayer.BuildingOnTop ||
                    alt == AltitudeLayer.DoorMoveable)
                {
                    BlockedBy = thing.LabelCap;
                    return false;
                }

                if (thing is Pawn pawn)
                {
                    if (PawnIsTarget(pawn))
                    {
                        pawns.Add(pawn);
                    }
                }
            }
            return true;
        }

        public virtual bool PawnIsTarget(Pawn pawn)
        {
            return pawn != null && !pawn.Destroyed && !pawn.Dead && !pawn.Downed && pawn.HostileTo(Faction.OfPlayerSilentFail);
        }

        public virtual void AttackNow(List<Pawn> pawns)
        {
            if (LinkedTo == null)
            {
                Core.Error("AttackNow called with null LinkedTo. Pawns will be damaged, but effects will not be spawned.");
            }

            // Make electrical arcs.
            if (LinkedTo != null)
            {
                var start = this.TopPos;
                var end = LinkedTo.TopPos;
                var map = this.Map;

                const float POINTS_PER_CELL = 3;
                float dst = (start - end).WorldToFlat().magnitude;
                int points = Mathf.Clamp(Mathf.RoundToInt(dst * POINTS_PER_CELL), 1, 100); // Max out at 100.

                for (int i = 0; i < arcs.Length; i++)
                {
                    var current = arcs[i];
                    current?.Destroy();
                    var arc = new LinearElectricArc(points);
                    arc.Start = start.WorldToFlat();
                    arc.End = end.WorldToFlat();
                    arc.Spawn(map);
                    arcs[i] = arc;
                }

                arcTicksAlive = 0;
            }

            // Make motes.
            for (int i = 0; i < 4; i++)
                FleckMaker.ThrowLightningGlow(TopPos, Map, 1);
            if (LinkedTo != null)
            {
                for (int i = 0; i < 4; i++)
                    FleckMaker.ThrowLightningGlow(LinkedTo.TopPos, Map, 1);
            }

            // TODO play sound.

            // Damage pawns.
            foreach (var pawn in pawns)
            {
                try
                {
                    AttackPawn(pawn);
                }
                catch (Exception e)
                {
                    Core.Error($"Exception when trying to data pawn with tesla coil. Pawn: '{pawn.LabelCap}'", e);
                }
            }

            GenericEventTracker.Fire(AchievementEvent.CoilsFire);

            CooldownTicksRemaining = Settings.TeslaCooldown;
        }

        public virtual void AttackPawn(Pawn pawn)
        {
            if (Settings.TeslaStunDuration > 0)
            {
                var stunner = pawn.stances?.stunner;
                if (stunner != null)
                    stunner.StunFor(Settings.TeslaStunDuration, this);
                else
                    Core.Warn($"Failed to stun pawn '{pawn.LabelCap}' because they do not have a <stance?.stunner>");
            }

            float damage = Settings.TeslaDamage;
            if (pawn.RaceProps?.IsMechanoid ?? false)
                damage *= Settings.TeslaMechanoidDamageMulti;

            if (damage > 0)
            {
                DamageDef dDef = RFDefOf.RF_Electrocution;
                var info = new DamageInfo(dDef, damage, 1, instigator: this);
                pawn.TakeDamage(info);
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (LinkedTo == null)
                return;

            Color color = Color.red;
            color.a = 0.65f;
            GenDraw.DrawFieldEdges(affectedCells, color);
        }

        public IEnumerable<IntVec3> GetAffectedCells()
        {
            if (affectedCells == null)
                yield break;
            foreach (var cell in affectedCells)
                yield return cell;
        }

        public virtual void GenerateAffectedCells(IntVec3 start, IntVec3 end, List<IntVec3> cells)
        {
            if (start.x == end.x)
            {
                // Vertical aligned.
                if(start.z < end.z)
                    for (int z = start.z + 1; z < end.z; z++)
                        cells.Add(new IntVec3(start.x, 0, z));
                else
                    for (int z = end.z - 1; z > start.z; z--)
                        cells.Add(new IntVec3(start.x, 0, z));
                return;
            }
            if (start.z == end.z)
            {
                // Horizontal aligned.
                if (start.x < end.x)
                    for (int x = start.x + 1; x < end.x; x++)
                        cells.Add(new IntVec3(x, 0, start.z));
                else
                    for (int x = end.x - 1; x > start.x; x--)
                        cells.Add(new IntVec3(x, 0, start.z));
                return;
            }

            // Freeform: Bresenham's line algorithm...
            var tempPoints = new HashSet<IntVec3>();
            MakeLine(start, end, tempPoints);

            // ... but this algorithm doesn't fill in the 'corners', allowing pawns to slip through the 'gaps'
            //     when approaching at an angle.
            //     Lets fix that.
            var toAdd = new List<IntVec3>(tempPoints.Count);
            if (tempPoints.Contains(start))
                tempPoints.Remove(start);
            if (tempPoints.Contains(end))
                tempPoints.Remove(end);
            foreach (var point in tempPoints)
            {
                bool tr = tempPoints.Contains(new IntVec3(point.x + 1, 0, point.z + 1));
                bool br = tempPoints.Contains(new IntVec3(point.x + 1, 0, point.z - 1));
                bool tl = tempPoints.Contains(new IntVec3(point.x - 1, 0, point.z + 1));
                bool bl = tempPoints.Contains(new IntVec3(point.x - 1, 0, point.z - 1));

                if (tr)
                    toAdd.Add(new IntVec3(point.x + 1, 0, point.z));
                if (bl)
                    toAdd.Add(new IntVec3(point.x, 0, point.z - 1));

                if (br)
                    toAdd.Add(new IntVec3(point.x, 0, point.z - 1));
                if(tl)
                    toAdd.Add(new IntVec3(point.x - 1, 0, point.z));

            }
            foreach (var point in toAdd)
            {
                // Important note: there are duplicate positions in toAdd.
                // However, since tempPoints is a HashSet, it discards these duplicate items.
                tempPoints.Add(point);
            }

            foreach (var point in tempPoints)
                cells.Add(point);
        }

        public static void MakeLine(IntVec3 start, IntVec3 end, ICollection<IntVec3> cells)
        {
            if (cells == null)
                return;

            // Based on:
            // https://jstutorial.medium.com/coding-your-first-algorithm-bc0fc2a4e862

            int x1 = start.x;
            int x2 = end.x;
            int y1 = start.z;
            int y2 = end.z;

            // Calculate line deltas
            float dx = x2 - x1;
            float dy = y2 - y1;

            // Create a positive copy of deltas (makes iterating easier)
            float dx1 = Math.Abs(dx);
            float dy1 = Math.Abs(dy);

            // Calculate error intervals for both axis
            float px = 2 * dy1 - dx1;
            float py = 2 * dx1 - dy1;

            int x, y;

            // The line is X-axis dominant
            if (dy1 <= dx1)
            {
                // Line is drawn left to right
                int xe;
                if (dx >= 0)
                {
                    x = x1;
                    y = y1;
                    xe = x2;
                }
                else
                { 
                    // Line is drawn right to left (swap ends)
                    x = x2;
                    y = y2;
                    xe = x1;
                }
                cells.Add(new IntVec3(x, 0, y));

                // Rasterize the line
                for (int i = 0; x < xe; i++)
                {
                    x = x + 1;
                    // Deal with octants...
                    if (px < 0)
                    {
                        px = px + 2 * dy1;
                    }
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                        {
                            y += 1;
                        }
                        else
                        {
                            y -= 1;
                        }
                        px += 2 * (dy1 - dx1);
                    }
                    
                    cells.Add(new IntVec3(x, 0, y));
                }
            }
            else
            { 
                // The line is Y-axis dominant
                // Line is drawn bottom to top
                int ye;
                if (dy >= 0)
                {
                    x = x1;
                    y = y1;
                    ye = y2;
                }
                else
                { 
                    // Line is drawn top to bottom
                    x = x2;
                    y = y2;
                    ye = y1;
                }
                cells.Add(new IntVec3(x, 0, y));

                // Rasterize the line
                for (int i = 0; y < ye; i++)
                {
                    y = y + 1;
                    // Deal with octants...
                    if (py <= 0)
                    {
                        py += 2 * dx1;
                    }
                    else
                    {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0))
                        {
                            x += 1;
                        }
                        else
                        {
                            x -= 1;
                        }
                        py += 2 * (dx1 - dy1);
                    }
                    // Draw pixel from line span at
                    // currently rasterized position
                    cells.Add(new IntVec3(x, 0, y));
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            if (LinkedTo == null)
            {
                yield return new Command_Target()
                {
                    defaultLabel = "RF.Coil.LinkLabel".Translate(),
                    defaultDesc = "RF.Coil.LinkDesc".Translate(),
                    action = (thing) =>
                    {
                        // Try link to this.
                        if (!(thing.Thing is Building_TeslaCoil coil))
                            return;

                        // Target coil is already linked to me. Don't allow double-links.
                        if (coil.LinkedTo == this)
                        {
                            Messages.Message("RF.Coil.AlreadyLinked".Translate(), MessageTypeDefOf.RejectInput);
                            return;
                        }
                        float dst = (Position - coil.Position).ToVector3().WorldToFlat().magnitude;
                        if (dst > Settings.TeslaMaxDistance)
                        {
                            Messages.Message("RF.Coil.TooFar".Translate(), MessageTypeDefOf.RejectInput);
                            return;
                        }

                        this.LinkedTo = coil;
                        affectedCells ??= new List<IntVec3>();
                        affectedCells.Clear();
                        GenerateAffectedCells(Position, LinkedTo.Position, affectedCells);

                        Messages.Message("RF.Coil.LinkSuccess".Translate(), MessageTypeDefOf.SilentInput);

                    },
                    targetingParams = new TargetingParameters()
                    {
                        canTargetBuildings = true,
                        canTargetPawns = false,
                        canTargetSelf = false,
                        mapObjectTargetsMustBeAutoAttackable = false,
                        validator = info =>
                        {
                            if (!info.HasThing)
                                return false;
                            return info.Thing is Building_TeslaCoil;
                        }
                    },
                    icon = Content.LinkIcon
                };
            }
            else
            {
                yield return new Command_Action()
                {
                    defaultLabel = "RF.Coil.UnlinkLabel".Translate(),
                    action = () =>
                    {
                        LinkedTo = null;
                        affectedCells = null;
                        Messages.Message("RF.Coil.UnLinkSuccess".Translate(), MessageTypeDefOf.SilentInput);
                    },
                    icon = Content.LinkIcon,
                    defaultIconColor = Color.red
                };
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Fire now (no damage)",
                    action = () =>
                    {
                        AttackNow(pawns);
                    },
                };
            }
        }

        public override string GetInspectString()
        {
            bool onCooldown = IsOnCooldown;
            bool noPower = !HasPower;
            bool notLinked = LinkedTo == null;
            bool blocked = BlockedBy != null;

            float hours = CooldownTicksRemaining / 2500f;

            string msg = "RF.Coil.Ready".Translate();
            if (noPower)
                msg = "RF.Coil.NoPower".Translate();
            else if (onCooldown)
                msg = "RF.Coil.OnCooldown".Translate(hours.ToString("F1"));
            else if (notLinked)
                msg = "RF.Coil.NotLinked".Translate();
            else if (blocked)
                msg = "RF.Coil.Blocked".Translate(BlockedBy);

            return $"{base.GetInspectString().TrimEnd()}\n{msg}";
        }
    }
}
