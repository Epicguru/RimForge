using RimForge.Buildings;
using UnityEngine;
using Verse;

namespace RimForge
{
    [StaticConstructorOnStartup]
    public static class Content
    {
        public static readonly Texture2D SignalIcon, CopyIcon, PasteIcon, LinkIcon;
        public static Graphic ForgeIdle, ForgeGlowAll, ForgeGlowSides;
        public static Graphic ForgeMetalOut, ForgeMetalLeft, ForgeMetalMiddle, ForgeMetalRight;

        static Content()
        {
            SignalIcon = ContentFinder<Texture2D>.Get("RF/UI/Signal");
            CopyIcon   = ContentFinder<Texture2D>.Get("RF/UI/Copy");
            PasteIcon  = ContentFinder<Texture2D>.Get("RF/UI/Paste");
            LinkIcon   = ContentFinder<Texture2D>.Get("RF/UI/Link");
        }

        internal static void LoadForgeTextures(Building_Forge forge)
        {
            var gd = forge.DefaultGraphic.data;
            Graphic Make(string path, bool unlit = false)
            {
                return GraphicDatabase.Get(gd.graphicClass, path, unlit ? RFDefOf.TransparentPostLight.Shader : gd.shaderType.Shader, gd.drawSize, Color.white, Color.white, gd, gd.shaderParameters);
            }

            ForgeIdle = Make("RF/Buildings/ForgeIdle");
            ForgeGlowAll = Make("RF/Buildings/ForgeGlowAll");
            ForgeGlowSides = Make("RF/Buildings/ForgeGlowSides");

            // This patch changes the texture repeat mode to Clamp.
            Patches.Patch_MaterialPool_MatFrom.Active = true;

            ForgeMetalOut = Make("RF/Buildings/ForgeMetalOut", true);
            ForgeMetalLeft = Make("RF/Buildings/ForgeMetalLeft", true);
            ForgeMetalMiddle = Make("RF/Buildings/ForgeMetalMiddle", true);
            ForgeMetalRight = Make("RF/Buildings/ForgeMetalRight", true);

            Patches.Patch_MaterialPool_MatFrom.Active = false;
        }
    }
}
