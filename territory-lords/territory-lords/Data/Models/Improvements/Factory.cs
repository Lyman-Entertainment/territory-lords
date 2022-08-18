namespace territory_lords.Data.Models.Improvements
{
    public class Factory : BaseTileImprovement, ITileImprovement
    {
        public Factory(Player owningPlayer) : base(owningPlayer)
        {
            base.MoneyBonus = 2;
            base.BuildCost = 5;
        }
    }
}
