using System;
using HarmonyLib;
using Verse;

namespace RimForge
{
    public class Core : Mod
    {
        internal static void Log(string msg)
        {
            Verse.Log.Message($"<color=#b7ff1c>[RimForge]</color> {msg ?? "<null>"}");
        }

        internal static void Error(string msg, Exception exception = null)
        {
            Verse.Log.Error($"[RimForge] {msg ?? "<null>"}");
            if(exception != null)
                Verse.Log.Error(exception.ToString());
        }

        public Core(ModContentPack content) : base(content)
        {
            Log("Hello, world!");

            // Apply harmony patches.
            var harmony = new Harmony("co.uk.epicguru.rimforge");
            try
            {
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Error("Failed to apply 1 or more harmony patches! Mod will not work as intended. Contact author.", e);
            }
            finally
            {
                Log($"Patched {harmony.GetPatchedMethods().EnumerableCount()} methods.");
            }
        }
    }
}
