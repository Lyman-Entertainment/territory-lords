namespace territory_lords.Data.Models.Wonders
{
    public interface IWonderBuilding
    {
        public WonderTypes WonderType { get; }
        public string RequiredTech { get; }
        public string? ObsoleteTech { get; }
    }
}
