using System;
using territory_lords.Data.Models;
using System.Linq;
using territory_lords.Data.Statics.Extensions;

namespace territory_lords.Data
{
    public class WorldBuilder
    {
        public string GameBoardId { get; set; }
        public int RowCount { get; init; }
        public int ColumnCount { get; init; }
        public int LandMass { get; init; }
        public int Temperature { get; init; }
        public int Climate { get; init; }
        public int Age { get; init; }
        public int ResourceAbundance { get; set; } = 4;
        private int _specialSeed { get; set; }
        public GameTile[,] Board { get; private set; }
        private static readonly Random RandomNumGen = new();

        public WorldBuilder(string gameBoardId, int rows = 25, int columns = 25, int landMass = 1, int temperature = 1, int climate = 1, int age = 1)
        {
            //because there is a border of 1 ocean around everything we need to actually make the incoming rows and columns bigger by 2 to make that border
            rows += 2;
            columns += 2;

            LandMass = landMass;
            Temperature = temperature;
            Climate = climate;
            Age = age;

            GameBoardId = gameBoardId;
            RowCount = rows;//do we need this, can't this be figured out by getting the length of the different directions of the Board?
            ColumnCount = columns;
            Board = new GameTile[rows, columns];
            _specialSeed = RandomNumGen.Next(16);//seed it with a random number
        }

        /// <summary>
        /// Kicks off the world generation process
        /// </summary>
        /// <returns></returns>
        public GameBoard GenerateWorld()
        {
            var elevationMap = GenerateElevationMap();
            var latitudeMap = TemperatureAdjustments();
            MergeElevationAndLatitudeMaps(elevationMap, latitudeMap);
            ClimateAdjustments();
            AgeAdjustments();
            CreateRivers();
            CreateSpecialTiles();

            return new GameBoard(GameBoardId, Board, LandMass, Temperature, Climate, Age);
        }



        /// <summary>
        /// Uses the maps row and column counts to generate sections of land for the map at a time
        /// </summary>
        /// <returns></returns>
        private bool[,] GenerateLandChunk()
        {

            int rowBuffer = 3, colBuffer = 4;//we don't want to start right up against the edge so create a buffer zone for starting

            bool[,] stencil = new bool[RowCount, ColumnCount];
            int r = RandomNumGen.Next(rowBuffer, RowCount - (rowBuffer + 1));
            int c = RandomNumGen.Next(colBuffer, ColumnCount - (colBuffer + 1));
            int maxElevationChanges = RandomNumGen.Next((int)((RowCount*ColumnCount)/62.5));//why 1-64? 80*50/62.5=64//using the board dimensions since it can be less than 80 wide by 50 tall


            for (int i = 0; i < maxElevationChanges; i++)
            {
                //create a starter patch
                stencil[r, c] = true;
                stencil[r + 1, c] = true;
                stencil[r, c + 1] = true;

                //now randomly move in a direction
                switch (RandomNumGen.Next(4))
                {
                    case 0: r -= 1; break;
                    case 1: c += 1; break;
                    case 2: r += 1; break;
                    case 3: c -= 1; break;
                }

                //if at any point we're outside our "buffered bounds" break out
                if (r < rowBuffer || c < colBuffer || r > RowCount - (rowBuffer + 1) || c > ColumnCount - (colBuffer + 1))
                    break;
            }
            //return the whole map with a section of tiles as trues
            return stencil;
        }

        /// <summary>
        /// Uses the maps row and column counts to generate a secondary elevation map
        /// </summary>
        /// <returns></returns>
        private int[,] GenerateElevationMap()
        {

            //we need an elevation map to know where ocean,plains,hills,mountains are
            int[,] elevation = new int[RowCount, ColumnCount];

            int landMassSize = (int)(RowCount * ColumnCount / 12.5) * (LandMass + 2);//why 12.5? Why + 2? Taken from civ clone code.

            //so long as we haven't surpased our land mass size, which seems a bit arbitrary keep generating land
            while ((from int tile in elevation where tile > 0 select 1).Sum() < landMassSize)
            {
                int tilesWithLand = (from int tile in elevation where tile > 0 select 1).Sum();
                bool[,] chunk = GenerateLandChunk();

                //loooooops
                for (int r = 0; r < RowCount; r++)
                {
                    for (int c = 0; c < ColumnCount; c++)
                    {
                        if (chunk[r, c])
                        {
                            elevation[r, c]++;
                        }

                    }
                }
            }

            //TODO: remove narrow passages
            //maybe, I kind of like the narrow passages

            return elevation;
        }

