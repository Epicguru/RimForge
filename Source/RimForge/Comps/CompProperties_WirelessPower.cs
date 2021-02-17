using Verse;

namespace RimForge.Comps
{
    public class CompProperties_WirelessPower : CompProperties
    {
        public string buildingName = "pylon";
        public bool canSendPower = false;
        public int? fixedPowerLevel = null;
        public int maxPower = 999999;

        public CompProperties_WirelessPower()
        {
            compClass = typeof(CompWirelessPower);
        }
    }
}
