using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models
{
    public class GameBoard
    {
        public int MaxRows { get; set; }
        public int MaxColumns { get; set; }
        public GameSquare[,] Board {get;set;}

        public GameBoard(int rows, int columns)
        {
            MaxRows = rows;
            MaxColumns = columns;
            Board = new GameSquare[rows, columns];
            InitBoard();
        }

        private void InitBoard()
        {
            for(int r = 0; r < this.MaxRows; r++)
            {
                for(int c = 0; c < this.MaxColumns; c++)
                {
                    GameSquare gameSquare = Board[r, c] = new GameSquare { Color = "green" };
                }
            }
        }
    }
}
