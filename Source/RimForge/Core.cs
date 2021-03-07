using System;
using HarmonyLib;
using RimForge.Effects;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class Core : Mod
    {
        public static ModContentPack ContentPack { get; private set; }

        internal static void Log(string msg)
        {
            Verse.Log.Message($"<color=#b7ff1c>[RimForge]</color> {msg ?? "<null>"}");
        }

        internal static void Warn(string msg)
        {
            Verse.Log.Warning($"[RimForge] {msg ?? "<null>"}");
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
            ContentPack = content;

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

            // Create MonoBehaviour hook.
            var go = new GameObject("RimForge hook");
            go.AddComponent<UnityHook>();
            Log("Created Unity hook game object.");

            // When the game closes, shut down this particle processing thread.
            UnityHook.UponApplicationQuit += () =>
            {
                MapEffectHandler.ThreadedHandler?.Stop();
            };
        }
    }
}
