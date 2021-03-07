namespace RimForge.Effects
{
    public abstract class ThreadedEffect : MapEffect
    {
        public abstract void Tick(float deltaTime);
    }
}
