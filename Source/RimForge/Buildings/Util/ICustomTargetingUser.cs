using Verse;

namespace RimForge.Buildings
{
    public interface ICustomTargetingUser
    {
        void OnStartTargeting();
        void OnStopTargeting();
        void SetTargetInfo(LocalTargetInfo info);
    }
}
