using Verse;

namespace RimForge
{
    public class Settings : ModSettings
    {
        // Power poles and similar
        [TweakValue("RimForge", 0, 10)]
        public static int CableSegmentsPerCell = 3;

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
        [TweakValue("RimForge", 0, 1)]
        public static float CoilgunPenDamageMultiplier = 1f;
        [TweakValue("RimForge", 0, 10)]
        public static float CoilgunBaseDamageMultiplier = 1f;
        [TweakValue("RimForge", 0, 5)]
        public static float CoilgunBuildingDamageMulti = 1f;
        [TweakValue("RimForge", 0, 10)]
        public static float CoilgunBasePowerReq = 1;
        [TweakValue("RimForge", 0, 1)]
        public static bool CoilgunSplatterBlood = true;
        [TweakValue("RimForge", 0, 1)]
        public static bool CoilgunDisplayDamageReport = true;

        // RITUAL
        [TweakValue("RimForge", 0f, 1f)]
        public static float RitualFailCoefficient = 1f;
        [TweakValue("RimForge", 0f, 1f)]
        public static float RitualFailMajorChance = 1f / 3f;
        [TweakValue("RimForge", 0f, 1f)]
        public static bool RitualMustBeAtNight = true;

        // GREENHOUSE
        [TweakValue("RimForge", 1, 5000)]
        public static int MaxGreenhouseSize = 300;
        [TweakValue("RimForge", 0f, 10f)]
        public static float GreenhouseGrowthAccelerationFactor = 1f;
        [TweakValue("RimForge", 0.01f, 100f)]
        public static float GreenWattsPerCell = 25;

        // OTHER
        [TweakValue("RimForge", 0, 20)]
        public static float HateWaveRadius = 5.5f;
        [TweakValue("RimForge", 0, 1000)]
        public static int HateWaveStunDuration = 400;

        // Preferences
        public static bool UseCustomTab = true;
    }
}
