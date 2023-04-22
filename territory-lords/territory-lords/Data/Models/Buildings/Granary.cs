using territory_lords.Data.Technologies;

namespace territory_lords.Data.Models.Buildings
{
    public class Granary : BaseCityBuilding, ICityBuilding
    {
        public Granary() : base()
        {
            BuildingType = BuildingTypes.Granary;
            RequiredTech = new Pottery();
            Maintenance = 1;
            Price = 6;
            SellPrice = 10;
        }
    }
}
