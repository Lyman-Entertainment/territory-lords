using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using territory_lords.Data.Models.Units;

namespace territory_lords.Data.Models.Tiles
{
    public class UnitTile : IEquatable<UnitTile>
    {
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
        public List<IUnit>? Units { get; set; }
        public Player? OwningPlayer { get; set; }


        /// <summary>
        /// Empty ctor
        /// </summary>
        public UnitTile()
        {

        }
        /// <summary>
        /// ctor for a player
        /// </summary>
        /// <param name="owningPlayer"></param>
        public UnitTile(Player owningPlayer)
        {
            OwningPlayer = owningPlayer;
            
        }


        /// <summary>
        /// To see if a tile is at the same location as another tile.
        /// </summary>
        /// <param name="otherUnit"></param>
        /// <returns></returns>
        public bool Equals(UnitTile? otherUnit)
        {
            return otherUnit != null
                && ColumnIndex == otherUnit.ColumnIndex
                && RowIndex == otherUnit.ColumnIndex;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}


