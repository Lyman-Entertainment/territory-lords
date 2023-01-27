using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models.Units
{
    public class Calvary : BaseUnit, IUnit
    {
        public Calvary() : base()
        {
            base.Price = 2;
            base.Maintenance = 1;
            base.Attack = 2;
            base.Defense = 1;
            base.MoveDistance = 2;
        }
    }
}
