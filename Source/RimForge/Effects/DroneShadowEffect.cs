using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public class DroneShadowEffect : MapEffect
    {
        private int waitTicks;
        private int duration;
        private Vector3 from, to;
        private int progress;
        private Vector3 pos;
        private float angle;

        public DroneShadowEffect(Vector3 from, Vector3 to, int duration, int waitToStart)
        {
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.pos = from;
            this.waitTicks = waitToStart;

            angle = (to - from).ToAngleFlat();
        }

        public override void TickAccurate()
        {
            base.TickAccurate();

            pos = Vector3.Lerp(from, to, (float) progress / duration);

            if (waitTicks > 0)
            {
                waitTicks--;
                return;
            }

            progress++;

            if (progress > duration)
                Destroy();
        }

        public override void Draw(bool tick, Map map)
        {
            if(waitTicks <= 0)
                Content.DroneShadowGraphic.DrawWorker(pos, default, null, null, angle);
        }
    }
}
