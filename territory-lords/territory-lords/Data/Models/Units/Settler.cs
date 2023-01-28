using System;

namespace territory_lords.Data.Models.Units
{
    public class Settler : BaseUnit, IUnit
    {
        public Settler(GameBoardCoordinate coordinate, Player owningPlayer, int id) : base()
        {
            base.Price = 4;
            base.Maintenance = 1;
            base.Attack = 0;
            base.Defense = 1;
            base.MoveDistance = 1;
            base.Active = false;
            base.Coordinate = coordinate;
            base.OwningPlayer = owningPlayer;

            base.Id = id;
        }
    }
}
