using System.Collections.Generic;
using territory_lords.Data.Technologies;

namespace territory_lords.Data.Models.Buildings
{
    public abstract class BaseCityBuilding : ICityBuilding
    {
        public BuildingTypes BuildingType { get; protected set; }
        public ITechnology RequiredTech { get; protected set; } = default!;
        public byte Maintenance { get; protected set; }
        public byte Price { get; protected set; }
        public short SellPrice { get; protected set; }
    }
}
