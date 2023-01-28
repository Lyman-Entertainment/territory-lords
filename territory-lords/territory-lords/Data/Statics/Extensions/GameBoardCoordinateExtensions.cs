using territory_lords.Data.Models;

namespace territory_lords.Data.Statics.Extensions
{
    public static class GameBoardCoordinateExtensions
    {
        /// <summary>
        /// Check if a coordinate is at row and column
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsAtPoint(this GameBoardCoordinate coordinate, int row, int column)
        {
            return coordinate.RowIndex == row && coordinate.ColumnIndex == column;
        }
    }
}
