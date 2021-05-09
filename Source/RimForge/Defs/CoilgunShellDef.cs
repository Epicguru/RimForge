using Verse;

namespace RimForge
{
    public class CoilgunShellDef : ThingDef
    {
        public float baseDamage = 800;
        public int maxPen = -1;
        public float penDamageMultiplier = 0.95f;
        public bool pawnsCountAsPen = false;

        public float explosionRadius = 5f;
        public DamageDef explosionDamageType;
        public int? explosionDamage;
        public float? explosionArmorPen;
    }
}
