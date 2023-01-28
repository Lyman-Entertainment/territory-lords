using System;

namespace territory_lords.Data.Models.Units
{
    public class Phalanx : BaseUnit, IUnit
    {
        public Phalanx(GameBoardCoordinate coordinate, Player owningPlayer, int id) : base()
        {
            base.Price = 2;
            base.Maintenance = 1;
            base.Attack = 1;
            base.Defense = 2;
            base.MoveDistance = 1;
            base.Active = false;
            base.Coordinate = coordinate;
            base.OwningPlayer = owningPlayer;

            base.Id = id;
        }
    }
}
