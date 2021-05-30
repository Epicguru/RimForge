using Verse;

namespace RimForge.Buildings
{
    public interface ICustomTargetingUser
    {
        void OnStartTargeting(int index);
        void OnStopTargeting(int index);
        void SetTargetInfo(LocalTargetInfo info, int index);
    }
}
