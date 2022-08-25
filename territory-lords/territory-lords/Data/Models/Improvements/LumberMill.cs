namespace territory_lords.Data.Models.Improvements
{
    public class LumberMill : BaseTileImprovement, ITileImprovement
    {
        public LumberMill(Player owningPlayer) : base(owningPlayer)
        {
            base.MoneyBonus = 1;
            base.BuildCost = 5;
            base.AcceptableBuildLocations.Add(LandType.Forest);
            base.AcceptableBuildLocations.Add(LandType.Jungle);
        }
    }
}
