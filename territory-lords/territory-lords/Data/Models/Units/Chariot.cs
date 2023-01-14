using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models.Units
{
    public class Chariot : BaseUnit, IUnit
    {
        public Chariot() : base()
        {
            base.Price = 4;
            base.Maintenance = 1;
            base.Attack = 4;
            base.Defense = 1;
            base.MoveDistance = 2;
        }
    }
}
