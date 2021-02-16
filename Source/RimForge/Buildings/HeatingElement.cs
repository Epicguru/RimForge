using Verse;

namespace RimForge.Buildings
{
    public abstract class HeatingElement : Building
    {
        public HeatingElementDef HEDef => def as HeatingElementDef;

        public Building_Forge ConnectedForge
        {
            get
            {
                if (_forge != null)
                {
                    if (_forge.Destroyed || !_forge.Spawned)
                        _forge = null;
                }
                return _forge;
            }
            set => _forge = value;
        }
        private Building_Forge _forge;

        /// <summary>
        /// Gets the current heat offset that this provides to a forge,
        /// in degrees celsius. Should return 0 when inactive.
        /// </summary>
        public abstract float GetProvidedHeat();

        public override string GetInspectString()
        {
            bool isConnected = ConnectedForge != null;
            float heat = GetProvidedHeat();
            bool isProvidingHeat = heat != 0;

            string connected = (isProvidingHeat && isConnected)
                ? "RF.ConnectedToForgeActive".Translate(heat.ToStringTemperatureOffset())
                : isConnected
                    ? "RF.ConnectedToForge".Translate()
                    : "RF.NotConnectedToForge".Translate();

            return $"{base.GetInspectString()}\n{connected}";
        }

        public virtual string GetInspectorQuickString()
        {
            float heat = GetProvidedHeat();
            bool isProvidingHeat = heat != 0;
            return $"{HEDef.LabelCap}, {(isProvidingHeat ? "RF.Active".Translate() : "RF.NotActive".Translate())}{(isProvidingHeat ? $", {heat.ToStringTemperatureOffset()}" : "")}";
        }
    }
}
