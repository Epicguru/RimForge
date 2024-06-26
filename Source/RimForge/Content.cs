﻿using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge
{
    [StaticConstructorOnStartup]
    public static class Content
    {
        public static readonly Texture2D SignalIcon, CopyIcon, PasteIcon, LinkIcon, RitualStartIcon;
        public static readonly Texture2D BuildBlueprintIcon, RitualGearTexture, CapacitorCharge;
        public static readonly Texture2D ArrowIcon, CoilgunShootIcon, MissilesIcon, DeflectIcon;

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
        public static Graphic[] GreenhouseActiveFrames;
        public static Graphic DroneShadowGraphic, FallingBombGraphic, BombShadowGraphic;
        public static Graphic DroneNorth, DroneEast, DroneSouth, DroneWest;

        static Content()
        {
            SignalIcon         = ContentFinder<Texture2D>.Get("RF/UI/Signal");
            ArrowIcon          = ContentFinder<Texture2D>.Get("RF/UI/Arrow");
            CopyIcon           = ContentFinder<Texture2D>.Get("RF/UI/Copy");
            PasteIcon          = ContentFinder<Texture2D>.Get("RF/UI/Paste");
            LinkIcon           = ContentFinder<Texture2D>.Get("RF/UI/Link");
            RitualStartIcon    = ContentFinder<Texture2D>.Get("RF/UI/RitualStart");
            BuildBlueprintIcon = ContentFinder<Texture2D>.Get("RF/UI/BuildBlueprint");
            CoilgunShootIcon   = ContentFinder<Texture2D>.Get("RF/UI/CoilgunFire");
            MissilesIcon       = ContentFinder<Texture2D>.Get("RF/UI/Missiles");
            DeflectIcon        = ContentFinder<Texture2D>.Get("RF/UI/Deflect");
            RitualGearTexture  = ContentFinder<Texture2D>.Get("RF/Effects/RitualGear");
            CapacitorCharge    = ContentFinder<Texture2D>.Get("RF/Effects/CapacitorCharge");

            const float DRONE_RATIO = 343f / 214f;
            const float DRONE_SCALE = 5f;
            DroneShadowGraphic = GraphicDatabase.Get(typeof(Graphic_Single), "RF/Other/DroneShadowSoft", ShaderTypeDefOf.Transparent.Shader, new Vector2(DRONE_SCALE, DRONE_SCALE * DRONE_RATIO), new Color(1, 1, 1, 0.5f), Color.white);
            BombShadowGraphic = GraphicDatabase.Get(typeof(Graphic_Single), "RF/Other/BombShadow", ShaderTypeDefOf.Transparent.Shader, new Vector2(1, 1), new Color(1, 1, 1, 1), Color.white);
            FallingBombGraphic = GraphicDatabase.Get(typeof(Graphic_Single), "RF/Other/FallingBomb", ShaderTypeDefOf.Transparent.Shader, new Vector2(1, 1), new Color(1, 1, 1, 1), Color.white);

            const float SCALE = 3.5f;
            float ratio = 867f / 597f;
            DroneEast = GraphicDatabase.Get(typeof(Graphic_Single), "RF/Other/DroneEast", ShaderTypeDefOf.Cutout.Shader, new Vector2(SCALE, SCALE * ratio), new Color(1, 1, 1, 1), Color.white);
            DroneWest = GraphicDatabase.Get(typeof(Graphic_Single), "RF/Other/DroneWest", ShaderTypeDefOf.Cutout.Shader, new Vector2(SCALE, SCALE * ratio), new Color(1, 1, 1, 1), Color.white);
            ratio = 986f / 536f;
            DroneNorth = GraphicDatabase.Get(typeof(Graphic_Single), "RF/Other/DroneNorth", ShaderTypeDefOf.Cutout.Shader, new Vector2(SCALE * ratio, SCALE), new Color(1, 1, 1, 1), Color.white);
            ratio = 986f / 584f;
            DroneSouth = GraphicDatabase.Get(typeof(Graphic_Single), "RF/Other/DroneSouth", ShaderTypeDefOf.Cutout.Shader, new Vector2(SCALE * ratio, SCALE), new Color(1, 1, 1, 1), Color.white);
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

        internal static void LoadGreenhouseFrames(Building greenhouse)
        {
            var gd = greenhouse.DefaultGraphic.data;
            Graphic Make(string path)
            {
                return GraphicDatabase.Get(gd.graphicClass, path, gd.shaderType.Shader, Vector2.one, Color.white, Color.white, gd, gd.shaderParameters);
            }

            GreenhouseActiveFrames = new Graphic[15];
            for (int i = 0; i < GreenhouseActiveFrames.Length; i++)
            {
                int frameNum = i * 2;
                string name = frameNum.ToString().PadLeft(4, '0');
                string path = $"RF/Buildings/Greenhouse/{name}";
                GreenhouseActiveFrames[i] = Make(path);
            }
        }
    }
}
