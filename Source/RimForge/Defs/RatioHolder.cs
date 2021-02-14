using System.Collections.Generic;
using Verse;

namespace RimForge
{
    public class RatioHolder
    {
        public ThingDef resource;
        public int count = 1;

        // Needed for xml serialization. Implicit constructor won't do.
        public RatioHolder()
        {

        }

        public IEnumerable<string> ConfigErrors()
        {
            if (count <= 0)
                yield return $"Invalid ratio count of {count}. Must be at least 1.";

            if (resource == null)
                yield return "Null resource in this ratio holder.";
        }

        public override string ToString()
        {
            return $"{resource?.defName ?? "null"} x{count}";
        }
    }
}
