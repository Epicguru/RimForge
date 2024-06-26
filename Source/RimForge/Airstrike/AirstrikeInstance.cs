﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace RimForge.Airstrike
{
    public class AirstrikeInstance : IExposable
    {
        public bool IsDone;
        public Thing Instigator;
        public Map Map;
        public int DelayTicks;
        public SoundDef SoundAfterDelay;

        private List<SingleStrike> strikes = new List<SingleStrike>();
        private int tick;

        public AirstrikeInstance() { }

        public AirstrikeInstance(IEnumerable<SingleStrike> strikes)
        {
            this.strikes.AddRange(strikes);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref strikes, "strikePositions", LookMode.Deep);
            Scribe_Values.Look(ref tick, "tick");
            Scribe_Values.Look(ref IsDone, "isDone");
            Scribe_References.Look(ref Instigator, "instigator");
            Scribe_Defs.Look(ref SoundAfterDelay, "sound");
            Scribe_Values.Look(ref DelayTicks, "delayTicks");

            strikes ??= new List<SingleStrike>();
        }

        public void Tick(AirstrikeComp comp)
        {
            if (IsDone)
                return;

            this.Map = comp.map;

            tick++;
            if (tick == DelayTicks)
            {
                SoundAfterDelay?.PlayOneShotOnCamera(Map);
            }

            foreach (var item in strikes)
            {
                try
                {
                    item.Tick(this, tick);
                }
                catch(Exception e)
                {
                    Core.Error("SingleStrike.Tick() exception:", e);
                }
            }

            var next = strikes[0];
            if (next.IsDone)
            {
                strikes.RemoveAt(0);
                if (strikes.Count == 0)
                    IsDone = true;
            }
        }
    }
}
