using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models.Units;

namespace territory_lords.Data.Models
{
    public class GameBoard
    {
        public string GameBoardId { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public GameTile[,] Board {get;set;}

        public GameBoard(string gameBoardId, int rows = 15, int columns = 15)
        {
            GameBoardId = gameBoardId;
            RowCount = rows;
            ColumnCount = columns;
            Board = new GameTile[rows, columns];
            InitBoard();
        }

        private void InitBoard()
        {
            
            for(int r = 0; r < this.RowCount; r++)
            {
                for(int c = 0; c < this.ColumnCount; c++)
                {
                    LandType tileLandType = LandType.Ocean;

                    //we want a 1 game tile ocean boarder around the whole thing so land never reaches and edge
                    //find a better way to do this check. This is fine for now
                    if(!(r == 0 || r == this.RowCount - 1 || c == 0 || c == this.ColumnCount - 1))
                    {
                        tileLandType = LandTypeFacotry.GetRandomLandType();
                    }

                    GameTile gameSquare = Board[r, c] = new GameTile {
                        Color = ""
                        , LandType = tileLandType
                        , Improvement = "castle"
                        , Unit = GetRandomUnitType()
                        , RowIndex = r
                        , ColumnIndex = c
                    };

                    //no units in the ocean for now
                    if(Board[r,c].LandType == LandType.Ocean)
                    {
                        Board[r, c].Unit = null;
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
        public GameTile GetGameTileAtIndex(int rowIndex, int columnIndex)
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

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        /// <summary>
        /// Just a silly "random" unit generator for now. Just testing purposes
        /// </summary>
        /// <returns></returns>
        private IUnit GetRandomUnitType()
        {
            var rnd = new Random().Next(40); 
            var unitList = new List<IUnit> {
                new Malitia()
                ,new Phalanx()
                ,new Legion()
                ,new Calvary()
                ,new Chariot()
            };

            if(rnd < unitList.Count)
            {
                return unitList[rnd];
            }
            return null;
        }
    }
}
