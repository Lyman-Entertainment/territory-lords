using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using territory_lords.Data.Models.Units;
using territory_lords.Data.Statics;
using territory_lords.Data.Models.Tiles;
using territory_lords.Data.Statics.Extensions;

namespace territory_lords.Data.Models
{
    public class GameBoard
    {
        //These should probably be private with some accessors
        public string GameBoardId { get; set; }
        public int RowCount { get => GameTileLayer.GetLength(0); }
        public int ColumnCount { get => GameTileLayer.GetLength(1); }
        public int LandMass { get; init; }
        public int Temperature { get; init; }
        public int Climate { get; init; }
        public int Age { get; init; }
        /// <summary>
        /// What would be the physical board in a real game. This is what holds the land and it's properties
        /// </summary>
        public GameBoardTile[,] GameTileLayer { get; set; }

        /// <summary>
        /// A list of units. This will probably need to change as having a list of units that needs to be sorted or searched all the time will suck
        /// </summary>
        public  List<IUnit> UnitBag { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        private static readonly Random RandomNumGen = new();

        public GameBoard(string gameBoardId, GameBoardTile[,] gameTiles, int landMass, int temperature, int climate, int age)
        {

            //these should only be 1-3, maybe check that here
            LandMass = landMass;
            Temperature = temperature;
            Climate = climate;
            Age = age;

            GameBoardId = gameBoardId;
            GameTileLayer = gameTiles;
            UnitBag = new();
        }
        

        /// <summary>
        /// Exactly what you think it is
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        /// <summary>
        /// Adds a player to the game if they don't already exist
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public Player? AddPlayerToGame(Guid playerId, string playerName)
        {
            //if they don't exist then add them
            if (!Players.Any(p => p.Id == playerId))
            {

                Player newPlayer = new()
                {
                    Id = playerId,
                    Name = playerName,
                    Colors = GetNextAvailableColors()
                };
                Players.Add(newPlayer);
                return newPlayer;
            }
            return null;
        }

        /// <summary>
        /// Gets a new set of colors based on how many players are in the game
        /// </summary>
        /// <returns></returns>
        private PlayerColors GetNextAvailableColors()
        {
            //TODO:This will fail if there are more than 3 players or more players than colors. Also it's always going to be the same colors in that order and that's lame.
            return new PlayerColors { 
                TileColor = ((TileColors) Players.Count).ToString(), 
                BorderColor = ((BorderColors)Players.Count).ToString()
            };
        }


        public GameBoardTile InsertCityToMountainSpotOnMap(Player owningPlayer)
        {
            var mountains = (from GameBoardTile tile in GameTileLayer where tile.LandType == LandType.Mountains select tile).ToArray();
            var randomTile = mountains[RandomNumGen.Next(mountains.Length)];
            while (UnitBag.Where(u => u.Coordinate.IsAtPoint(randomTile.RowIndex, randomTile.ColumnIndex)).Count() > 0)
            {
                randomTile = mountains[RandomNumGen.Next(mountains.Length)];
            }
            var city = new Data.Models.Improvements.City(owningPlayer);
            randomTile.Improvement = city;
            return randomTile;
        }

        /// <summary>
        /// Probably just for now putting a unit on the board in a random spot
        /// </summary>
        /// <param name="newUnit"></param>
        [Obsolete("Only for development purposes")]
        public IUnit InsertUnitToRandomSpotOnMap(Player owningPlayer, UnitName unitName)
        {
            var grassland = (from GameBoardTile tile in GameTileLayer where tile.LandType == LandType.Grassland select tile).ToArray();
            var randomTile = grassland[RandomNumGen.Next(grassland.Length)];
            while(UnitBag.Where(u => u.Coordinate.IsAtPoint(randomTile.RowIndex,randomTile.ColumnIndex)).Count() > 0)
            {
                randomTile = grassland[RandomNumGen.Next(grassland.Length)];
            }

            IUnit returnUnit;
            switch (unitName)
            {
                case UnitName.Settler:
                    returnUnit = new Settler(new GameBoardCoordinate(randomTile.RowIndex, randomTile.ColumnIndex), owningPlayer, RandomNumGen.Next());
                    break;
                case UnitName.Malitia:
                    returnUnit = new Malitia(new GameBoardCoordinate(randomTile.RowIndex, randomTile.ColumnIndex), owningPlayer, RandomNumGen.Next());
                    break;
                case UnitName.Phalanx:
                    returnUnit = new Phalanx(new GameBoardCoordinate(randomTile.RowIndex, randomTile.ColumnIndex), owningPlayer, RandomNumGen.Next());
                    break;
                case UnitName.Calvary:
                    returnUnit = new Calvary(new GameBoardCoordinate(randomTile.RowIndex, randomTile.ColumnIndex), owningPlayer, RandomNumGen.Next());
                    break;
                case UnitName.Legion:
                    returnUnit = new Legion(new GameBoardCoordinate(randomTile.RowIndex, randomTile.ColumnIndex), owningPlayer, RandomNumGen.Next());
                    break;
                case UnitName.Chariot:
                    returnUnit = new Chariot(new GameBoardCoordinate(randomTile.RowIndex, randomTile.ColumnIndex), owningPlayer, RandomNumGen.Next());
                    break;
                default:
                    throw new ArgumentException("No Unit Matches Enum Type");
            }

            UnitBag.Add(returnUnit);

            return returnUnit;
        }
    }
}
