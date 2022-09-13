namespace territory_lords.Data.Models
{
    public class City
    {
        internal int Id { get; init; }
        internal byte ColumnIndex { get; set; }
        internal byte RowIndex { get; set; }
        internal Player Owner { get; set; }
        internal byte Size { get; set; }

        public City(int id, byte columnIndex, byte rowIndex, Player owner, byte size)
        {
            Id = id;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            Owner = owner;
            Size = size;
        }
    }
}
