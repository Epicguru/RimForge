using Verse;

namespace RimForge
{
    public static class AlloyHelper
    {
        public const float DEFAULT_MELTING_POINT = 800;

        /// <summary>
        /// Gets the melting point of this resource.
        /// Only intended to be used for things that could logically be melted
        /// down in a forge, such as metals or alloys.
        /// Unless a custom value has been defined using the <see cref="Extension"/>,
        /// this will return the default value <see cref="DEFAULT_MELTING_POINT"/>.
        /// </summary>
        /// <param name="def">The ThingDef to get the melting point for.</param>
        /// <returns>The melting point, measured in degrees celsius.</returns>
        public static float GetMeltingPoint(this ThingDef def)
        {
            var extension = def?.GetModExtension<Extension>();
            if (extension != null)
                return extension.meltingPoint ?? DEFAULT_MELTING_POINT;

            return DEFAULT_MELTING_POINT;
        }
    }
}
