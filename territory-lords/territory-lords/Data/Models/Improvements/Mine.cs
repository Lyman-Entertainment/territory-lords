namespace territory_lords.Data.Models.Improvements
{
    public class Mine : BaseTileImprovement, ITileImprovement
    {
        public Mine(Player owningPlayer) : base(owningPlayer)
        {
            base.MoneyBonus = 1;
            base.BuildCost = 5;
            base.AcceptableBuildLocations.Add(LandType.Hills);
            base.AcceptableBuildLocations.Add(LandType.Mountains);
        }
    }
}
