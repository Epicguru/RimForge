using System;
using System.Collections.Generic;
using RimForge.Buildings.DiscoPrograms;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_DJStand : Building
    {
        private static readonly Queue<IntVec3> openNodes = new Queue<IntVec3>(64);
        private static readonly HashSet<IntVec3> closedNodes = new HashSet<IntVec3>(64);
        private static readonly List<FloatMenuOption> options = new List<FloatMenuOption>();

        public static void FloodFillDiscoFloorCells(Map map, IntVec3 startCell, int maxSize, ref List<IntVec3> cells)
        {
            if (map == null)
                return;

            cells ??= new List<IntVec3>(64);
            cells.Clear();
            openNodes.Clear();
            closedNodes.Clear();

            var floorDef = RFDefOf.RF_DiscoFloor;
            var terrainGrid = map.terrainGrid;
            var thingGrid = map.thingGrid;

            bool IsDiscoFloorAndPassable(IntVec3 cell)
            {
                if (!cell.InBounds(map))
                    return false;

                if (terrainGrid.TerrainAt(cell) != floorDef)
                    return false;

                //foreach (var thing in thingGrid.ThingsListAtFast(cell))
                //{
                //    if (thing is Building b && b.def.passability == Traversability.Impassable)
                //        return false;
                //}

                return cell.Walkable(map);
            }

            if (IsDiscoFloorAndPassable(startCell))
                openNodes.Enqueue(startCell);

            while (openNodes.Count > 0)
            {
                IntVec3 current = openNodes.Dequeue();
                cells.Add(current);
                closedNodes.Add(current);
                if (cells.Count >= maxSize)
                    break;

                // Try add neighbors.
                for (int x = current.x - 1; x <= current.x + 1; x++)
                {
                    IntVec3 cell = new IntVec3(x, current.y, current.z);
                    if (closedNodes.Contains(cell) || openNodes.Contains(cell))
                        continue;

                    if (IsDiscoFloorAndPassable(cell))
                        openNodes.Enqueue(cell);
                }
                for (int z = current.z - 1; z <= current.z + 1; z++)
                {
                    IntVec3 cell = new IntVec3(current.x, current.y, z);
                    if (closedNodes.Contains(cell) || openNodes.Contains(cell))
                        continue;

                    if (IsDiscoFloorAndPassable(cell))
                        openNodes.Enqueue(cell);
                }
            }

            openNodes.Clear();
            closedNodes.Clear();
        }

        public DiscoProgram CurrentProgram;

        private List<IntVec3> floorCells = new List<IntVec3>(Settings.DiscoMaxFloorSize);
        private int tickCounter;
        private DiscoFloorGlowGrid glowGrid;
        private MaterialPropertyBlock block;

        public override void Tick()
        {
            base.Tick();

            if (floorCells.Count >= 2 && glowGrid != null)
            {
                try
                {
                    TickFloor();
                }
                catch (Exception e)
                {
                    Core.Error("Exception ticking disco floor. That's wiggity wack, yo.", e);
                }
            }

            tickCounter++;
            if (tickCounter % 120 != 0 || floorCells.Count >= 2)
                return;

            RecalculateFloor();
        }

        public void RecalculateFloor()
        {
            FloodFillDiscoFloorCells(Map, Position, Settings.DiscoMaxFloorSize, ref floorCells);
            glowGrid = floorCells.Count >= 2 ? new DiscoFloorGlowGrid(floorCells) : null;

            block = new MaterialPropertyBlock();
        }

        public void TickFloor()
        {
            if (CurrentProgram == null)
            {
                glowGrid.ClearAll();
            }
            else
            {
                CurrentProgram.DJStand = this;
                try
                {
                    CurrentProgram.Tick();
                }
                catch (Exception e)
                {
                    Core.Error($"Exception ticking disco floor program '{CurrentProgram.GetType().Name}'", e);
                }

                try
                {
                    glowGrid.SetAllColors(cell => CurrentProgram.ColorFor(cell));
                }
                catch (Exception e)
                {
                    Core.Error($"Exception getting colors from disco floor program '{CurrentProgram.GetType().Name}'", e);
                }

                CurrentProgram.TickCounter++;
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (glowGrid != null && floorCells.Count >= 2)
                DrawFloor();
        }

        public void DrawFloor()
        {
            if (Content.DiscoFloorGlowGraphic == null)
                Content.LoadDiscoFloorGraphics(this);

            float aMulti = Settings.DiscoFloorColorIntensity;
            
            foreach (var cell in floorCells)
            {
                glowGrid.GetColorAndMatrix(cell, out var color, out var matrix);
                color.a *= aMulti;
                if (color.a == 0f)
                    continue;

                block.SetColor("_Color", color);
                Graphics.DrawMesh(MeshPool.plane10, matrix, Content.DiscoFloorGlowGraphic.MatNorth, 0, Find.Camera, 0, block);
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            GenDraw.DrawFieldEdges(floorCells, Color.cyan);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            if (!Prefs.DevMode)
                yield break;

            yield return new Command_Action()
            {
                defaultLabel = "Recalculate floor",
                action = RecalculateFloor
            };

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("None", () =>
                {
                    CurrentProgram = null;
                }));
                foreach (var d in DefDatabase<DiscoProgramDef>.AllDefsListForReading)
                {
                    options.Add(new FloatMenuOption(d.defName, () =>
                    {
                        CurrentProgram = d.MakeProgram();
                    }));
                }
            }

            yield return new Command_Action()
            {
                defaultLabel = "Change program",
                action = () =>
                {
                    Find.WindowStack.Add(new FloatMenu(options, "Select a program"));
                }
            };
        }

        public override string GetInspectString()
        {
            return $"{floorCells.Count} disco floor tiles. Let's groove!";
        }
    }
}
