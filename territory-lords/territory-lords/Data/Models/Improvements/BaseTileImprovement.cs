using System.Collections.Generic;

namespace territory_lords.Data.Models.Improvements
{
    public class BaseTileImprovement : ITileImprovement
    {
        public int MoneyBonus { get; set; }
        public int BuildCost { get; set; }
        public List<LandType> AcceptableBuildLocations { get; set; } = new List<LandType>();

    }
}
