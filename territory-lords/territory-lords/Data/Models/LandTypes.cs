using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models
{
    public enum LandType
    {
        Ocean = 0,
        Grass = 1,
        Desert = 2,
        Jungle = 3,
        Forrest = 4,
        Hill = 5,
        Mountain = 6
    }

    public static class LandTypeFacotry
    {
        public static LandType GetRandomLandType()
        {
            var rnd = new Random();
            return (LandType)rnd.Next(0,7);
        }
    }
}
