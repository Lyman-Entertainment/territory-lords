namespace territory_lords.Data.Models.Units
{
    public class Settler : BaseUnit, IUnit
    {
        public Settler() : base()
        {
            base.Price = 4;
            base.Maintenance = 1;
            base.Attack = 0;
            base.Defense = 1;
            base.MoveDistance = 1;
        }
    }
}
