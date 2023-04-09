using territory_lords.Data.Models;
using territory_lords.Data.Models.Tiles;

namespace territory_lords.Data.Statics.Extensions
{
    public static class GameBoardTileExtensions
    {
        public static GameBoardTile? GetGameBoardTileAtIndex(this GameBoardTile[,] gameBoardTiles, GameBoardCoordinate gameBoardCoordinate)
        {
            return gameBoardTiles.GetGameBoardTileAtIndex(gameBoardCoordinate.RowIndex, gameBoardCoordinate.ColumnIndex);
        }
        public static GameBoardTile? GetGameBoardTileAtIndex(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            //we're outside the board bounds
            if (rowIndex < 0 || columnIndex < 0 || rowIndex > gameTiles.GetLength(0) - 1 || columnIndex > gameTiles.GetLength(1) - 1)
            {
                return null;
            }

            //otherwise return the game tile at that location
            return gameTiles[rowIndex, columnIndex];
        }

        public static GameBoardTile?[] GetGameBoardTileDirectNeighbors(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            var neighbors = new GameBoardTile?[4];
            //get to the north
            neighbors[0] = gameTiles.GetGameBoardTileAtIndex(rowIndex - 1, columnIndex);
            //get to the east
            neighbors[1] = gameTiles.GetGameBoardTileAtIndex(rowIndex, columnIndex + 1);
            //get to the south
            neighbors[2] = gameTiles.GetGameBoardTileAtIndex(rowIndex + 1, columnIndex);
            //get to the west
            neighbors[3] = gameTiles.GetGameBoardTileAtIndex(rowIndex, columnIndex - 1);

            return neighbors;
        }

        public static GameBoardTile?[] GetGameBoardTileCornerNeighbors(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            var neighbors = new GameBoardTile?[4];
            //top right
            neighbors[0] = gameTiles.GetGameBoardTileAtIndex(rowIndex + 1, columnIndex + 1);
            //bottom right
            neighbors[1] = gameTiles.GetGameBoardTileAtIndex(rowIndex - 1, columnIndex + 1);
            //bottom left
            neighbors[2] = gameTiles.GetGameBoardTileAtIndex(rowIndex - 1, columnIndex - 1);
            //top left
            neighbors[3] = gameTiles.GetGameBoardTileAtIndex(rowIndex + 1, columnIndex - 1);

            return neighbors;
        }


        public static GameBoardTile?[] GetAllGameBoardTileNeighbors(this GameBoardTile[,] gameTiles, int rowIndex, int columnIndex)
        {
            var neighbors = new GameBoardTile?[8];
            //get to the north
            neighbors[0] = gameTiles.GetGameBoardTileAtIndex(rowIndex - 1, columnIndex);
            //top right
            neighbors[1] = gameTiles.GetGameBoardTileAtIndex(rowIndex + 1, columnIndex + 1);
            //get to the east
            neighbors[2] = gameTiles.GetGameBoardTileAtIndex(rowIndex, columnIndex + 1);
            //bottom right
            neighbors[3] = gameTiles.GetGameBoardTileAtIndex(rowIndex - 1, columnIndex + 1);
            //get to the south
            neighbors[4] = gameTiles.GetGameBoardTileAtIndex(rowIndex + 1, columnIndex);
            //bottom left
            neighbors[5] = gameTiles.GetGameBoardTileAtIndex(rowIndex - 1, columnIndex - 1);
            //get to the west
            neighbors[6] = gameTiles.GetGameBoardTileAtIndex(rowIndex, columnIndex - 1);
            //top left
            neighbors[7] = gameTiles.GetGameBoardTileAtIndex(rowIndex + 1, columnIndex - 1);

            return neighbors;
        }
    }
}
