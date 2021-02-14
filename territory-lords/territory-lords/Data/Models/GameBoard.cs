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
        public GameTile[,] Board {get;set;}

        public GameBoard(int rows, int columns)
        {
            MaxRows = rows;
            MaxColumns = columns;
            Board = new GameTile[rows, columns];
            InitBoard();
        }

        private void InitBoard()
        {
            for(int r = 0; r < this.MaxRows; r++)
            {
                for(int c = 0; c < this.MaxColumns; c++)
                {
                    GameTile gameSquare = Board[r, c] = new GameTile {
                        Color = "green"
                        , LandType = LandTypeFacotry.GetRandomLandType()
                        ,Improvement = "castle"
                        ,Piece = "peasant"
                    };
                }
            }
        }
    }
}
