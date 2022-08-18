using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models.Units
{
    public class Malitia : BaseUnit, IUnit
    {
        public Malitia(Player player) : base(player)
        {
            base.Price = 1;
            base.Maintenance = 1;
            base.Attack = 1;
            base.Defense = 1;
            base.MoveDistance = 1;
            base.Active = false;
        }
    }
}