        /// <summary>
        /// Create a map of the game board with latitude values
        /// </summary>
        /// <returns></returns>
        private int[,] TemperatureAdjustments()
        {

            int[,] latitude = new int[RowCount, ColumnCount];

            for (int r = 0; r < RowCount; r++)
                for (int c = 0; c < ColumnCount; c++)
                {
                    int l = (int)((float)r / RowCount * RowCount) - (RowCount / 2 + 4);//why 50? why 29? What is this trying to represent?
                    l += RandomNumGen.Next(7);
                    if (l < 0) l = -l;
                    l += 1 - Temperature;//why 1?

                    l = l / 6 + 1;

                    switch (l)//why have this be 8 different options with two options meaning the same thing? Why not just do 4 options?
                    {
                        case 0:
                        case 1: latitude[r, c] = 0; break;
                        case 2:
                        case 3: latitude[r, c] = 1; break;
                        case 4:
                        case 5: latitude[r, c] = 2; break;
                        case 6:
                        default: latitude[r, c] = 3; break;
                    }
                }

            return latitude;
        }

        /// <summary>
        /// Merges an elevation map and a latitude map to fill in the GameTile[,] Board map
        /// </summary>
        /// <param name="elevationMap"></param>
        /// <param name="latitudeMap"></param>
        private void MergeElevationAndLatitudeMaps(int[,] elevationMap, int[,] latitudeMap)
        {

            for (int r = 0; r < RowCount; r++)
                for (int c = 0; c < ColumnCount; c++)
                {
                    //bool special = TileIsSpecial(x, y);
                    //TODO: figure out special tiles

                    //churn through the two maps and make...A WORLD!
                    switch (elevationMap[r, c])
                    {
                        case 0:
                            {
                                Board[r, c] = new GameTile(r, c, LandType.Ocean); break;//new Ocean(x, y, special); break;
                            }
                        case 1:
                            {
                                switch (latitudeMap[r, c])
                                {
                                    case 0: Board[r, c] = new GameTile(r, c, LandType.Desert); break; //_tiles[x, y] = new Desert(x, y, special); break;
                                    case 1: Board[r, c] = new GameTile(r, c, LandType.Plains); break; //_tiles[x, y] = new Plains(x, y, special); break;
                                    case 2: Board[r, c] = new GameTile(r, c, LandType.Tundra); break; //_tiles[x, y] = new Tundra(x, y, special); break;
                                    case 3: Board[r, c] = new GameTile(r, c, LandType.Arctic); break; //_tiles[x, y] = new Arctic(x, y, special); break;
                                }
                            }
                            break;
                        case 2: Board[r, c] = new GameTile(r, c, LandType.Hills); break; //_tiles[x, y] = new Hills(x, y, special); break;
                        default: Board[r, c] = new GameTile(r, c, LandType.Mountains); break; //_tiles[x, y] = new Mountains(x, y, special); break;
                    }
                }
        }


