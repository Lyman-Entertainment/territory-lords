using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models
{
    public enum LandType
    {
        Ocean = 0,
        Arctic = 1,
        Desert = 2,
        Forrest = 3,
        Grassland = 4,
        Hills = 5,
        Jungle = 6,
        Mountain = 7,
        Plains = 8,
        River = 9,
        Swamp = 10,
        Tundra = 11
    }

    //this is only for a bit. Need to create an algo to make an actual world rather than just random tiles
    public static class LandTypeFacotry
    {
        public static LandType GetRandomLandType()
        {
            var rnd = new Random();
            return (LandType)rnd.Next(0,12);
        }

        public static LandType GetOcean()
        {
            return LandType.Ocean;
        }
    }
}
