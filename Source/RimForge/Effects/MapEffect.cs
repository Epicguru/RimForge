using Verse;

namespace RimForge.Effects
{
    public abstract class MapEffect
    {
        public int MapId { get; internal set; } = -1;
        public bool Destroyed { get; protected set; }

        public void Spawn(Map map)
        {
            if (Destroyed)
            {
                Core.Error("Already destroyed, don't try to re-spawn.");
                return;
            }
            if (MapId != -1)
            {
                Core.Warn("Already spawned effect");
                return;
            }

            MapEffectHandler.Current.RegisterEffect(this, map); // Checks for map null inside.
        }

        public void Destroy()
        {
            this.Destroyed = true;
        }

        public virtual bool ShouldDeSpawn()
        {
            return Destroyed || MapId == -1;
        }

        public virtual void TickAccurate()
        {

        }

        public abstract void Draw(bool tick, Map map);
    }
}
