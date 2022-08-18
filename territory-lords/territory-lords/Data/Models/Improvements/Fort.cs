namespace territory_lords.Data.Models.Improvements
{
    public class Fort : BaseTileImprovement, ITileImprovement
    {
        public Fort(Player owningPlayer) : base(owningPlayer)
        {
            base.MoneyBonus = -1;
            base.BuildCost = 10;
            base.AcceptableBuildLocations.Add(LandType.Grass);
            base.AcceptableBuildLocations.Add(LandType.Hill);
            base.AcceptableBuildLocations.Add(LandType.Forrest);
            base.AcceptableBuildLocations.Add(LandType.Jungle);
            base.AcceptableBuildLocations.Add(LandType.Desert);
        }
    }
}
