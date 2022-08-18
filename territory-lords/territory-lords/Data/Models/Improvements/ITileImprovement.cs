using System.Collections.Generic;

namespace territory_lords.Data.Models.Improvements
{
    public interface ITileImprovement
    {
        public Player OwningPlayer { get; set; }
        public int MoneyBonus { get; set; }
        public int BuildCost { get; set; }
        public List<LandType> AcceptableBuildLocations { get; set; }
    }
}
