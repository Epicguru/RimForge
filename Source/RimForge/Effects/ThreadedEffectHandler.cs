using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Verse;

namespace RimForge.Effects
{
    public class ThreadedEffectHandler
    {
        public  const double TARGET_UPS = 60;
        private const double FRAME_TIME = 1.0 / TARGET_UPS;
        private const double FRAME_TIME_MS = 1000.0 * FRAME_TIME;
        private const    int FRAME_TIME_MS_INT = (int)FRAME_TIME_MS;

        [TweakValue("RimForge")]
        public static bool RemoveBadTickParticles = true;
        [TweakValue("RimForge", 0, (float)FRAME_TIME_MS)]
        public static float InfoParticleTickTimeMs = 0f;

        public static float TickRate = 1f;

        public bool IsRunning { get; private set; }

        private readonly List<ThreadedEffect> effects = new List<ThreadedEffect>();
        private Thread thread;

        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            thread = new Thread(Run)
            {
                Name = "RimForge Particle Thread"
            };
            thread.Start();

            effects.Clear();
            Core.Log("Started particle processing thread.");
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            thread = null;
            effects.Clear();
            Core.Log("Shutting down particle processing thread...");
        }

        public void AddEffect(ThreadedEffect effect)
        {
            if (effect == null)
                return;

            // It shouldn't really matter if this is done while ticking is happening.
            effects.Add(effect);
        }

        private void Run()
        {
            var watch = new Stopwatch();

            while (IsRunning)
            {
                // If the game is paused, not focused whatever then don't bother ticking particles.
                if (TickRate <= 0f)
                {
                    Thread.Sleep(FRAME_TIME_MS_INT);
                    continue;
                }

                watch.Restart();
                try
                {
                    TickAll((float)(FRAME_TIME * TickRate));
                }
                catch(Exception e)
                {
                    Core.Error("Exception ticking threaded particles (outer loop)", e);
                }
                watch.Stop();
                double took = watch.Elapsed.TotalSeconds;
                InfoParticleTickTimeMs = (float)watch.Elapsed.TotalMilliseconds;
                double toWait = FRAME_TIME - took;

                // Will round down.
                // This is fine since Thread.Sleep sometimes waits a little longer than expected especially on lower priority threads.
                int toWaitMs = (int) (toWait * 1000.0);
                if (toWaitMs > 0)
                    Thread.Sleep(toWaitMs);
            }

            Core.Log("Stopped particle thread.");
        }

        private void TickAll(float deltaTime)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null || effect.ShouldDeSpawn())
                {
                    effects.RemoveAt(i);
                    i--;
                    continue;
                }

                try
                {
                    effect.Tick(deltaTime);
                }
                catch (Exception e)
                {
                    // Only log errors in dev mode. Logging errors is slow and may not even work from another thread (idk).
                    // Just in case, avoid messing up the average player's game and log by not logging particle tick errors.
                    if (Prefs.DevMode)
                        Core.Error("Exception ticking threaded particle.", e);

                    if (RemoveBadTickParticles)
                    {
                        effects.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
