using Verse;

namespace RimForge
{
    public class Settings : ModSettings
    {
        // TESLA COILS
        [TweakValue("RimForge", 1, 500)]
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

        // COILGUN
        [TweakValue("RimForge", -1, 1000)]
        public static int CoilgunMaxPen = -1;
        [TweakValue("RimForge", 0, 1)]
        public static float CoilgunPenDamageMultiplier = 0.95f;
        [TweakValue("RimForge", 0, 10000)]
        public static int CoilgunBaseDamage = 800;
    }
}
