using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models
{
    public class GameTile
    {
        public string Color { get; set; }
        public LandType LandType { get; set; }
        public string Improvement { get; set; }
        public string Piece { get; set; }
    }
}
