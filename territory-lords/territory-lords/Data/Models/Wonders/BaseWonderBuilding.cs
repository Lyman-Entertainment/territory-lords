using territory_lords.Data.Technologies;

namespace territory_lords.Data.Models.Wonders
{
    public abstract class BaseWonderBuilding : IWonderBuilding
    {
        public WonderTypes WonderType { get; init; }
        public ITechnology RequiredTech { get; init; }
        public ITechnology? ObsoleteTech { get; init; }

    }
}
