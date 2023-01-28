namespace territory_lords.Data.Models
{
    public struct GameBoardCoordinate
    {
        public GameBoardCoordinate(int rowIndex, int columnIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
        }

        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
    }
}
