using System;

namespace territory_lords.Data.Models.Units
{
    public class Malitia : BaseUnit, IUnit
    {
        public Malitia(GameBoardCoordinate coordinate, Player owningPlayer, int id) : base()
        {
            base.Price = 1;
            base.Maintenance = 1;
            base.Attack = 1;
            base.Defense = 1;
            base.MoveDistance = 1;
            base.Active = false;
            base.Coordinate = coordinate;
            base.OwningPlayer = owningPlayer;

            base.Id = id;
        }
    }
}
