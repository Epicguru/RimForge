using HarmonyLib;
using System;
using Verse;

namespace RimForge.Disco
{
    [HarmonyPatch(typeof(TickManager), "set_CurTimeSpeed")]
    static class Patch_TickManager_TogglePaused
    {
        public static event Action<bool> OnPauseChange;

        private static bool wasLastPaused = false;

        static void Postfix()
        {
            bool isPaused = Find.TickManager.Paused;

            if (isPaused != wasLastPaused)
            {
                wasLastPaused = isPaused;
                OnPauseChange?.Invoke(isPaused);
            }
        }
    }
}
