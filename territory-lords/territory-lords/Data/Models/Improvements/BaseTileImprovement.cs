using System.Collections.Generic;

namespace territory_lords.Data.Models.Improvements
{
    public class BaseTileImprovement : ITileImprovement
    {
        public Player OwningPlayer { get; set; }
        public int MoneyBonus { get; set; }
        public int BuildCost { get; set; }
        public List<LandType> AcceptableBuildLocations { get; set; }

        public BaseTileImprovement(Player owningPlayer)
        {
            OwningPlayer = owningPlayer;
            AcceptableBuildLocations = new List<LandType>();
        }
    }
}
