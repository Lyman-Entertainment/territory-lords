using System;

namespace territory_lords.Data.Models.Units
{
    public class BaseUnit : IUnit
    {
        public byte Price { get; set; }
        public byte Maintenance { get; set; }
        public byte Attack { get; set; }
        public byte Defense { get; set; }
        public byte MoveDistance { get; set; }

    }
}
