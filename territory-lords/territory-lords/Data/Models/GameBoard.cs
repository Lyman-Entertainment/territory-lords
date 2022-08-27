using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models.Units;

namespace territory_lords.Data.Models
{
    public class GameBoard
    {
        //These should probably be private with some accessors
        public string GameBoardId { get; set; }
        public int RowCount { get; init; }
        public int ColumnCount { get; init; }
        public int LandMass { get; init; }
        public int Temperature { get; init; }
        public int Climate { get; init; }
        public int Age { get; init; }
        public GameTile[,] Board { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        private static readonly Random RandomNumGen = new();
        private ILogger<GameBoard> logger = LoggerFactory.Create(options => { options.AddConsole(); }).CreateLogger<GameBoard>();

        //TODO: handle this differently or more better or something. Creating enums inside this class feels odd
        private enum TileColorsToChooseFrom
        {
            DodgerBlue,
            Pink,
            MediumSeaGreen,
            Khaki,
            LightCoral
        };

        private enum BorderColorsToChooseFrom 
        {
            DeepSkyBlue,
            DeepPink,
            DarkOliveGreen,
            Gold,
            Firebrick
        };

        public GameBoard(string gameBoardId, int rows = 25, int columns = 25, int landMass = 1, int temperature = 1, int climate = 1, int age = 1)
        {
            //Console.WriteLine("Constructing game board");
            //because there is a border of 1 ocean around everything we need to actually make the incoming rows and columns bigger by 2 to make that border
            rows += 2;
            columns += 2;

            //these should only be 1-3, maybe check that here
            LandMass = landMass;
            Temperature = temperature;
            Climate = climate;
            Age = age;

            GameBoardId = gameBoardId;
            RowCount = rows;//do we need this, can't this be figured out by getting the length of the different directions of the Board?
            ColumnCount = columns;
            Board = new GameTile[rows, columns];
        }

        /// <summary>
        /// Initialize the Gameboard, really it builds it
        /// </summary>
        public void InitBoard()
        {
            logger.LogDebug("Initializing Board");
            //Console.WriteLine("Initializing Board");
            //Console.WriteLine($"Rows: {RowCount}");
            //Console.WriteLine($"Columns: {ColumnCount}");
            //Console.WriteLine($"LandMass: {LandMass}");
            //Console.WriteLine($"Temperature: {Temperature}");
            //Console.WriteLine($"Climate: {Climate}"); 
            //Console.WriteLine($"Age: {Age}");
            var elevationMap = GenerateElevationMap();
            var latitudeMap = TemperatureAdjustments();
            MergeElevationAndLatitudeMaps(elevationMap, latitudeMap); 
            var mountains = (from GameTile tile in Board where tile.LandType == LandType.Mountains select 1).Sum();
            //Console.WriteLine($"Mountain Count:{mountains}");
            var hills = (from GameTile tile in Board where tile.LandType == LandType.Hills select 1).Sum();
            //Console.WriteLine($"HIll Count:{hills}");
            ClimateAdjustments();
            AgeAdjustments();
            var allTiles = RowCount * ColumnCount;
            //Console.WriteLine($"allTiles Count:{allTiles}");
            mountains = (from GameTile tile in Board where tile.LandType == LandType.Mountains select 1).Sum();
            //Console.WriteLine($"Mountain Count:{mountains}");
            hills = (from GameTile tile in Board where tile.LandType == LandType.Hills select 1).Sum();
            //Console.WriteLine($"HIll Count:{hills}");
            var arctic = (from GameTile tile in Board where tile.LandType == LandType.Arctic select 1).Sum();
            //Console.WriteLine($"arctic Count:{arctic}");
            var tundra = (from GameTile tile in Board where tile.LandType == LandType.Tundra select 1).Sum();
            //Console.WriteLine($"tundra Count:{tundra}");
            var ocean = (from GameTile tile in Board where tile.LandType == LandType.Ocean select 1).Sum();
            //Console.WriteLine($"ocean Count:{ocean}");
            var plains = (from GameTile tile in Board where tile.LandType == LandType.Plains select 1).Sum();
            //Console.WriteLine($"plains Count:{plains}");
            var grassland = (from GameTile tile in Board where tile.LandType == LandType.Grassland select 1).Sum();
            //Console.WriteLine($"grassland Count:{grassland}");
            var jungle = (from GameTile tile in Board where tile.LandType == LandType.Jungle select 1).Sum();
            //Console.WriteLine($"jungle Count:{jungle}");
            var desert = (from GameTile tile in Board where tile.LandType == LandType.Desert select 1).Sum();
            //Console.WriteLine($"desert Count:{desert}");
            var swamp = (from GameTile tile in Board where tile.LandType == LandType.Swamp select 1).Sum();
            //Console.WriteLine($"swamp Count:{swamp}");
            CreateRivers();
        }

        /// <summary>
        /// Uses the maps row and column counts to generate sections of land for the map at a time
        /// </summary>
        /// <returns></returns>
        private bool[,] GenerateLandChunk()
        {
            logger.LogDebug("Generating land chunks");
            //Console.WriteLine("Generating land chunks");

            //var randomNumGen = new Random();
            int rowBuffer = 3, colBuffer = 4;//we don't want to start right up against the edge so create a buffer zone for starting

            bool[,] stencil = new bool[RowCount, ColumnCount];
            int r = RandomNumGen.Next(rowBuffer, RowCount - (rowBuffer+1));
            int c = RandomNumGen.Next(colBuffer, ColumnCount - (colBuffer+1));
            int maxElevationChanges = RandomNumGen.Next(1, 40);// (int)((RowCount*ColumnCount)/62.5));//why 1-64? 80*50/62.5=64//using the board dimensions since it can be less than 80 wide by 50 tall
            
            logger.LogDebug($"Max Elevation Changes: {maxElevationChanges}");
            //Console.WriteLine($"Max Elevation Changes: {maxElevationChanges}");
            
            for (int i = 0; i < maxElevationChanges; i++)
            {
                //create a starter patch
                stencil[r, c] = true;
                stencil[r + 1, c] = true;
                stencil[r,c + 1] = true;

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
            logger.LogDebug("Generating Elevation map");
            //Console.WriteLine("Generating Elevation map");
            //we need an elevation map to know where ocean,plains,hills,mountains are
            int[,] elevation = new int[RowCount, ColumnCount];
            int maxMountainsForArea = 1;
            
            int landMassSize = (int)((RowCount * ColumnCount)/12.5) * (LandMass + 2);//why 12.5? Why + 2? Taken from civ clone code.
            logger.LogDebug($"Land Mass Size: {landMassSize}");
            //Console.WriteLine($"Land Mass Size: {landMassSize}");
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
            logger.LogDebug("Making Temperature Adjustments");
            //Console.WriteLine("Making Temperature Adjustments");
            int[,] latitude = new int[RowCount,ColumnCount];

            for (int r = 0; r < RowCount; r++)
                for (int c = 0; c < ColumnCount; c++)
                {
                    int l = (int)(((float)r / RowCount) * RowCount) - (RowCount/2 + 4);//why 50? why 29? What is this trying to represent?
                    l += RandomNumGen.Next(7);
                    if (l < 0) l = -l;
                    l += 1 - Temperature;//why 1?

                    l = (l / 6) + 1;

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
            logger.LogDebug("Merging Elevation and Latitude maps");
            //Console.WriteLine("Merging Elevation and Latitude maps");

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
                        case 2:Board[r, c] = new GameTile(r, c, LandType.Hills); break; //_tiles[x, y] = new Hills(x, y, special); break;
                        default: Board[r, c] = new GameTile(r, c, LandType.Mountains); break; //_tiles[x, y] = new Mountains(x, y, special); break;
                    }
                }
        }

        /// <summary>
        /// Takes the world map and changes tiles based on the world specs
        /// </summary>
        private void ClimateAdjustments()
        {
            logger.LogDebug("Adjusting for climate");
            //Console.WriteLine("Adjusting for climate");

            int wetness, latitude;

            for (int r = 0; r < RowCount; r++)
            {
                int rr = (int)(((float)r / RowCount) * RowCount);//why 50?

                //reset wetness for each row
                wetness = 0;
                latitude = Math.Abs(RowCount/2 - rr);//why 25? Initial civ map height was 50, 25 is half way, or the equator

                for (int c = 0; c < ColumnCount; c++)
                {
                    if (Board[r, c].LandType == LandType.Ocean )
                    {
                        int wy = latitude - RowCount/4;//why 12?
                        if (wy < 0) wy = -wy;
                        wy += (Climate * 4);//why 4?
                        if (wy > wetness) wetness++;
                    }
                    else if (wetness > 0)
                    {
                        //bool special = TileIsSpecial(r, c);
                        int rainfall = RandomNumGen.Next(7 - (Climate * 2));//why 2?
                        wetness -= rainfall;

                        switch (Board[r,c].LandType)
                        {
                            case LandType.Plains: Board[r, c].LandType = LandType.Grassland;break;
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
                latitude = Math.Abs(RowCount/2 - rr);

                for (int c = ColumnCount -1; c > 0; c--)
                {
                    //if we're on an ocean tile pick up water
                    if (Board[r,c].LandType == LandType.Ocean)
                    {
                        int wy = (latitude / 2) + Climate;
                        if (wy > wetness) wetness++;
                    }
                    else if (wetness > 0)
                    {
                        //bool special = TileIsSpecial(r, c);
                        int rainfall = RandomNumGen.Next(7 - (Climate * 2));//why 7? why 2?
                        wetness -= rainfall;

                        switch(Board[r,c].LandType)
                        {
                            case LandType.Swamp:
                                Board[r, c].LandType = LandType.Forest;break;
                            case LandType.Plains:
                                Board[r, c].LandType = LandType.Grassland;break;
                            case LandType.Grassland:
                                Board[r, c].LandType = LandType.Jungle;break;
                            case LandType.Hills:
                                Board[r, c].LandType = LandType.Forest;break;
                            case LandType.Mountains:
                                Board[r, c].LandType = LandType.Forest;wetness -= 3;break;//why 3?
                            case LandType.Desert:
                                Board[r, c].LandType = LandType.Plains;break;
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
            logger.LogDebug("Adjusting for Age");
            //Console.WriteLine("Adjusting for Age");

            //I'm not sure how this does erosion. when we set tile land types we turn 2 flat types into hills and we turn hills into mountains.
            //But we only turn mountains into water if it's not corner next to water.
            //Seems like we're actually going up, not down in land height

            int rRan = 0;
            int cRan = 0;
            int ageRepeat = (int)(((float)(600) * (1 + Age) / (80*50)) * (ColumnCount * RowCount));//why 800? Why 80 * 50? That was the size of civ 1 boards, but why hard code it?
            //Console.WriteLine($"Times to repeat age adjustment: {ageRepeat}");

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
                switch(Board[rRan,cRan].LandType)
                {
                    case LandType.Forest:
                        Board[rRan, cRan].LandType = LandType.Jungle;break;
                    case LandType.Swamp:
                        Board[rRan, cRan].LandType = LandType.Grassland; break;
                    case LandType.Plains:
                        Board[rRan, cRan].LandType = LandType.Hills; break;
                    case LandType.Tundra:
                        Board[rRan, cRan].LandType = LandType.Hills; break;
                    //case LandType.River:
                    //    Board[rRan, cRan].LandType = LandType.Forest; break;
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
                        if((rRan == 0 || Board[rRan - 1, cRan -1].LandType != LandType.Ocean) &&
                                (cRan == 0 || Board[rRan + 1, cRan -1].LandType != LandType.Ocean) &&
                            (rRan == ( RowCount - 1) || Board[rRan + 1, cRan + 1].LandType != LandType.Ocean) &&
                            (cRan == ( ColumnCount -1) || Board[rRan -1, cRan + 1].LandType != LandType.Ocean))
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
            logger.LogDebug("Creating Rivers");
            //Console.WriteLine("Creating Rivers");

            int maxRiverCreationTries = 50;//50 seems like a nice round arbitrary number
            int minRiverLength = 3;
            int riverCount = 0;

            for (int i = 0; i < 50 && riverCount < ((Climate + LandMass) * 2) + 6; i++)//haha why 256? why *2? why +6?
            {
                logger.LogDebug($"Attempt #{i}");
                //Console.WriteLine($"Attempt #{i}");

                //copy the world so we can revert? Also... copy the entire world?
                var tilesBackup = CopyBoard(Board);

                int riverLength = 0;
                int riverDirection = RandomNumGen.Next(4) * 2;
                bool nearOcean;

                GameTile? gameTile = null;


                //find a poor unsuspecting hill tile that we're later going to turn into a river tile
                var riverStarters = (from GameTile tile in Board where (tile.LandType == LandType.Mountains) select tile).ToArray();
                int potentialStarts = riverStarters.Length;
                if (potentialStarts == 0)
                {
                    riverStarters = (from GameTile tile in Board where (tile.LandType == LandType.Hills) select tile).ToArray();
                    potentialStarts = riverStarters.Length;
                }

                gameTile = riverStarters[RandomNumGen.Next(potentialStarts)];

                if (gameTile == null)
                {
                    return;//we don't have any mountains or hills to choose from so bail out
                }

                //build the river until we run into the ocean, another river, or a mountain
                do
                {
                    Board[gameTile.RowIndex, gameTile.ColumnIndex].LandType = LandType.River;
                    int varB = riverDirection;
                    int varC = RandomNumGen.Next(2);
                    riverDirection = (((varC - riverLength % 2) * 2 + riverDirection) & 0x07);//multiply by 2 makes right angles, no diagonal river movements

                    riverLength++;
                    //Console.WriteLine($"River Length: {riverLength}");

                    nearOcean = IsGameTileNearOcean(gameTile.RowIndex, gameTile.ColumnIndex);
                    switch(riverDirection)
                    {
                        case 0:
                        case 1: gameTile = Board[gameTile.RowIndex, gameTile.ColumnIndex - 1];break;
                        case 2:
                        case 3: gameTile = Board[gameTile.RowIndex + 1, gameTile.ColumnIndex];break;
                        case 4:
                        case 5: gameTile = Board[gameTile.RowIndex, gameTile.ColumnIndex + 1];break;
                        case 6:
                        case 7: gameTile = Board[gameTile.RowIndex - 1, gameTile.ColumnIndex];break;
                    }
                }
                while (!nearOcean && (gameTile.LandType != LandType.Ocean && gameTile.LandType != LandType.River ));//&& gameTile.LandType != LandType.Mountains

                //check our river to make sure it's long enough and ends someplace respectable, if not wipe it and start over
                if ((nearOcean || gameTile.LandType == LandType.River) && riverLength >= minRiverLength)//arbitrary minlength for rivers
                {
                    riverCount++;
                    //Console.WriteLine($"River Count: {riverCount}");
                    //turn any Forest tiles around the end of our river into jungle, because river endings mean jungle?
                    var tilesAround = GetAllGameTileNeighbors(gameTile);
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
        /// Check the surrounding direct neighbors to see if any are ocean tiles
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private bool IsGameTileNearOcean(int row, int column)
        {
            var centerTile = GetGameTileAtIndex(row, column);
            if (centerTile != null)
            {
                var directNeighbors = GetGameTileDirectNeighbors(centerTile);
                foreach (var tile in directNeighbors)
                {
                    if(tile != null)
                    {
                        if (tile.LandType == LandType.Ocean)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Return a game tile at a row and column index.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns>GameTile object. Null if one doesn't exist</returns>
        public GameTile? GetGameTileAtIndex(int rowIndex, int columnIndex)
        {
            //we're outside the board bounds
            if (rowIndex < 0 || columnIndex < 0 || rowIndex > RowCount - 1 || columnIndex > ColumnCount - 1)
            {
                return null;
            }

            //otherwise return the game tile at that location
            return Board[rowIndex, columnIndex];
        }

        /// <summary>
        /// Get the NESW cardinal direction neighbors of a gameTile
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns>GameTile[4] starting at 12 o clock or North</returns>
        public GameTile[] GetGameTileDirectNeighbors(GameTile gameTile)
        {

            var neighbors = new GameTile[4];
            //get to the north
            neighbors[0] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex);
            //get to the east
            neighbors[1] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex + 1);
            //get to the south
            neighbors[2] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex);
            //get to the west
            neighbors[3] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex - 1);

            return neighbors;
        }

        /// <summary>
        /// Get the corner neighbors
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns>GameTile[4] starting at top right or North-West corner</returns>
        public GameTile[] GetGameTileCornerNeighbors(GameTile gameTile)
        {
            var neighbors = new GameTile[4];
            //top right
            neighbors[0] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex + 1);
            //bottom right
            neighbors[1] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex + 1);
            //bottom left
            neighbors[2] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex - 1);
            //top left
            neighbors[3] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex - 1);
            return neighbors;
        }

        public GameTile[] GetAllGameTileNeighbors(GameTile gameTile)
        {
            var neighbors = new GameTile[8];
            //get to the north
            neighbors[0] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex);
            //top right
            neighbors[1] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex + 1);
            //get to the east
            neighbors[2] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex + 1);
            //bottom right
            neighbors[3] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex + 1);
            //get to the south
            neighbors[4] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex);
            //bottom left
            neighbors[5] = GetGameTileAtIndex(gameTile.RowIndex - 1, gameTile.ColumnIndex - 1);
            //get to the west
            neighbors[6] = GetGameTileAtIndex(gameTile.RowIndex, gameTile.ColumnIndex - 1);
            //top left
            neighbors[7] = GetGameTileAtIndex(gameTile.RowIndex + 1, gameTile.ColumnIndex - 1);

            return neighbors;
        }

        /// <summary>
        /// Exactly what you think it is
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        /// <summary>
        /// Adds a player to the game if they don't already exist
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public Player? AddPlayerToGame(Guid playerId, string playerName)
        {
            //if they don't exist then add them
            if (!Players.Any(p => p.Id == playerId))
            {

                Player newPlayer = new Player
                {
                    Id = playerId,
                    Name = playerName,
                    Colors = GetNextAvailableColors()
                };
                Players.Add(newPlayer);
                return newPlayer;
            }
            return null;
        }

        /// <summary>
        /// Just a silly "random" unit generator for now. Just testing purposes
        /// </summary>
        /// <returns></returns>
        private IUnit? GetRandomUnitType()
        {
            
            var rnd = new Random().Next(40);

            //grab one of the players
            var tempPlayer = rnd % 2 == 0 ? Players[0] : Players[1]; 
            var unitList = new List<IUnit> {
                new Malitia(tempPlayer)
                ,new Phalanx(tempPlayer)
                ,new Legion(tempPlayer)
                ,new Calvary(tempPlayer)
                ,new Chariot(tempPlayer)
            };

            //a chinsy way to only put units on some of the tiles since there are only 5 unit types but our random goes to 40. That's 5/40 or ~1 out of every 8 that will have a unit
            if (rnd < unitList.Count)
            {
                return unitList[rnd];
            }
            return null;
        }

        /// <summary>
        /// Gets a new set of colors based on how many players are in the game
        /// </summary>
        /// <returns></returns>
        private PlayerColors GetNextAvailableColors()
        {
            //TODO:This will fail if there are more than 3 players or more players than colors. Also it's always going to be the same colors in that order and that's lame.
            return new PlayerColors { 
                TileColor = ((TileColorsToChooseFrom) Players.Count).ToString(), 
                BorderColor = ((BorderColorsToChooseFrom)Players.Count).ToString()
            };
        }


        private GameTile[,] CopyBoard(GameTile[,] theBoard)
        {
            int rows = theBoard.GetLength(0);
            int cols = theBoard.GetLength(1);
            var copiedBoard = new GameTile[rows, cols];
            for(int r = 0;r < rows;r++)
                for(int c = 0; c < cols;c++)
                {
                    GameTile oldGameTile = theBoard[r, c];
                    copiedBoard[r,c] = new GameTile(oldGameTile.RowIndex, oldGameTile.ColumnIndex, oldGameTile.LandType);
                }

            return copiedBoard;

        }
    }
}
