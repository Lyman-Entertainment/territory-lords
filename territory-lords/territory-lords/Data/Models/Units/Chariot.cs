using System;

namespace territory_lords.Data.Models.Units
{
    public class Chariot : BaseUnit, IUnit
    {
        public Chariot(GameBoardCoordinate coordinate, Player owningPlayer, int id) : base()
        {
            base.Price = 4;
            base.Maintenance = 1;
            base.Attack = 4;
            base.Defense = 1;
            base.MoveDistance = 2;
            base.Active = false;
            base.Coordinate = coordinate;
            base.OwningPlayer = owningPlayer;

            base.Id = id;
        }
    }
}
