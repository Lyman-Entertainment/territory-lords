namespace territory_lords.Data.Models.Units
{
    public class Settler : BaseUnit, IUnit
    {
        public Settler(Player player) : base(player)
        {
            base.Price = 1;
            base.Maintenance = 1;
            base.Attack = 0;
            base.Defense = 0;
            base.MoveDistance = 1;
            base.Active = false;
        }
    }
}
