using Verse;

namespace RimForge
{
    public class IngredientValueGetter_IgnoreVolume : IngredientValueGetter_Volume
    {
        public override float ValuePerUnitOf(ThingDef t) => 1f;

        public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing) => "BillRequires".Translate(ing.GetBaseCount(), ing.filter.Summary);

        public override string ExtraDescriptionLine(RecipeDef r) => null;
    }
}
