using HarmonyLib;
using RimForge.Comps;
using UnityEngine;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    static class Patch_PawnRenderer_DrawEquipmentAiming_Oversized
    {
        [HarmonyPriority(Priority.First - 1)]
        static bool Prefix(Pawn ___pawn, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            var comp = eq.TryGetCompOversizedWeapon();
            if (comp == null)
                return true;

            // Ultimately I think it is better to do this than try to use a transpiler.
            // In any solution I will have to check for the comp (which adds the overhead)
            // and beyond that there is no advantage of using a transpiler vs replacing the original
            // like this.

			float num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material matSingle;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }

            // CHANGED
            Vector3 scale = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            var curOffset = comp.Props != null ? OffsetFromRotation(___pawn.Rotation, comp.Props) : Vector3.zero;
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(drawLoc + curOffset, Quaternion.AngleAxis(num, Vector3.up), scale), matSingle, 0);
            return false;
		}

        private static Vector3 OffsetFromRotation(Rot4 rotation, CompProperties_OversizedWeapon props)
        {
            if (rotation == Rot4.North)
                return props.northOffset;
            if (rotation == Rot4.East)
                return props.eastOffset;
            if (rotation == Rot4.West)
                return props.westOffset;
            return props.southOffset;
        }
    }
}
