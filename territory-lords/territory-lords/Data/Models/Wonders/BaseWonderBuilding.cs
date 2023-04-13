namespace territory_lords.Data.Models.Wonders
{
    public abstract class BaseWonderBuilding : IWonderBuilding
    {
        public WonderTypes WonderType { get; protected set; }
        public string RequiredTech { get; protected set; }
        public string? ObsoleteTech { get; protected set; }

    }
}
