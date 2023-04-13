using System.Collections.Generic;
using territory_lords.Data.Technologies;

namespace territory_lords.Data.Models.Buildings
{
    public interface ICityBuilding
    {
        public BuildingTypes BuildingType { get;}
        public ITechnology RequiredTech { get; }
        public byte Maintenance { get; }
        public byte Price { get; }
        public short SellPrice { get; }
    }
}
