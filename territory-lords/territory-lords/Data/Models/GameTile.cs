using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models.Units;

namespace territory_lords.Data.Models
{
    public class GameTile
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }    
        public string Color { get; set; }
        public LandType LandType { get; set; }
        public string Improvement { get; set; }
        public IUnit Unit { get; set; }
    }
}
