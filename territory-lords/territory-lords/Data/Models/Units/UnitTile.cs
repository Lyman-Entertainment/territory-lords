using Newtonsoft.Json;
using System;

namespace territory_lords.Data.Models.Units
{
    public class UnitTile : IEquatable<UnitTile>
    {
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
        public IUnit Unit { get; set; }
        public Player OwningPlayer { get; set; }
        public bool Active { get; set; }

        public UnitTile(Player owningPlayer)
        {
            OwningPlayer = owningPlayer;
            Unit = new Malitia();
        }
        public bool Equals(UnitTile? otherUnit)
        {
            return otherUnit != null
                && this.OwningPlayer.Id == otherUnit.OwningPlayer.Id
                && this.ColumnIndex == otherUnit.ColumnIndex
                && this.RowIndex == otherUnit.ColumnIndex;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}


