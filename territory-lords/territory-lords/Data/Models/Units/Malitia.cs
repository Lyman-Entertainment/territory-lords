using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models.Units
{
    public class Malitia : IUnit
    {
        //the setting should probably be private or ctor only
        public byte Price { get; set; } = 1;
        public byte Maintenance { get; set; } = 1;
        public byte Attack { get; set; } = 1;
        public byte Defense { get; set; } = 1;
        public byte MoveDistance { get; set; } = 1;
    }
}
