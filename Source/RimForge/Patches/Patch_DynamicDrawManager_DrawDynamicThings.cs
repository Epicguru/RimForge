using HarmonyLib;
using System;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(DynamicDrawManager), "DrawDynamicThings")]
    static class Patch_DynamicDrawManager_DrawDynamicThings
    {
        private static Action<Map> onLateDraw;
        private static bool anyListener = false;

        public static void TryRegisterListener(Action<Map> method)
        {
            if (anyListener)
            {
                Core.Error("There is already a listener.");
                return;
            }

            anyListener = true;
            onLateDraw = method;
            Core.Log("Registered map late draw listener.");
        }

        static void Postfix(Map ___map)
        {
            try
            {
                onLateDraw?.Invoke(___map);
            }
            catch (Exception e)
            {
                Core.Error("Exception in map late draw patch!", e);
            }
        }
    }
}
