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
        public bool Active { get; set; }
        public byte Player { get; set; }//this will be turned into a player object later but is byte for now
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
        
    
    }
}
