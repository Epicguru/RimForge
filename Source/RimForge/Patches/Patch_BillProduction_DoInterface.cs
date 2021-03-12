using HarmonyLib;
using RimForge.Buildings;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(Bill), "DoInterface")]
    static class Patch_BillProduction_DoInterface
    {
        static void Postfix(Bill __instance, float x, float y, float width)
        {
            if (__instance is Bill_Production bp && __instance.billStack?.billGiver is Building_ForgeRewritten forge && !forge.CanDoBillNow(bp))
            {
                Rect rect1 = new Rect(x, y, width, 53f);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.yellow;
                string text = "RF.Forge.BillNotHot".Translate();
                var size = Text.CalcSize(text) + new Vector2(10, 10);
                Widgets.DrawBoxSolid(new Rect(0, 0, size.x, size.y).CenteredOnXIn(rect1).CenteredOnYIn(rect1), Color.grey);
                Widgets.Label(rect1, "<b>" + text + "</b>");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
            
        }
    }
}
