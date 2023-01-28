using Newtonsoft.Json;
using territory_lords.Data.Models.Improvements;
using territory_lords.Data.Models.Units;

namespace territory_lords.Data.Models.Tiles
{
    public class GameBoardTile
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public LandType LandType { get; set; }
        public ITileImprovement? Improvement { get; set; }
        public Player? OwningPlayer { get; set; }
        public bool Special { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        public GameBoardTile(int rowIndex, int columnIndex, LandType landType, ITileImprovement? improvement = null, Player? owningPlayer = null)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
            LandType = landType;
            Improvement = improvement;
            //Unit = unit;
            OwningPlayer = owningPlayer;
        }
    }
}
