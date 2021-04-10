using UnityEngine;
using Verse;

namespace RimForge
{
    [StaticConstructorOnStartup]
    public static class Content
    {
        public static readonly Texture2D SignalIcon, CopyIcon, PasteIcon, LinkIcon, RitualStartIcon;
        public static readonly Texture2D BuildBlueprintIcon, RitualGearTexture;

        public static Graphic ForgeIdle, ForgeGlowAll, ForgeGlowSides;
        public static Graphic HEFueledIdle, HEFueledGlow;
        public static Graphic HEPoweredIdle, HEPoweredPowerOn, HEPoweredGlow;
        public static Graphic ForgeMetalOut, ForgeMetalLeft, ForgeMetalMiddle, ForgeMetalRight;

        public static Graphic CoilgunTop, CoilgunTopGlow;
        public static Graphic CoilgunCables, CoilgunCablesGlow;
        public static Graphic CoilgunBarLeft, CoilgunBarRight, CoilgunLinkLeft, CoilgunLinkRight;
        public static Graphic CoilgunBeam;

        public static Graphic RitualCircle, RitualCircleText, RitualGear, RitualBall;
        public static Graphic RitualSymbolA, RitualSymbolB;
        public static Graphic DiscoFloorGlowGraphic;

        static Content()
        {
            SignalIcon         = ContentFinder<Texture2D>.Get("RF/UI/Signal");
            CopyIcon           = ContentFinder<Texture2D>.Get("RF/UI/Copy");
            PasteIcon          = ContentFinder<Texture2D>.Get("RF/UI/Paste");
            LinkIcon           = ContentFinder<Texture2D>.Get("RF/UI/Link");
            RitualStartIcon    = ContentFinder<Texture2D>.Get("RF/UI/RitualStart");
            BuildBlueprintIcon = ContentFinder<Texture2D>.Get("RF/UI/BuildBlueprint");
            RitualGearTexture  = ContentFinder<Texture2D>.Get("RF/Effects/RitualGear");
        }

        internal static void LoadForgeTextures(Building forge)
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

        internal static void LoadBuildingGraphics(Building gun)
        {
            var gd = gun.DefaultGraphic.data;
            Graphic Make(string path, Vector2 size)
            {
                return GraphicDatabase.Get(gd.graphicClass, path, gd.shaderType.Shader, size, Color.white, Color.white, gd, gd.shaderParameters);
            }
            Graphic MakeUnlit(string path, Vector2 size)
            {
                return GraphicDatabase.Get(gd.graphicClass, path, RFDefOf.TransparentPostLight.Shader, size, Color.red, Color.white, gd, gd.shaderParameters);
            }

            CoilgunTop = Make("RF/Buildings/Coilgun/Top", new Vector2(8.886f, 3.233f));
            CoilgunTopGlow = Make("RF/Buildings/Coilgun/TopGlow", new Vector2(8.886f, 3.233f));
            CoilgunCables = Make("RF/Buildings/Coilgun/Cables", new Vector2(1.702f, 1.900f));
            CoilgunCablesGlow = Make("RF/Buildings/Coilgun/CablesGlow", new Vector2(1.702f, 1.900f));

            CoilgunBarLeft = Make("RF/Buildings/Coilgun/BarLeft", new Vector2(2.283f, 0.970f));
            CoilgunBarRight = Make("RF/Buildings/Coilgun/BarRight", new Vector2(2.283f, 0.970f));
            CoilgunLinkLeft = Make("RF/Buildings/Coilgun/PivotLeft", new Vector2(2.064f, 0.697f));
            CoilgunLinkRight = Make("RF/Buildings/Coilgun/PivotRight", new Vector2(2.064f, 0.697f));

            Patches.Patch_MaterialPool_MatFrom.Active = true;
            CoilgunBeam = MakeUnlit("RF/Buildings/Coilgun/Beam", new Vector2(1000, 0.403f));
            Patches.Patch_MaterialPool_MatFrom.Active = false;
        }

        internal static void LoadRitualGraphics(Building building)
        {
            var gd = building.DefaultGraphic.data;
            Graphic MakeUnlit(string path, Vector2 size)
            {
                return GraphicDatabase.Get(gd.graphicClass, path, RFDefOf.TransparentPostLight.Shader, size, Color.white, Color.white, gd, gd.shaderParameters);
            }

            RitualCircle = MakeUnlit("RF/Effects/RitualCircle", new Vector2(21, 21));
            RitualCircleText = MakeUnlit("RF/Effects/RitualCircleText", new Vector2(10, 10));
            RitualGear = MakeUnlit("RF/Effects/RitualGear", new Vector2(14, 14));
            RitualBall = MakeUnlit("RF/Effects/RitualBall", new Vector2(1, 1));

            RitualSymbolA = MakeUnlit("RF/Effects/RitualSymbolA", new Vector2(1, 1));
            RitualSymbolB = MakeUnlit("RF/Effects/RitualSymbolB", new Vector2(1, 1));
        }

        internal static void LoadFueledHeatingElementGraphics(Building b)
        {
            var gd = b.DefaultGraphic.data;
            Graphic Make(string path, bool unlit = false)
            {
                return GraphicDatabase.Get(gd.graphicClass, path, unlit ? RFDefOf.TransparentPostLight.Shader : gd.shaderType.Shader, gd.drawSize, Color.white, Color.white, gd, gd.shaderParameters);
            }

            HEFueledIdle = Make("RF/Buildings/HeatingElement_FueledIdle");
            HEFueledGlow = Make("RF/Buildings/HeatingElement_FueledGlow");
        }

        internal static void LoadPoweredHeatingElementGraphics(Building b)
        {
            var gd = b.DefaultGraphic.data;
            Graphic Make(string path, bool unlit = false)
            {
                return GraphicDatabase.Get(gd.graphicClass, path, unlit ? RFDefOf.TransparentPostLight.Shader : gd.shaderType.Shader, gd.drawSize, Color.white, Color.white, gd, gd.shaderParameters);
            }

            HEPoweredIdle = Make("RF/Buildings/HeatingElement_PoweredIdle");
            HEPoweredPowerOn = Make("RF/Buildings/HeatingElement_PoweredPowerOn");
            HEPoweredGlow = Make("RF/Buildings/HeatingElement_PoweredGlow");
        }
    }
}
