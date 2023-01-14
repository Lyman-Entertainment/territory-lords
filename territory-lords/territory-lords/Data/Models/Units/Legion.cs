using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models.Units
{
    public class Legion : BaseUnit, IUnit
    {
        public Legion() : base()
        {
            base.Price = 2;
            base.Maintenance = 1;
            base.Attack = 3;
            base.Defense = 1;
            base.MoveDistance = 1;
        }
    }
}
