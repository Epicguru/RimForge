using RimForge.Comps;
using RimForge.Effects;
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

        private LinearElectricArc arc;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            Wireless = GetComp<CompWirelessPower>();
            if (arc != null)
                return;

            arc = new LinearElectricArc(10);
            arc.Start = new Vector2(DrawPos.x, DrawPos.z);
            arc.End = new Vector2(DrawPos.x + 5, DrawPos.z);
            arc.Spawn(base.Map);
        }

        public override void Tick()
        {
            base.Tick();

            if (arc != null)
            {
                arc.Start = DrawPos.WorldToFlat();
                arc.End = DrawPos.WorldToFlat() + new Vector2(5, 2);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            if (arc == null)
                return;

            arc.Destroy();
            arc = null;
        }
    }
}
