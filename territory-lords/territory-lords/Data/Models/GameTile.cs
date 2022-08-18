using Newtonsoft.Json;
using territory_lords.Data.Models.Improvements;
using territory_lords.Data.Models.Units;

namespace territory_lords.Data.Models
{
    public class GameTile
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public LandType LandType { get; set; }
        public ITileImprovement? Improvement { get; set; }
        public IUnit? Unit { get; set; }
        public Player? OwningPlayer { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}
