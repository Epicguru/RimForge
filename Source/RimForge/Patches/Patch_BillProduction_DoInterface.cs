using System;
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
            try
            {
                if (__instance is Bill_Production bp &&
                    __instance.billStack?.billGiver is Building_ForgeRewritten forge && !forge.CanDoBillNow(bp))
                {
                    Rect rect1 = new Rect(x, y, width, 53f);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color = Color.yellow;
                    float reqTemp = bp?.recipe?.TryGetAlloyDef()?.MinTemperature ?? 420f;
                    string text = "RF.Forge.BillNotHot".Translate(reqTemp.ToStringTemperature("F0"));
                    var size = Text.CalcSize(text) + new Vector2(0, -2);
                    Widgets.DrawBoxSolid(new Rect(0, 0, width, size.y).CenteredOnXIn(rect1).CenteredOnYIn(rect1),
                        new Color(0, 0, 0, 0.65f));
                    Widgets.Label(rect1, "<b>" + text + "</b>");
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                }
            }
            catch (Exception e)
            {
                Core.Error("Forge bill exception: ", e);
            }
        }
    }
}
