using System;

namespace territory_lords.Data.Models.Units
{
    public class Legion : BaseUnit, IUnit
    {
        public Legion(GameBoardCoordinate coordinate, Player owningPlayer, int id) : base()
        {
            base.Price = 2;
            base.Maintenance = 1;
            base.Attack = 3;
            base.Defense = 1;
            base.MoveDistance = 1;
            base.Active = false;
            base.Coordinate = coordinate;
            base.OwningPlayer = owningPlayer;

            base.Id = id;
        }
    }
}
