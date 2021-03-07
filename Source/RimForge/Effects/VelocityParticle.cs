using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public abstract class VelocityParticle : ThreadedEffect
    {
        public virtual Vector3 DrawPos => new Vector3(Position.x, Layer.AltitudeFor(), Position.y);

        public AltitudeLayer Layer = AltitudeLayer.VisEffects;
        public Vector2 Position;
        public Vector2 Velocity;

        public override void Tick(float deltaTime)
        {
            // High tier physics.
            Position += Velocity * deltaTime;
        }

        public sealed override void Draw(bool tick, Map map)
        {
            Draw(map);
        }

        public abstract void Draw(Map map);
    }
}
