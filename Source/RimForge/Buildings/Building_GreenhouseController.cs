using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_GreenhouseController : Building
    {
        [TweakValue("_RimForge", 0, 0.1f)]
        public static float MoteChance = 0.0005f;
        [TweakValue("_RimForge", 0.01f, 2f)]
        public static float MoteScaleMin = 0.3f;
        [TweakValue("_RimForge", 0.01f, 2f)]
        public static float MoteScaleMax = 0.9f;
        [TweakValue("_RimForge", 0.01f, 1f)]
        public static float MoteOffsetRadius = 0.3f;
        [TweakValue("_RimForge", 0, 1000f)]
        public static float MoteRotation = 360f;

        public Room GreenhouseRoom => InteractionCell.GetRoom(Map, RegionType.Set_All);

        public virtual float GetSpecialPlantGrowthPerTick(Plant plant)
        {
            if (plant == null || !plant.Spawned || plant.Blighted)
                return 0f;

            if (plant.LifeStage != PlantLifeStage.Growing)
                return 0f;

            float rateBase = plant.GrowthRateFactor_Fertility * plant.GrowthRateFactor_Temperature; // Ignores light level, although light level is forced to 100% inside the greenhouse.
            return (float) (1.0 / (60000.0 * plant.def.plant.growDays)) * rateBase * Settings.GreenhouseGrowthAccelerationFactor;
        }

        public override void Tick()
        {
            base.Tick();

            var room = GreenhouseRoom;
            if (room == null || GetRoomError(room) != null)
                return;

            if (Settings.GreenhouseGrowthAccelerationFactor <= 0f)
                return;

            var map = Map;
            Color32 light = new Color32(255, 255, 255, 1);
            foreach (var cell in room.Cells)
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
        }

        public string GetRoomError(Room room)
        {
            if (room == null)
                return "RF.Greenhouse.RoomMissing".Translate();

            if (room.TouchesMapEdge)
                return "RF.Greenhouse.RoomOutdoors".Translate();

            if (room.UsesOutdoorTemperature)
                return "RF.Greenhouse.RoomNotSealed".Translate();

            if (room.CellCount > Settings.MaxGreenhouseSize)
                return "RF.Greenhouse.RoomTooBig".Translate(Settings.MaxGreenhouseSize, room.CellCount);

            return null;
        }

        private static readonly List<IntVec3> cells = new List<IntVec3>(512);
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            var room = GreenhouseRoom;
            if (GetRoomError(room) != null)
                return;

            cells.Clear();
            cells.AddRange(room.Cells);

            GenDraw.DrawFieldEdges(cells, Color.green);
        }

        public override string GetInspectString()
        {
            string error = GetRoomError(GreenhouseRoom);
            return $"{base.GetInspectString().TrimEnd()}{(error == null ? "" : $"\n{error}")}";
        }
    }
}
