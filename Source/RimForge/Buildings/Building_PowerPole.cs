using System.Collections.Generic;
using System.Threading.Tasks;
using RimForge.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_PowerPole : Building
    {
        public static List<Vector2> GeneratePoints(Building_PowerPole poleA, Building_PowerPole poleB, int pointCount, Vector2? p1 = null, Vector2? p2 = null, List<Vector2> points = null)
        {
            if (poleA.DestroyedOrNull() || poleB.DestroyedOrNull())
                return points;

            points ??= new List<Vector2>(128);
            points.Clear();

            if (pointCount < 3)
                pointCount = 3;

            Vector2 start = poleA.FlatConnectionPoint();
            Vector2 end   = poleB.FlatConnectionPoint();

            if (p1 == null)
            {
                Vector2 midA = Vector2.Lerp(start, end, 0.3f);
                p1 = midA + new Vector2(0, -1.2f);
            }
            if (p2 == null)
            {
                Vector2 midB = Vector2.Lerp(start, end, 0.7f);
                p2 = midB + new Vector2(0, -1.2f);
            }

            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                Vector2 bezier = Bezier.Evaluate(t, start, p1.Value, p2.Value, end);
                points.Add(bezier);
            }

            return points;
        }

        public static void GeneratePointsAsync(Building_PowerPole dom, Building_PowerPole sub, int pointCount, Vector2? p1 = null, Vector2? p2 = null)
        {
            if (dom.DestroyedOrNull() || sub.DestroyedOrNull())
                return;

            if (dom.poleToPoints.ContainsKey(sub))
                dom.poleToPoints[sub] = null;
            else
                dom.poleToPoints.Add(sub, null);

            Task.Run(() =>
            {
                var list = GeneratePoints(dom, sub, pointCount, p1, p2, null);
                dom.poleToPoints[sub] = list;
            });
        }

        public int LinkedPoleCount => LinkedPoles?.Count ?? 0;

        public List<Building_PowerPole> LinkedPoles;
        public List<Building_PowerPole> BackLinkedPoles = new List<Building_PowerPole>();
        private Dictionary<Building_PowerPole, List<Vector2>> poleToPoints = new Dictionary<Building_PowerPole, List<Vector2>>();

        public override void ExposeData()
        {
            base.ExposeData();

            if (LinkedPoles != null)
            {
                foreach(var item in LinkedPoles)
                {
                    if (item.BackLinkedPoles.Contains(this))
                        item.BackLinkedPoles.Remove(this);
                }
            }

            Scribe_Collections.Look(ref LinkedPoles, "pp_poles", LookMode.Reference);

            if (LinkedPoles != null)
            {
                foreach (var item in LinkedPoles)
                {
                    if(!item.BackLinkedPoles.Contains(this))
                        item.BackLinkedPoles.Add(this);
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            RegenerateAllPointsAsync();
        }

        public override void TickRare()
        {
            base.TickRare();

            for (int i = 0; i < BackLinkedPoles.Count; i++)
            {
                var pole = BackLinkedPoles[i];
                if(pole.DestroyedOrNull())
                {
                    i--;
                    BackLinkedPoles.RemoveAt(i);
                }
            }

            // Aprox. once every 4 seconds.
            if (LinkedPoles == null)
                return;

            for (int i = 0; i < LinkedPoles.Count; i++)
            {
                var pole = LinkedPoles[i];
                if (pole.DestroyedOrNull())
                {
                    if (poleToPoints.ContainsKey(LinkedPoles[i]))
                        poleToPoints.Remove(LinkedPoles[i]);
                    LinkedPoles.RemoveAt(i);
                    i--;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (LinkedPoles == null)
                return;

            foreach (var pair in poleToPoints)
            {
                if (pair.Key.DestroyedOrNull())
                    continue;

                var points = pair.Value;
                if (points == null || points.Count < 2)
                    continue;

                float height = AltitudeLayer.Skyfaller.AltitudeFor();
                for (int i = 1; i < points.Count; i++)
                {
                    var last = points[i - 1].FlatToWorld(height);
                    var current = points[i].FlatToWorld(height);

                    GenDraw.DrawLineBetween(last, current, Content.PowerPoleCableMat);
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Command_Target()
            {
                defaultLabel = "RF.Coil.LinkLabel".Translate(),
                defaultDesc = "RF.Coil.LinkDesc".Translate(),
                action = (thing) =>
                {
                    // Try link to this.
                    if (thing is not Building_PowerPole pole)
                        return;

                    LinkTo(pole);

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
                        return info.Thing is Building_PowerPole pole && CanLinkTo(pole);
                    }
                },
                icon = Content.LinkIcon
            };

            if (Prefs.DevMode)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "rebuild cables",
                    action = RegenerateAllPointsAsync
                };
                
            }
        }

        public Vector2 FlatConnectionPoint()
        {
            // nesw are 0123

            Vector2 root = DrawPos.WorldToFlat() + new Vector2(0, 0.5f);

            switch (Rotation.AsInt)
            {
                case 0:
                    return root + new Vector2(0, -0.0578f);
                case 1:
                    return root + new Vector2(0.291f, 0.2f);
                case 2:
                    return root + new Vector2(0, -0.0578f);
                case 3:
                    return root + new Vector2(-0.291f, 0.2f);
            }
            return root;
        }

        public bool IsLinkedTo(Building_PowerPole other)
        {
            if (other == null || other == this || other.Destroyed)
                return false;

            if (LinkedPoles == null)
                return false;

            if (LinkedPoles.Contains(other))
                return true;

            return other.LinkedPoles != null && other.LinkedPoles.Contains(this);
        }

        public bool CanLinkTo(Building_PowerPole other)
        {
            return other != null && other != this && !other.Destroyed && other.Map == this.Map && !IsLinkedTo(other);
        }

        public void LinkTo(Building_PowerPole other, bool genPoints = true)
        {
            // Only check for the basics: CanLinkTo should have already been done before calling this.
            if (other.DestroyedOrNull())
                return;

            LinkedPoles ??= new List<Building_PowerPole>();
            LinkedPoles.Add(other);
            other.BackLinkedPoles.Add(this);

            // Causes power net to be rebuilt.
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.PowerGrid, true, false);

            if (genPoints)
                GeneratePointsAsync(this, other, 50);
        }

        public void Unlink(Building_PowerPole other)
        {
            if (LinkedPoles.Contains(other))
            {
                LinkedPoles.Remove(other);
                poleToPoints.Remove(other);
            }

            if (other.BackLinkedPoles.Contains(this))
                other.BackLinkedPoles.Remove(this);
        }

        public void RegenerateAllPointsAsync()
        {
            poleToPoints.Clear();
            if (LinkedPoles == null)
                return;

            foreach (var pole in LinkedPoles)
            {
                GeneratePointsAsync(this, pole, 50);
            }
        }
    }
}
