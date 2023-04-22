using territory_lords.Data.Technologies;

namespace territory_lords.Data.Models.Wonders
{
    public interface IWonderBuilding
    {
        public WonderTypes WonderType { get; }
        public ITechnology RequiredTech { get; }
        public ITechnology? ObsoleteTech { get; }
    }
}
