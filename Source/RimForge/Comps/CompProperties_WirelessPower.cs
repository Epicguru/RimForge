using Verse;

namespace RimForge.Comps
{
    public class CompProperties_WirelessPower : CompProperties
    {
        public string buildingName = "pylon";
        public bool canSendPower = false;
        public int? fixedPowerLevel = null;

        public CompProperties_WirelessPower()
        {
            compClass = typeof(CompWirelessPower);
        }
    }
}
