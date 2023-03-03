using RimForge.Effects;
using System;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class UnityHook : MonoBehaviour
    {
        public static event Action<bool> OnPauseChange;

        public static event Action UponApplicationQuit;
        private bool lastPaused;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            if (Current.Game == null)
                return;

            ThreadedEffectHandler.TickRate = Find.TickManager.TickRateMultiplier;

            bool currentPaused = Find.TickManager?.Paused ?? false;
            if (lastPaused != currentPaused)
            {
                OnPauseChange?.Invoke(currentPaused);
                lastPaused = currentPaused;
            }
        }

        private void OnApplicationQuit()
        {
            Core.Log("Detected application quit...");
            UponApplicationQuit?.Invoke();
        }
    }
}
