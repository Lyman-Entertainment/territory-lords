using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models.Units;

namespace territory_lords.Data.Models
{
    public class GameBoard
    {
        //These should probably be private with some accessors
        public string GameBoardId { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public GameTile[,] Board { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();

        //TODO: handle this differently or more better or something. Creating enums inside this class feels odd
        private enum TileColorsToChooseFrom
        {
            DodgerBlue,
            Pink,
            DarkGreen,
            Yellow,
        };

        private enum BorderColorsToChooseFrom 
        {
            DeepSkyBlue,
            DeepPink,
            DarkOliveGreen,
            DarkYellow
        };

        public GameBoard(string gameBoardId, int rows = 15, int columns = 15)
        {
            //because there is a border of 1 ocean around everything we need to actually make the incoming rows and columns bigger by 2 to make that border
            rows = rows + 2;
            columns = columns + 2;

            GameBoardId = gameBoardId;
            RowCount = rows;//do we need this, can't this be figured out by getting the length of the different directions of the Board?
            ColumnCount = columns;
            Board = new GameTile[rows, columns];

            ////for now do this
            //Players = new List<Player> {
            //    new Player { Id = new Guid("2a35f57a-b83a-4f70-9a38-0755c7540721"),Name = "KHL-Y",Color = "dodgerblue", BorderColor = "deepblue"},
            //    new Player { Id = new Guid("e3428b1c-343e-4008-aac5-f84fcea54088"),Name = "KHL-G",Color = "pink", BorderColor = "deeppink" }
            //};
            InitBoard();
        }

        private void InitBoard()
        {
            for (int r = 0; r < this.RowCount; r++)
            {
                for (int c = 0; c < this.ColumnCount; c++)
                {
                    LandType tileLandType = LandType.Ocean;

                    //we want a 1 game tile ocean boarder around the whole thing so land never reaches and edge
                    //find a better way to do this check. This is fine for now
                    if (!(r == 0 || r == this.RowCount - 1 || c == 0 || c == this.ColumnCount - 1))
                    {
                        tileLandType = LandTypeFacotry.GetRandomLandType();
                    }

                    ////fill with some bogus player owned tiles
                    //Player? fillPlayerForNow = null;
                    //if (r >= 6 && r <= 8 && c >= 6 && c <= 8)
                    //{
                    //    fillPlayerForNow = Players[0];
                    //}
                    ////this some Capture stuff right here.
                    //if (r >= 1 && r <= 3 && c >= 1 && c <= 3)
                    //{
                    //    fillPlayerForNow = Players[1];
                    //}

                    //GameTile gameSquare = Board[r, c] = new GameTile
                    //{
                    //    OwningPlayer = fillPlayerForNow,
                    //    LandType = tileLandType,
                    //    Improvement = "castle",
                    //    Unit = GetRandomUnitType(),
                    //    RowIndex = r,
                    //    ColumnIndex = c
                    //};

                    Board[r, c] = new GameTile
                    {
                        OwningPlayer = null,
                        LandType = tileLandType,
                        Improvement = "",
                        Unit = null,
                        RowIndex = r,
                        ColumnIndex = c
                    };

                    //no units in the ocean or players owning the ocean for now
                    if (Board[r, c].LandType == LandType.Ocean)
                    {
                        Board[r, c].Unit = null;
                        Board[r, c].OwningPlayer = null;
                    }
                }
            }
        }


        /// <summary>
        /// Return a game tile at a row and column index.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns>GameTile object. Null if one doesn't exist</returns>
        public GameTile? GetGameTileAtIndex(int rowIndex, int columnIndex)
        {
            //we're outside the board bounds
            if (rowIndex < 0 || columnIndex < 0 || rowIndex > RowCount - 1 || columnIndex > ColumnCount - 1)
            {
                return null;
            }

            //otherwise return the game tile at that location
            return Board[rowIndex, columnIndex];
        }

        /// <summary>
        /// Get the NESW cardinal direction neighbors of a gameTile
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns>GameTile[4] starting at 12 o clock or North</returns>
        public GameTile[] GetGameTileDirectNeighbors(GameTile gameTile)
        {

            var neighbors = new GameTile[4];
            //get to the north
            neighbors[0] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex);
            //get to the east
            neighbors[1] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex + 1);
            //get to the south
            neighbors[2] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex);
            //get to the west
            neighbors[3] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex - 1);

            return neighbors;
        }

        /// <summary>
        /// Get the corner neighbors
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns>GameTile[4] starting at top right or North-West corner</returns>
        public GameTile[] GetGameTileCornerNeighbors(GameTile gameTile)
        {
            var neighbors = new GameTile[4];
            //top right
            neighbors[0] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex + 1);
            //bottom right
            neighbors[1] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex + 1);
            //bottom left
            neighbors[2] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex - 1);
            //top left
            neighbors[3] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex - 1);
            return neighbors;
        }

        public GameTile[] GetAllGameTileNeighbors(GameTile gameTile)
        {
            var neighbors = new GameTile[8];
            //get to the north
            neighbors[0] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex);
            //top right
            neighbors[1] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex + 1);
            //get to the east
            neighbors[2] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex + 1);
            //bottom right
            neighbors[3] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex + 1);
            //get to the south
            neighbors[4] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex);
            //bottom left
            neighbors[5] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex - 1);
            //get to the west
            neighbors[6] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex - 1);
            //top left
            neighbors[7] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex - 1);

            return neighbors;
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

                Player newPlayer = new Player
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
        /// Just a silly "random" unit generator for now. Just testing purposes
        /// </summary>
        /// <returns></returns>
        private IUnit? GetRandomUnitType()
        {
            
            var rnd = new Random().Next(40);

            //grab one of the players
            var tempPlayer = rnd % 2 == 0 ? Players[0] : Players[1]; 
            var unitList = new List<IUnit> {
                new Malitia(tempPlayer)
                ,new Phalanx(tempPlayer)
                ,new Legion(tempPlayer)
                ,new Calvary(tempPlayer)
                ,new Chariot(tempPlayer)
            };

            //a chinsy way to only put units on some of the tiles since there are only 5 unit types but our random goes to 40. That's 5/40 or ~1 out of every 8 that will have a unit
            if (rnd < unitList.Count)
            {
                return unitList[rnd];
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
                TileColor = ((TileColorsToChooseFrom) Players.Count).ToString(), 
                BorderColor = ((BorderColorsToChooseFrom)Players.Count).ToString()
            };
        }

    }
}
