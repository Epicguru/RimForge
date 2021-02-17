using UnityEngine;

namespace RimForge
{
    [Verse.StaticConstructorOnStartup]
    public static class Content
    {
        public static readonly Texture2D SignalIcon, CopyIcon, PasteIcon;

        static Content()
        {
            SignalIcon = Verse.ContentFinder<Texture2D>.Get("RF/UI/Signal");
            CopyIcon   = Verse.ContentFinder<Texture2D>.Get("RF/UI/Copy");
            PasteIcon  = Verse.ContentFinder<Texture2D>.Get("RF/UI/Paste");
        }
    }
}
