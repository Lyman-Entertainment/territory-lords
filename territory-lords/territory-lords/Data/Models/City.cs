using System.Collections.Generic;
using territory_lords.Data.Models.Buildings;
using territory_lords.Data.Models.Wonders;

namespace territory_lords.Data.Models
{
    public class City
    {
        internal int FoodStored { get; private set; }
        internal int Id { get; init; }
        internal int ColumnIndex { get; set; }
        internal int RowIndex { get; set; }
        internal Player OwningPlayer { get; set; }
        /// <summary>
        /// The numeric size of the city in a small number to be displayed on the map.
        /// </summary>
        internal byte Size { get; set; }
        /// <summary>
        /// The name that will be displayed
        /// </summary>
        internal string Name { get; set; }
        /// <summary>
        /// What buildings does this city have
        /// </summary>
        internal List<ICityBuilding> Buildings = new();
        /// <summary>
        /// What wonders does this city have
        /// </summary>
        internal List<IWonderBuilding> Wonders = new();
        /// <summary>
        /// The big number population of a city. Used for display in the City view
        /// </summary>
        internal int Population
        {
            get {
                //Calculate the simulated population size of the city
                int totalPop = 0;
                for (int i = 0; i < Size; i++)
                {
                    totalPop += 10000 * i;
                }
                return totalPop; 
            }
        }

        public City(int id, int rowIndex, int columnIndex, Player owner, byte size)
        {
            Id = id;
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            OwningPlayer = owner;
            Size = size;
            Name = $"City_{id}";
        }

        public City(int id, GameBoardCoordinate gameBoardCoordinate, Player owner, byte size) : this(id, gameBoardCoordinate.RowIndex, gameBoardCoordinate.ColumnIndex, owner, size)
        {

        }

        public void AddFood()
        {
            //Look at the land around the city and how much food the city is using and then add or subtract food from the _storedFood
            //if there is extra make the city bigger and then set the food stored to the remainder
            //if there is no food subtract from the city population
        }
    }
}
