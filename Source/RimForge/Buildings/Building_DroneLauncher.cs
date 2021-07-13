using System;
using System.Collections.Generic;
using RimForge.Achievements;
using RimForge.Airstrike;
using RimForge.CombatExtended;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimForge.Buildings
{
    public class Building_DroneLauncher : Building, ICustomTargetingUser
    {
        // projectileWhenLoaded
        // MortarShells

        public static readonly List<ThingDef> LoadableBombs = new List<ThingDef>();

        [TweakValue("_RimForge", 0, 480)]
        public static int TicksToLeave = 120;
        [TweakValue("_RimForge", 0, 480)]
        public static int TicksInMiddle = 300;
        [TweakValue("_RimForge", 0, 480)]
        public static int TicksToReturn = 190;

        private static readonly AnimationCurve launchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        private static readonly List<IntVec3> tempCells = new List<IntVec3>();
        private static readonly List<IntVec3> tempCells2 = new List<IntVec3>();

        public ThingDef CurrentShellDef
        {
            get
            {
                var comp = GetComp<CompRefuelable>();
                return comp.Props.fuelFilter.AnyAllowedDef;
            }
            set
            {
                var comp = GetComp<CompRefuelable>();
                comp.Props.fuelFilter.SetDisallowAll();
                if (value != null)
                    comp.Props.fuelFilter.SetAllow(value, true);

                shellDef = value;
            }
        }
        public int LoadedShellCount
        {
            get
            {
                var comp = GetComp<CompRefuelable>();
                return Mathf.RoundToInt(comp.Fuel);
            }
            set
            {
                var comp = GetComp<CompRefuelable>();
                comp.ConsumeFuel(comp.Fuel);
                comp.Refuel(value);
            }
        }
        public int MaxLoadedShellCount
        {
            get
            {
                var comp = GetComp<CompRefuelable>();
                return Mathf.RoundToInt(comp.Props.fuelCapacity);
            }
        }

        private Vector3 droneDrawPos;
        private bool drawAffectedCells;
        private IntVec3? firstPosition;
        private IntVec3? secondPosition;
        private bool keepGoing;
        private int ticksFlying;
        private bool isFlying;
        private bool isBlocked;
        private string blockedBy;
        private int timer;
        private ThingDef shellDef;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            drawAffectedCells = false;
            firstPosition = null;
            secondPosition = null;
            keepGoing = false;

            isFlying = false;
            ticksFlying = 0;

            CurrentShellDef ??= RFDefOf.Shell_HighExplosive;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref isBlocked, "RF_isBlocked");
            Scribe_Defs.Look(ref shellDef, "RF_loadedShell");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                ReplaceFuelProps(GetComp<CompRefuelable>());

            CurrentShellDef = shellDef;
        }

        public override void PostMake()
        {
            base.PostMake();
            ReplaceFuelProps(GetComp<CompRefuelable>());
            CurrentShellDef = shellDef;
        }

        public void ReplaceFuelProps(CompRefuelable comp)
        {
            var props = comp.Props;
            var newProps = props.CloneObject();
            newProps.fuelFilter = new ThingFilter();
            comp.props = newProps;
        }

        public override void Tick()
        {
            base.Tick();

            timer++;
            if (timer % 60 == 0)
            {
                isBlocked = false;
                blockedBy = null;
                foreach (var cell in GetLaunchCells())
                {
                    if (IsCellBlocked(cell, out blockedBy))
                    {
                        isBlocked = true;
                        break;
                    }
                }
            }

            var comp = GetComp<CompRefuelable>();
            comp.Props.fuelLabel = CurrentShellDef.LabelCap;
            comp.Props.fuelGizmoLabel = CurrentShellDef.LabelCap;

            if (isFlying)
            {
                bool spawnSmoke = false;
                bool spawnFire = false;
                ticksFlying++;
                if (ticksFlying <= TicksToLeave)
                {
                    Vector3 start = GetDroneIdlePos();
                    Vector3 end = GetDroneIdlePos() + GetDroneTravelDirection() * 350;
                    float p = Mathf.Clamp01((float) ticksFlying / TicksToLeave);
                    float t = launchCurve.Evaluate(p);
                    droneDrawPos = Vector3.Lerp(start, end, t);
                    spawnSmoke = true;
                    spawnFire = true;
                }
                else if(ticksFlying >= TicksToLeave + TicksInMiddle)
                {
                    if(ticksFlying > TicksToLeave + TicksInMiddle + TicksToReturn)
                    {
                        isFlying = false;
                    }
                    else
                    {
                        Vector3 start = GetDroneIdlePos() + GetDroneReturnDirection() * 350;
                        Vector3 end = GetDroneIdlePos();
                        float p = Mathf.Clamp01((float)(ticksFlying - TicksToLeave - TicksInMiddle) / TicksToReturn);
                        float t = launchCurve.Evaluate(p);
                        droneDrawPos = Vector3.Lerp(start, end, t);
                        spawnSmoke = true;
                    }
                }
                
                Map map = Map;
                if (spawnFire)
                {

                    Vector3 pos2 = droneDrawPos + Rand.InsideUnitCircleVec3 * 0.5f;
                    if (pos2.ToIntVec3().InBounds(map))
                    {
#if V13
                        FleckMaker.ThrowFireGlow(pos2, map, 1f);
#else
                        MoteThrown moteThrown2 = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_FireGlow);
                        moteThrown2.Scale = Rand.Range(4f, 6f) * 1f;
                        moteThrown2.rotationRate = Rand.Range(-3f, 3f);
                        moteThrown2.exactPosition = pos2;
                        moteThrown2.SetVelocity(Rand.Range(0, 360), 0.12f);
                        GenSpawn.Spawn(moteThrown2, pos2.ToIntVec3(), map);
#endif
                    }
                }
                if (spawnSmoke)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 pos = droneDrawPos + Rand.InsideUnitCircleVec3 * 2f;
                        if (pos.ToIntVec3().InBounds(map))
                        {
#if V13
                            FleckCreationData dataStatic = FleckMaker.GetDataStatic(pos, map, FleckDefOf.Smoke, Rand.Range(1.5f, 2.5f) * 1f);
                            dataStatic.spawnPosition = pos;
                            dataStatic.rotationRate = Rand.Range(-30f, 30f);
                            dataStatic.velocityAngle = (float)Rand.Range(30, 40);
                            dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
                            map.flecks.CreateFleck(dataStatic);
#else
                            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_Smoke);
                            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * 1f;
                            moteThrown.rotationRate = Rand.Range(-30f, 30f);
                            moteThrown.exactPosition = pos;
                            moteThrown.SetVelocity(Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
                            GenSpawn.Spawn(moteThrown, pos.ToIntVec3(), map);
#endif
                        }
                    }
                }
            }
            else
            {
                droneDrawPos = GetDroneIdlePos();
            }
        }

        public Vector3 GetDroneTravelDirection()
        {
            if (Rotation == Rot4.East)
                return new Vector3(5, 0, 1).normalized;
            if (Rotation == Rot4.West)
                return new Vector3(-5, 0, 1).normalized;
            if (Rotation == Rot4.North)
                return new Vector3(0, 0, 1);
            return new Vector3(0, 0, -1);
        }
        
        public Vector3 GetDroneReturnDirection()
        {
            if(Rotation == Rot4.East)
                return new Vector3(-15, 0, 1).normalized;
            if (Rotation == Rot4.West)
                return new Vector3(15, 0, 1).normalized;
            if (Rotation == Rot4.North)
                return new Vector3(0, 0, -1);
            return new Vector3(0, 0, 1);
        }

        public Vector3 GetDroneIdlePos()
        {
            if(Rotation.IsHorizontal)
                return DrawPos + new Vector3(0, 0, 0.35f);
            if (Rotation == Rot4.North)
                return DrawPos + new Vector3(0, 0, -0.25f);
            return DrawPos + new Vector3(0, 0, 0.5f);
        }

        public override void Draw()
        {
            base.Draw();

            Graphic g = null;
            switch (Rotation.AsInt)
            {
                case 0:
                    g = Content.DroneNorth;
                    break;
                case 1:
                    g = Content.DroneEast;
                    break;
                case 2:
                    g = Content.DroneSouth;
                    break;
                case 3:
                    g = Content.DroneWest;
                    break;
            }

            droneDrawPos.y = DrawPos.y + 0.00001f;
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(droneDrawPos, Quaternion.identity, new Vector3(g.drawSize.x, 1f, g.drawSize.y)), g.MatSingle, 0);
        }

        public void EjectLoadedShells()
        {
            var loaded = CurrentShellDef;
            if (loaded == null)
                return;

            if (LoadedShellCount <= 0)
                return;

            var thing = ThingMaker.MakeThing(loaded);
            thing.stackCount = LoadedShellCount;
            GenPlace.TryPlaceThing(thing, Position - new IntVec3(0, 0, 3), Map, ThingPlaceMode.Near);

            ClearLoadedShells();
        }

        public void ClearLoadedShells()
        {
            LoadedShellCount = 0;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
                yield return item;

            yield return new Command_Action()
            {
                defaultLabel = "RF.DL.ChangeShellLabel".Translate(),
                defaultDesc = "RF.DL.ChangeShellDesc".Translate(),
                action = () =>
                {
                    Func<ThingDef, string> labelGetter = shell => shell.LabelCap;
                    Func<ThingDef, Action> actionGetter = shell =>
                    {
                        if (shell == CurrentShellDef)
                            return null;
                        return () =>
                        {
                            EjectLoadedShells();
                            CurrentShellDef = shell;
                        };
                    };
                    FloatMenuUtility.MakeMenu(LoadableBombs, labelGetter, actionGetter);
                },
                icon = CurrentShellDef?.uiIcon
            };

            yield return new Command_TargetCustom()
            {
                times = 2,
                defaultLabel = "RF.DL.TargetLabel".Translate(),
                defaultDesc = "RF.DL.TargetDesc".Translate(),
                icon = Content.MissilesIcon,
                defaultIconColor = Color.yellow,
                disabled = drawAffectedCells || isBlocked || LoadedShellCount < 1,
                disabledReason = LoadedShellCount < 1 ? "RF.DL.NoShells".Translate() : isBlocked ? "RF.DL.Blocked".Translate(blockedBy) : "RF.DL.AlreadyTargeting".Translate(),
                continueCheck = () => keepGoing,
                action = (t, i) =>
                {
                    if (!t.IsValid)
                        return;

                    if(LoadedShellCount == 1)
                    {
                        firstPosition = t.Cell;
                        secondPosition = t.Cell;
                        keepGoing = false;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            firstPosition = t.Cell;
                            keepGoing = true;
                        }
                        else
                            secondPosition = t.Cell;
                    }

                    if (i == 1 || LoadedShellCount == 1)
                    {
                        const float MIN_DST = 5f;
                        bool hasDistance = LoadedShellCount == 1 || (firstPosition.Value - secondPosition.Value).LengthHorizontal >= MIN_DST;

                        if(hasDistance)
                            DoStrike();
                        else
                            Messages.Message("RF.DL.NotEnoughDistance".Translate(MIN_DST), MessageTypeDefOf.RejectInput, false);

                        firstPosition = null;
                        secondPosition = null;
                    }
                },
                user = this,
                targetingParams = new TargetingParameters()
                {
                    canTargetLocations = true,
                    canTargetPawns = false,
                    canTargetFires = false,
                    mapObjectTargetsMustBeAutoAttackable = false
                }
            };
        }

        public void DoStrike()
        {
            bool isAntimatter = CurrentShellDef == ThingDefOf.Shell_AntigrainWarhead;

            if (isAntimatter)
            {
                GenericEventTracker.Fire(AchievementEvent.DroneAntimatter);

                if (LoadedShellCount >= MaxLoadedShellCount)
                {
                    GenericEventTracker.Fire(AchievementEvent.DroneAntimatterFull);
                }
            }

            GenAirstrike.DoStrike(this, CECompat.IsCEActive ? CECompat.GetProjectile(CurrentShellDef) : CurrentShellDef.projectileWhenLoaded, firstPosition.Value, secondPosition.Value, LoadedShellCount, 200, RFDefOf.RF_Sound_Drone);
            ClearLoadedShells();

            isFlying = true;
            ticksFlying = 0;

            SoundInfo info = SoundInfo.InMap(this);
            RFDefOf.RF_Sound_DroneLaunch.PlayOneShot(info);
        }

        public IEnumerable<IntVec3> GetLaunchCells()
        {
            foreach (var cell in GenAdj.CellsOccupiedBy(Position, Rotation, this.def.size + new IntVec2(2, 4)))
                yield return cell;
        }

        public bool IsCellBlocked(IntVec3 cell, out string blockedBy)
        {
            Map map = Map;
            var roof = cell.GetRoof(map);
            if (roof != null)
            {
                blockedBy = roof.LabelCap;
                return true;
            }

            foreach (var thing in cell.GetThingList(map))
            {
                if (thing == this)
                    continue;

                if (thing is Building b)
                {
                    if (b.def.altitudeLayer >= AltitudeLayer.DoorMoveable)
                    {
                        blockedBy = b.LabelShortCap;
                        return true;
                    }
                }
            }

            blockedBy = null;
            return false;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            tempCells.Clear();
            tempCells2.Clear();
            foreach (var cell in GetLaunchCells())
            {
                tempCells2.Add(cell);
                if (IsCellBlocked(cell, out _))
                {
                    tempCells.Add(cell);
                }
            }
            if (tempCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(tempCells2, Color.white);
                GenDraw.DrawFieldEdges(tempCells, Color.red);
            }

            if (!drawAffectedCells)
                return;

            float radius;
            if (CECompat.IsCEActive)
                radius = CECompat.GetExplosionRadius(CurrentShellDef);
            else
                radius = CurrentShellDef?.projectileWhenLoaded?.projectile?.explosionRadius ?? -1;

            if (firstPosition == null || secondPosition == null)
            {
                if (firstPosition != null && LoadedShellCount == 1)
                    GenAirstrike.DrawStrikePreview(firstPosition.Value, firstPosition.Value, Map, 1, radius);
                return;
            }

            GenAirstrike.DrawStrikePreview(firstPosition.Value, secondPosition.Value, Map, LoadedShellCount, radius);
        }

        public void OnStartTargeting(int index)
        {
            drawAffectedCells = true;
            if (index == 0)
            {
                firstPosition = null;
                secondPosition = null;
                keepGoing = false;
            }
        }

        public void OnStopTargeting(int index)
        {
            drawAffectedCells = false;
        }

        public void SetTargetInfo(LocalTargetInfo info, int index)
        {
            if (!info.IsValid)
                return;

            if (index == 0)
                firstPosition = info.Cell;
            else
                secondPosition = info.Cell;
        }
    }
}
