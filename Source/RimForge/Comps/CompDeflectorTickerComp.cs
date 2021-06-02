using System.Collections.Generic;
using Verse;

namespace RimForge.Comps
{
    public class CompDeflectorTickerComp : GameComponent
    {
        public static CompDeflectorTickerComp Current => Verse.Current.Game?.GetComponent<CompDeflectorTickerComp>();

        [DebugAction("RimForge", "Log deflector comps", actionType = DebugActionType.Action)]
        public static void LogDeflectorComps()
        {
            foreach (var item in Current.comps)
            {
                Core.Log($"{item.parent.LabelCap} @ {item.parent.Position} on {item.parent.Map}");
            }
        }

        private List<CompDeflector> comps = new List<CompDeflector>();

        public CompDeflectorTickerComp() { }
        public CompDeflectorTickerComp(Game _) { }

        public void Add(CompDeflector deflector)
        {
            if(!comps.Contains(deflector))
                comps.Add(deflector);
        }

        public void Remove(CompDeflector deflector)
        {
            if(comps.Contains(deflector))
                comps.Remove(deflector);
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            for (int i = 0; i < comps.Count; i++)
            {
                var comp = comps[i];
                if (comp.parent == null || comp.parent.Destroyed)
                {
                    Core.Warn("Forcefully removed a CompDeflector, perhaps map was destroyed unexpectedly.");
                    comps.RemoveAt(i);
                    i--;
                    continue;
                }

                comp.TickCustom();
            }
        }
    }
}
