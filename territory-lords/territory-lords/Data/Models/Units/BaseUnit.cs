using Newtonsoft.Json;
using System;
using territory_lords.Data.Models.Tiles;

namespace territory_lords.Data.Models.Units
{
    public abstract class BaseUnit : IUnit
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public byte Price { get; set; }
        public byte Maintenance { get; set; }
        public byte Attack { get; set; }
        public byte Defense { get; set; }
        public byte MoveDistance { get; set; }
        public GameBoardCoordinate Coordinate { get; set; }
        public Player? OwningPlayer { get; set; }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
    }
}
