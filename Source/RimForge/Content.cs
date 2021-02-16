using UnityEngine;

namespace RimForge
{
    [Verse.StaticConstructorOnStartup]
    public static class Content
    {
        public static readonly Texture2D SignalIcon;

        static Content()
        {
            SignalIcon = Verse.ContentFinder<Texture2D>.Get("RF/UI/Signal");
        }
    }
}
