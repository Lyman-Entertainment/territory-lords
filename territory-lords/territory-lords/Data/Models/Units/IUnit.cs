using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models.Units
{
    public interface IUnit
    {
        public byte Price { get; set; }
        public byte Maintenance { get; set; }
        public byte Attack { get;  set; }
        public byte Defense { get;  set; }
        public byte MoveDistance { get; set; }
    }
}
