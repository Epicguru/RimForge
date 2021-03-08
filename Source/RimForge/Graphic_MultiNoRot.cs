using UnityEngine;
using Verse;

namespace RimForge
{
    public class Graphic_MultiNoRot : Graphic_Multi
    {
        public override Mesh MeshAt(Rot4 rot)
        {
            return MeshPool.GridPlane(data.drawSize);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Mesh mesh = this.MeshAt(rot);
            Quaternion quat = Quaternion.identity; // This line has been changed.
            if ((double)extraRotation != 0.0)
                quat *= Quaternion.Euler(Vector3.up * extraRotation);
            loc += this.DrawOffset(rot);
            Material mat = this.MatAt(rot, thing);
            this.DrawMeshInt(mesh, loc, quat, mat);
            if (this.ShadowGraphic == null)
                return;
            this.ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }
    }
}
