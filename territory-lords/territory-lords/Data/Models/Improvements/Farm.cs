namespace territory_lords.Data.Models.Improvements
{
    public class Farm : BaseTileImprovement, ITileImprovement
    {
        public Farm(Player owningPlayer) : base(owningPlayer)
        {
            base.MoneyBonus = 1;
            base.BuildCost = 5;
            base.AcceptableBuildLocations.Add(LandType.Grassland);
            base.AcceptableBuildLocations.Add(LandType.Hills);
        }
    }
}
