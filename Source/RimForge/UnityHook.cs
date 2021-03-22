using System;
using RimForge.Disco;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class UnityHook : MonoBehaviour
    {
        [TweakValue("RimForge", 0, 1)]
        public static bool DrawDebugGUI = false;

        public static event Action UponApplicationQuit;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnApplicationQuit()
        {
            Core.Log("Detected application quit...");
            UponApplicationQuit?.Invoke();
        }

        private void OnGUI()
        {
            if (!DrawDebugGUI)
                return;

            DrawDiscoDebug();
        }

        private void DrawDiscoDebug()
        {
            if (Current.ProgramState != ProgramState.Playing)
                return;

            var dt = Find.CurrentMap?.GetComponent<DiscoTracker>();
            if (dt == null)
                return;

            foreach (var stand in dt.GetAllValidDJStands())
            {
                stand.DebugOnGUI();
            }
        }
    }
}
