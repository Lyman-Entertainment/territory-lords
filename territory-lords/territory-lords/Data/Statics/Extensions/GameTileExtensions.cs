using territory_lords.Data.Models;
using territory_lords.Data.Models.Board;

namespace territory_lords.Data.Statics.Extensions
{
    public static class GameTileExtensions
    {
        public static GameBoardTile? GetGameTileAtIndex(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            //we're outside the board bounds
            if (rowIndex < 0 || columnIndex < 0 || rowIndex > gameTiles.GetLength(0) - 1 || columnIndex > gameTiles.GetLength(1) - 1)
            {
                return null;
            }

            //otherwise return the game tile at that location
            return gameTiles[rowIndex, columnIndex];
        }

        public static GameBoardTile?[] GetGameTileDirectNeighbors(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            var neighbors = new GameBoardTile?[4];
            //get to the north
            neighbors[0] = gameTiles.GetGameTileAtIndex(rowIndex - 1, columnIndex);
            //get to the east
            neighbors[1] = gameTiles.GetGameTileAtIndex(rowIndex, columnIndex + 1);
            //get to the south
            neighbors[2] = gameTiles.GetGameTileAtIndex(rowIndex + 1, columnIndex);
            //get to the west
            neighbors[3] = gameTiles.GetGameTileAtIndex(rowIndex, columnIndex - 1);

            return neighbors;
        }

        public static GameBoardTile?[] GetGameTileCornerNeighbors(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            var neighbors = new GameBoardTile?[4];
            //top right
            neighbors[0] = gameTiles.GetGameTileAtIndex(rowIndex + 1, columnIndex + 1);
            //bottom right
            neighbors[1] = gameTiles.GetGameTileAtIndex(rowIndex - 1, columnIndex + 1);
            //bottom left
            neighbors[2] = gameTiles.GetGameTileAtIndex(rowIndex - 1, columnIndex - 1);
            //top left
            neighbors[3] = gameTiles.GetGameTileAtIndex(rowIndex + 1, columnIndex - 1);

            return neighbors;
        }


        public static GameBoardTile?[] GetAllGameTileNeighbors(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            var neighbors = new GameBoardTile?[8];
            //get to the north
            neighbors[0] = gameTiles.GetGameTileAtIndex(rowIndex - 1, columnIndex);
            //top right
            neighbors[1] = gameTiles.GetGameTileAtIndex(rowIndex + 1, columnIndex + 1);
            //get to the east
            neighbors[2] = gameTiles.GetGameTileAtIndex(rowIndex, columnIndex + 1);
            //bottom right
            neighbors[3] = gameTiles.GetGameTileAtIndex(rowIndex - 1, columnIndex + 1);
            //get to the south
            neighbors[4] = gameTiles.GetGameTileAtIndex(rowIndex + 1, columnIndex);
            //bottom left
            neighbors[5] = gameTiles.GetGameTileAtIndex(rowIndex - 1, columnIndex - 1);
            //get to the west
            neighbors[6] = gameTiles.GetGameTileAtIndex(rowIndex, columnIndex - 1);
            //top left
            neighbors[7] = gameTiles.GetGameTileAtIndex(rowIndex + 1, columnIndex - 1);

            return neighbors;
        }
    }
}
