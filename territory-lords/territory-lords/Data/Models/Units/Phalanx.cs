using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models.Units
{
    public class Phalanx : BaseUnit, IUnit
    {
        public Phalanx() : base()
        {
            base.Price = 2;
            base.Maintenance = 1;
            base.Attack = 1;
            base.Defense = 2;
            base.MoveDistance = 1;
        }
    }
}
