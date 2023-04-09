namespace territory_lords.Data.Models.Improvements
{
    public class Factory : BaseTileImprovement, ITileImprovement
    {
        public Factory() : base()
        {
            base.MoneyBonus = 2;
            base.BuildCost = 5;
        }
    }
}
