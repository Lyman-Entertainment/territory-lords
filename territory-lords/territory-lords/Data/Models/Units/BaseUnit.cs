using System;

namespace territory_lords.Data.Models.Units
{
    public class BaseUnit : IUnit, IEquatable<IUnit>
    {
        public Player OwningPlayer { get; set; }
        public byte Price { get; set; }
        public byte Maintenance { get; set; }
        public byte Attack { get; set; }
        public byte Defense { get; set; }
        public byte MoveDistance { get; set; }
        public bool Active { get; set; }
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }


        public BaseUnit(Player owningPlayer)
        {
            OwningPlayer = owningPlayer;
        }

        public bool Equals(IUnit? otherUnit)
        {
            return otherUnit != null 
                && this.OwningPlayer.Id == otherUnit.OwningPlayer.Id
                && this.ColumnIndex == otherUnit.ColumnIndex 
                && this.RowIndex == otherUnit.ColumnIndex;
        }
    }
}
