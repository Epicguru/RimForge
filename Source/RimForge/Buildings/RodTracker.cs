using System.Collections.Generic;
using Verse;

namespace RimForge.Buildings
{
    public class RodTracker : MapComponent
    {
        public IReadOnlyList<Building_LightningRod> RodsReadOnly
        {
            get
            {
                SanitizeLists();
                return rods;
            }
        }

        private List<Building_LightningRod> rods = new List<Building_LightningRod>();

        public RodTracker(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            SanitizeLists();
            Scribe_Collections.Look(ref rods, "rods", LookMode.Reference);
            SanitizeLists();
        }

        private void SanitizeLists()
        {
            rods ??= new List<Building_LightningRod>();
            rods.RemoveAll(rod => rod.DestroyedOrNull());
        }

        public void Register(Building_LightningRod rod)
        {
            SanitizeLists();
            if (rod.DestroyedOrNull() || rods.Contains(rod))
                return;

            rods.Add(rod);
        }

        public void UnRegister(Building_LightningRod rod)
        {
            SanitizeLists();
            if (rod == null || !rods.Contains(rod))
                return;

            rods.Remove(rod);
        }
    }
}
