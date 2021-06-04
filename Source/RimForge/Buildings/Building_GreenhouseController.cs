using RimWorld;
using System.Collections.Generic;
using RimForge.Comps;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_GreenhouseController : Building, ICustomOverlayDrawer
    {
        public const int INTERVAL = 30;

        [TweakValue("_RimForge", 0, 0.1f)]
        public static float MoteChance = 0.022f;
        [TweakValue("_RimForge", 0, 1f)]
        public static float MoteMainChance = 0.5f;
        [TweakValue("_RimForge", 0.01f, 2f)]
        public static float MoteScaleMin = 0.45f;
        [TweakValue("_RimForge", 0.01f, 2f)]
        public static float MoteScaleMax = 1.4f;
        [TweakValue("_RimForge", 0.01f, 1f)]
        public static float MoteOffsetRadius = 0.3f;
        [TweakValue("_RimForge", 0, 1000f)]
        public static float MoteRotation = 360f;

        public string OverlayTexturePath => "RF/UI/WarningIcon";

        public override Graphic Graphic
        {
            get
            {
                if (!IsRunning)
                    return base.DefaultGraphic;

                if (Content.GreenhouseActiveFrames == null)
                    Content.LoadGreenhouseFrames(this);

                return Content.GreenhouseActiveFrames[frame];
            }
        }

        public bool IsRunning { get; private set; }
        public Room GreenhouseRoom => GreenhouseCell.Impassable(Map) ? null : GreenhouseCell.GetRoom(Map, RegionType.Set_All);
        public IntVec3 GreenhouseCell => Position + new Rot4(cellRotation).AsVector2.FlatToWorld(0).ToIntVec3();
        public CompPowerTrader PowerTrader => trader ??= GetComp<CompPowerTrader>();

        private int frame;
        private CompPowerTrader trader;
        private int cellRotation;
        private int tickCounter;
        private int tickCounter2;

        public virtual float GetSpecialPlantGrowthPerTick(Plant plant)
        {
            if (plant == null || !plant.Spawned || plant.Blighted)
                return 0f;

            if (plant.LifeStage != PlantLifeStage.Growing)
                return 0f;

            float rateBase = plant.GrowthRateFactor_Fertility * plant.GrowthRateFactor_Temperature; // Ignores light level, although light level is forced to 100% inside the greenhouse.
            return (float) (1.0 / (60000.0 * plant.def.plant.growDays)) * rateBase * Settings.GreenhouseGrowthAccelerationFactor * (INTERVAL);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref cellRotation, "RF_greenhouseRotation");
        }

        public override void Tick()
        {
            base.Tick();

            tickCounter2++;
            if (tickCounter2 % 3 == 0)
            {
                frame++;
                if (frame > 14)
                    frame = 0;
            }

            tickCounter++;
            if (tickCounter < INTERVAL)
                return;
            tickCounter = 0;

            IsRunning = false;

            // Update power consumption.
            var room = GreenhouseRoom;
            UpdatePowerUsage(room);

            if (!PowerTrader.PowerOn)
                return;

            if (room == null || GetRoomError(room) != null)
                return;

            IsRunning = true;

            if (Settings.GreenhouseGrowthAccelerationFactor <= 0f)
                return;

            var map = Map;
            Color32 light = new Color32(255, 255, 255, 1);
            foreach (var cell in room.Cells) // Note: It may be better to use the Room's ThingLister. Not sure which is faster.
            {
                int index = map.cellIndices.CellToIndex(cell);
                map.glowGrid.glowGrid[index] = light;
                var list = map.thingGrid.ThingsListAtFast(index);
                for(int i = 0; i < list.Count; i++)
                {
                    var thing = list[i];
                    if (thing is Plant plant)
                    {
                        float growthPerTick = GetSpecialPlantGrowthPerTick(plant);

                        float growthInt = plant.Growth;
                        int num = plant.LifeStage == PlantLifeStage.Mature ? 1 : 0;
                        plant.Growth += growthPerTick;
                        if ((num == 0 && plant.LifeStage == PlantLifeStage.Mature || (int)(growthInt * 10.0) != (int)(plant.Growth * 10.0)))
                            map.mapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);

                        if (Rand.Chance(MoteChance))
                        {
                            var mote = MoteMaker.MakeStaticMote(cell.ToVector3Shifted() + Rand.InsideUnitCircleVec3 * MoteOffsetRadius, map, RFDefOf.RF_Motes_Growth, Rand.Range(MoteScaleMin, MoteScaleMax));
                            if (mote != null)
                            {
                                mote.rotationRate = Rand.Value * MoteRotation * Rand.Sign;
                            }
                        }
                    }
                }
            }

            if (Rand.Chance(MoteMainChance))
            {
                var mote = (MoteThrown)MoteMaker.MakeStaticMote(Position.ToVector3Shifted() + Rand.InsideUnitCircleVec3 * 0.2f + new Vector3(0, 0, -0.1f), map, RFDefOf.RF_Motes_Air, Rand.Range(0.25f, 0.4f));
                if (mote != null)
                {
                    mote.Velocity = new Vector3(0, 0, Rand.Range(1, 2));
                }
            }
        }

        private void UpdatePowerUsage(Room room)
        {
            if (room != null && GetRoomError(room) == null)
            {
                int count = room.CellCount;
                float watts = Settings.GreenWattsPerCell * count;
                PowerTrader.PowerOutput = -Mathf.Max(50, watts);
            }
            else
            {
                PowerTrader.PowerOutput = -50;
            }
        }

        public string GetRoomError(Room room)
        {
            if (room == null)
                return "RF.Greenhouse.RoomMissing".Translate();

            if (room.TouchesMapEdge)
                return "RF.Greenhouse.RoomOutdoors".Translate();

            if (room.CellCount > Settings.MaxGreenhouseSize)
                return "RF.Greenhouse.RoomTooBig".Translate(Settings.MaxGreenhouseSize, room.CellCount);

            if (room.UsesOutdoorTemperature) // Note: this only check that there is at least 75% roof in the room. There can still be vents, open doors etc. that make temperature control difficult.
                return "RF.Greenhouse.RoomNotSealed".Translate();

            return null;
        }

        public override void Draw()
        {
            base.Draw();

            // There is power but there is an error with the room. Let the user know.
            if (PowerTrader.PowerOn && GetRoomError(GreenhouseRoom) != null)
            {
                // Draws the warning icon because of the Harmony patch.
                Map.overlayDrawer.DrawOverlay(this, OverlayTypes.BrokenDown);
            }
        }

        private static readonly List<IntVec3> cells = new List<IntVec3>(512);
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            GenDraw.DrawCircleOutline(GreenhouseCell.ToVector3Shifted(), 0.45f, SimpleColor.Cyan);

            var room = GreenhouseRoom;
            if (GetRoomError(room) != null)
                return;

            cells.Clear();
            cells.AddRange(room.Cells);
            cells.Add(Position);

            GenDraw.DrawFieldEdges(cells, Color.green);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Command_Action()
            {
                defaultLabel = "RF.Greenhouse.Rotate".Translate(),
                defaultDesc = "RF.Greenhouse.RotateDesc".Translate(),
                action = () =>
                {
                    cellRotation++;
                    if (cellRotation >= 4)
                        cellRotation = 0;
                },
                icon = Content.ArrowIcon,
                defaultIconColor = Color.cyan,
                iconAngle = new Rot4(cellRotation).AsAngle - 90f
            };
        }

        public override string GetInspectString()
        {
            string error = GetRoomError(GreenhouseRoom);
            return $"{base.GetInspectString().TrimEnd()}{(error == null ? "" : $"\nError: {error}")}";
        }
    }
}
