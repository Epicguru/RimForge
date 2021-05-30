using Verse;

namespace RimForge
{
    [StaticConstructorOnStartup]
    static class WikiLoader
    {
        static WikiLoader()
        {
            var wiki = InGameWiki.ModWiki.Create(Core.Instance);
            if (wiki == null)
                return;

            wiki.WikiTitle = "RimForge";
            wiki.NoSpoilerMode = false;
        }
    }
}
