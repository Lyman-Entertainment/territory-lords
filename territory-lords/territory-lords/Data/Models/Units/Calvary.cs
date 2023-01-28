using System;

namespace territory_lords.Data.Models.Units
{
    public class Calvary : BaseUnit, IUnit
    {
        public Calvary(GameBoardCoordinate coordinate, Player owningPlayer, int id) : base()
        {
            base.Price = 2;
            base.Maintenance = 1;
            base.Attack = 2;
            base.Defense = 1;
            base.MoveDistance = 2;
            base.Active = false;
            base.Coordinate = coordinate;
            base.OwningPlayer = owningPlayer;

            base.Id = id;
        }
    }
}
