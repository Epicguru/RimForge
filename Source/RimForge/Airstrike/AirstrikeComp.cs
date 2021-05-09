using System.Collections.Generic;
using Verse;

namespace RimForge.Airstrike
{
    public class AirstrikeComp : MapComponent
    {
        private List<AirstrikeInstance> airstrikes = new List<AirstrikeInstance>();

        public AirstrikeComp(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref airstrikes, "RF_Airstrikes", LookMode.Deep);
            airstrikes ??= new List<AirstrikeInstance>();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            for (int i = 0; i < airstrikes.Count; i++)
            {
                var item = airstrikes[i];
                item.Tick(this);

                if (item.IsDone)
                {
                    airstrikes.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
