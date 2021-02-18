using UnityEngine;

namespace RimForge
{
    [Verse.StaticConstructorOnStartup]
    public static class Content
    {
        public static readonly Texture2D SignalIcon, CopyIcon, PasteIcon, LinkIcon;

        static Content()
        {
            SignalIcon = Verse.ContentFinder<Texture2D>.Get("RF/UI/Signal");
            CopyIcon   = Verse.ContentFinder<Texture2D>.Get("RF/UI/Copy");
            PasteIcon  = Verse.ContentFinder<Texture2D>.Get("RF/UI/Paste");
            LinkIcon   = Verse.ContentFinder<Texture2D>.Get("RF/UI/Link");
        }
    }
}