        /// <summary>
        /// Takes the world map and changes tiles based on the world specs
        /// </summary>
        private void ClimateAdjustments()
        {

            int wetness, latitude;

            for (int r = 0; r < RowCount; r++)
            {
                int rr = (int)((float)r / RowCount * RowCount);//why 50?

                //reset wetness for each row
                wetness = 0;
                latitude = Math.Abs(RowCount / 2 - rr);//why 25? Initial civ map height was 50, 25 is half way, or the equator

                for (int c = 0; c < ColumnCount; c++)
                {
                    if (Board[r, c].LandType == LandType.Ocean)
                    {
                        int wy = latitude - RowCount / 4;//why 12?
                        if (wy < 0) wy = -wy;
                        wy += Climate * 4;//why 4?
                        if (wy > wetness) wetness++;
                    }
                    else if (wetness > 0)
                    {
                        //bool special = TileIsSpecial(r, c);
                        int rainfall = RandomNumGen.Next(7 - Climate * 2);//why 2?
                        wetness -= rainfall;

                        switch (Board[r, c].LandType)
                        {
                            case LandType.Plains: Board[r, c].LandType = LandType.Grassland; break;
                            case LandType.Tundra: Board[r, c].LandType = LandType.Arctic; break;
                            case LandType.Hills: Board[r, c].LandType = LandType.Forest; break;
                            case LandType.Desert: Board[r, c].LandType = LandType.Plains; break;
                            case LandType.Mountains: wetness -= 3; break;//why 3?

                        }
                    }
                }

                //now do a second run through but backwards to change things more and differently
                //reset things
                wetness = 0;
                latitude = Math.Abs(RowCount / 2 - rr);

                for (int c = ColumnCount - 1; c > 0; c--)
                {
                    //if we're on an ocean tile pick up water
                    if (Board[r, c].LandType == LandType.Ocean)
                    {
                        int wy = latitude / 2 + Climate;
                        if (wy > wetness) wetness++;
                    }
                    else if (wetness > 0)
                    {
                        //bool special = TileIsSpecial(r, c);
                        int rainfall = RandomNumGen.Next(7 - Climate * 2);//why 7? why 2?
                        wetness -= rainfall;

                        switch (Board[r, c].LandType)
                        {
                            case LandType.Swamp:
                                Board[r, c].LandType = LandType.Forest; break;
                            case LandType.Plains:
                                Board[r, c].LandType = LandType.Grassland; break;
                            case LandType.Grassland:
                                Board[r, c].LandType = LandType.Jungle; break;
                            case LandType.Hills:
                                Board[r, c].LandType = LandType.Forest; break;
                            case LandType.Mountains:
                                Board[r, c].LandType = LandType.Forest; wetness -= 3; break;//why 3?
                            case LandType.Desert:
                                Board[r, c].LandType = LandType.Plains; break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adjusts the age of the tiles on the board/map. It makes more mountains and hills the more times it runs
        /// </summary>
        private void AgeAdjustments()
        {

            //I'm not sure how this does erosion. when we set tile land types we turn 2 flat types into hills and we turn hills into mountains.
            //But we only turn mountains into water if it's not corner next to water.
            //Seems like we're actually going up, not down in land height

            int rRan = 0;
            int cRan = 0;
            int ageRepeat = (int)((float)600 * (1 + Age) / (80 * 50) * (ColumnCount * RowCount));//why 800? Why 80 * 50? That was the size of civ 1 boards, but why hard code it?


            for (int i = 0; i < ageRepeat; i++)
            {
                if (i % 2 == 0)
                {
                    rRan = RandomNumGen.Next(RowCount);
                    cRan = RandomNumGen.Next(ColumnCount);
                }
                else
                {
                    switch (RandomNumGen.Next(8))//why 8? that's the number of potential neighbor tiles?
                    {
                        case 0: { rRan--; cRan--; break; }
                        case 1: { cRan--; break; }
                        case 2: { rRan++; cRan--; break; }
                        case 3: { rRan--; break; }
                        case 4: { rRan++; break; }
                        case 5: { rRan--; cRan++; break; }
                        case 6: { cRan++; break; }
                        default: { rRan++; cRan++; break; }
                    }

                    //keep us in bounds
                    if (cRan < 0) cRan = 1;
                    if (rRan < 0) rRan = 1;
                    if (cRan >= ColumnCount) cRan = ColumnCount - 2;
                    if (rRan >= RowCount) rRan = RowCount - 2;
                }

                //bool special = TileIsSpecial(r,c);
                switch (Board[rRan, cRan].LandType)
                {
                    case LandType.Forest:
                        Board[rRan, cRan].LandType = LandType.Jungle; break;
                    case LandType.Swamp:
                        Board[rRan, cRan].LandType = LandType.Grassland; break;
                    case LandType.Plains:
                        Board[rRan, cRan].LandType = LandType.Hills; break;
                    case LandType.Tundra:
                        Board[rRan, cRan].LandType = LandType.Hills; break;
                    case LandType.Grassland:
                        Board[rRan, cRan].LandType = LandType.Forest; break;
                    case LandType.Jungle:
                        Board[rRan, cRan].LandType = LandType.Swamp; break;
                    case LandType.Hills:
                        Board[rRan, cRan].LandType = LandType.Mountains; break;
                    case LandType.Desert:
                        Board[rRan, cRan].LandType = LandType.Plains; break;
                    case LandType.Arctic:
                        Board[rRan, cRan].LandType = LandType.Mountains; break;
                    case LandType.Mountains://make some mountain lakes?
                        if ((rRan == 0 || Board[rRan - 1, cRan - 1].LandType != LandType.Ocean) &&
                                (cRan == 0 || Board[rRan + 1, cRan - 1].LandType != LandType.Ocean) &&
                            (rRan == RowCount - 1 || Board[rRan + 1, cRan + 1].LandType != LandType.Ocean) &&
                            (cRan == ColumnCount - 1 || Board[rRan - 1, cRan + 1].LandType != LandType.Ocean))
                            Board[rRan, cRan].LandType = LandType.Ocean;
                        break;
                }
            }
        }

        /// <summary>
        /// Creates rivers in the world dependant on the climate and landmass
        /// </summary>
        private void CreateRivers()
        {

            int maxRiverCreationTries = 50;//50 seems like a nice round arbitrary number
            int minRiverLength = 3;//a river of 1 or 2 is too short
            int riverCount = 0;

            for (int i = 0; i < maxRiverCreationTries && riverCount < (Climate + LandMass) * 2 + 6; i++)//haha why *2? why +6?
            {

                //copy the world so we can revert? Also... copy the entire world?
                var tilesBackup = CopyBoard(Board);

                int riverLength = 0;
                int riverDirection = RandomNumGen.Next(4) * 2;
                bool nearOcean;

                GameTile? gameTile = null;


                //find a poor unsuspecting mountain tile that we're later going to turn into a river tile
                var riverStarters = (from GameTile tile in Board where tile.LandType == LandType.Mountains select tile).ToArray();
                int potentialStarts = riverStarters.Length;
                if (potentialStarts == 0)
                {
                    riverStarters = (from GameTile tile in Board where tile.LandType == LandType.Hills select tile).ToArray();
                    potentialStarts = riverStarters.Length;
                }

                gameTile = riverStarters[RandomNumGen.Next(potentialStarts)];

                if (gameTile == null)
                {
                    return;//we don't have any mountains or hills to choose from so bail out
                }

                //build the river until we run into the ocean, another river
                do
                {
                    Board[gameTile.RowIndex, gameTile.ColumnIndex].LandType = LandType.River;
                    int varB = riverDirection;
                    int varC = RandomNumGen.Next(2);
                    riverDirection = (varC - riverLength % 2) * 2 + riverDirection & 0x07;//multiply by 2 makes right angles, no diagonal river movements

                    riverLength++;
                    //Console.WriteLine($"River Length: {riverLength}");

                    nearOcean = IsGameTileNearOcean(gameTile.RowIndex, gameTile.ColumnIndex);
                    switch (riverDirection)
                    {
                        case 0:
                        case 1: gameTile = Board[gameTile.RowIndex, gameTile.ColumnIndex - 1]; break;
                        case 2:
                        case 3: gameTile = Board[gameTile.RowIndex + 1, gameTile.ColumnIndex]; break;
                        case 4:
                        case 5: gameTile = Board[gameTile.RowIndex, gameTile.ColumnIndex + 1]; break;
                        case 6:
                        case 7: gameTile = Board[gameTile.RowIndex - 1, gameTile.ColumnIndex]; break;
                    }
                }
                while (!nearOcean && gameTile.LandType != LandType.Ocean && gameTile.LandType != LandType.River);

                //check our river to make sure it's long enough and ends someplace respectable, if not wipe it and start over
                if ((nearOcean || gameTile.LandType == LandType.River) && riverLength >= minRiverLength)
                {
                    riverCount++;

                    //turn any Forest tiles around the end of our river into jungle, because river endings mean jungle?
                    var tilesAround = Board.GetAllGameTileNeighbors(gameTile.RowIndex, gameTile.ColumnIndex);
                    foreach (var tile in tilesAround)
                    {
                        if (tile != null)
                        {
                            if (tile.LandType == LandType.Forest)
                                tile.LandType = LandType.Jungle;
                        }
                    }
                }
                else
                {
                    //Console.WriteLine($"Resetting River Try");
                    Board = CopyBoard(tilesBackup);//reset it our river sucked
                }
            }
        }

        /// <summary>
        /// Churns through the tiles making some of the special, based on a super secret and advanced formula
        /// </summary>
        private void CreateSpecialTiles()
        {
            for (int r = 0; r < RowCount; r++)
                for (int c = 0; c < ColumnCount; c++)
                {
                    Board[r, c].Special = IsGametileSpecial(r, c);
                }
        }

        /// <summary>
        /// Check the surrounding direct neighbors to see if any are ocean tiles
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool IsGameTileNearOcean(int row, int column)
        {

            var directNeighbors = Board.GetGameTileDirectNeighbors(row, column);
            foreach (var tile in directNeighbors)
            {
                if (tile != null)
                {
                    if (tile.LandType == LandType.Ocean)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Figure out if this tile is a special tile and therefore contains special resources
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool IsGametileSpecial(int row, int column)
        {
            //give a buffer from the top and bottom
            if (row < 2 || row > RowCount - 3) return false;

            //rivers can't be special. they kind of already are
            var tile = Board.GetGameTileAtIndex(row, column);
            if (tile?.LandType == LandType.River) return false;

            return ModTileForSpecial(row, column) == ((column / ResourceAbundance) + 13 + (row / ResourceAbundance) * 11 + _specialSeed) % (ResourceAbundance*4);//some crazy calculation to make special tiles
            
        }

        /// <summary>
        /// Get a mod number for tile to compare to see if it's "special"
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int ModTileForSpecial(int row, int column)
        {
            return (row % ResourceAbundance) * ResourceAbundance + (column % ResourceAbundance);
        }

        /// <summary>
        /// Churn through every tile and make a new tile that has the same coordinates and land type. Now you have a copy with no references pointing to the same place.
        /// </summary>
        /// <param name="theBoard"></param>
        /// <returns></returns>
        private GameTile[,] CopyBoard(GameTile[,] theBoard)
        {
            int rows = theBoard.GetLength(0);
            int cols = theBoard.GetLength(1);
            var copiedBoard = new GameTile[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    GameTile oldGameTile = theBoard[r, c];
                    copiedBoard[r, c] = new GameTile(oldGameTile.RowIndex, oldGameTile.ColumnIndex, oldGameTile.LandType);
                }

            return copiedBoard;

        }
    }
}
