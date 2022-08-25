namespace territory_lords.Data.Models.Improvements
{
    public class Fort : BaseTileImprovement, ITileImprovement
    {
        public Fort(Player owningPlayer) : base(owningPlayer)
        {
            base.MoneyBonus = -1;
            base.BuildCost = 10;
            base.AcceptableBuildLocations.Add(LandType.Grassland);
            base.AcceptableBuildLocations.Add(LandType.Hills);
            base.AcceptableBuildLocations.Add(LandType.Forest);
            base.AcceptableBuildLocations.Add(LandType.Jungle);
            base.AcceptableBuildLocations.Add(LandType.Desert);
        }
    }
}
