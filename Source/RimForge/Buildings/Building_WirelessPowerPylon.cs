using RimForge.Comps;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    [StaticConstructorOnStartup] // Just to get rimworld to shut up about it.
    public class Building_WirelessPowerPylon : Building
    {
        private static Graphic activeGraphic, idleGraphic;

        private static void LoadGraphics(Thing thing)
        {
            var gd = thing.DefaultGraphic.data;
            activeGraphic = GraphicDatabase.Get(gd.graphicClass, "RF/Buildings/PylonActive", gd.shaderType.Shader, gd.drawSize, Color.white, Color.white, gd, gd.shaderParameters);
            idleGraphic = GraphicDatabase.Get(gd.graphicClass, "RF/Buildings/PylonIdle", gd.shaderType.Shader, gd.drawSize, Color.white, Color.white, gd, gd.shaderParameters);
        }

        public override Graphic Graphic
        {
            get
            {
                if (activeGraphic == null)
                    LoadGraphics(this);
                return IsActive ? activeGraphic : idleGraphic;
            }
        }

        public CompWirelessPower Wireless;
        public bool IsActive => Wireless?.IsActive ?? false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            Wireless = GetComp<CompWirelessPower>();
        }
    }
}
