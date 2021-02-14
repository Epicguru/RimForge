using Verse;

namespace RimForge
{
    /// <summary>
    /// A DefModExtension intended to be put on resources (metals, mainly).
    /// Allows mod-makers to add RimForge properties to their own metals.
    /// For example, melting point. Also allows for an 'equivalent' material
    /// to be defined. For example, if a mod adds copper, then it can be marked as 'equivalent' to
    /// RimForge copper.
    /// </summary>
    public class Extension : DefModExtension
    {
        public ThingDef equivalentTo;
        public float? meltingPoint;
    }
}
