namespace territory_lords.Data.Models.Improvements
{
    public class City : BaseTileImprovement, ITileImprovement
    {
        public City(Player owningPlayer) : base(owningPlayer)
        {
            base.MoneyBonus = -1;
            base.BuildCost = 25;
            base.AcceptableBuildLocations.Add(LandType.Grass);
            base.AcceptableBuildLocations.Add(LandType.Hill);
            base.AcceptableBuildLocations.Add(LandType.Forrest);
            base.AcceptableBuildLocations.Add(LandType.Jungle);
            base.AcceptableBuildLocations.Add(LandType.Desert);
        }
    }
}
