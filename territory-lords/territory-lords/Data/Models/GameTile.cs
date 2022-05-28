using Newtonsoft.Json;
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
        [Obsolete("Player property will contain color")]
        public string Color { get; set; } = default!;
        public LandType LandType { get; set; }
        public string Improvement { get; set; } = default!;
        public IUnit? Unit { get; set; }
        public Player? OwningPlayer { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}
