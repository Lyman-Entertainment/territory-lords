namespace territory_lords.Data.Models
{
    public class City
    {
        internal int Id { get; init; }
        internal int ColumnIndex { get; set; }
        internal int RowIndex { get; set; }
        internal Player OwningPlayer { get; set; }
        internal int Size { get; set; }
        internal string Name { get; set; }

        public City(int id,  int rowIndex, int columnIndex, Player owner, int size)
        {
            Id = id;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            OwningPlayer = owner;
            Size = size;
            Name = $"City_{id}";
        }
    }
}
