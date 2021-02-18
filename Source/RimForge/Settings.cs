using Verse;

namespace RimForge
{
    public class Settings : ModSettings
    {
        public static int TeslaMaxDistance = 25;
        [TweakValue("RimForge", 1, 30)]
        public static int TeslaTickInterval = 4;
        [TweakValue("RimForge", 0, 10000)]
        public static int TeslaCooldown = 900;
        [TweakValue("RimForge", 0, 1000)]
        public static int TeslaStunDuration = 450;
        [TweakValue("RimForge", 0, 500)]
        public static float TeslaDamage = 10;
        [TweakValue("RimForge", 0, 50)]
        public static float TeslaMechanoidDamageMulti = 5;
    }
}
