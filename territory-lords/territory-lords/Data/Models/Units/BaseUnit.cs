namespace territory_lords.Data.Models.Units
{
    public class BaseUnit : IUnit
    {
        public Player? OwningPlayer { get; set; } = default;
        public byte Price { get; set; }
        public byte Maintenance { get; set; }
        public byte Attack { get; set; }
        public byte Defense { get; set; }
        public byte MoveDistance { get; set; }
        public bool Active { get; set; }
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }

    }
}
